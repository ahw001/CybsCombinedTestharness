// Apple Pay JS (web) — merchant decryption flow.
// NOTE: window.ApplePaySession is a native Safari API; only Safari on an Apple-verified HTTPS
// domain will ever satisfy canMakePayments(). This will not run on localhost or any browser
// other than Safari — see the "Local testing" decision in ApplePayWebPlanning.md.
//
// USER-GESTURE CONSTRAINT (important): WebKit requires `new ApplePaySession(...)` to be
// constructed synchronously inside a user-gesture handler, otherwise it throws
// InvalidAccessError ("Must create a new ApplePaySession from a user gesture handler").
// A Blazor @onclick cannot satisfy this — the tap travels to the server over SignalR and comes
// back through JS interop, by which point the gesture context is gone. That is why the click is
// wired here with a native addEventListener (attachApplePayButton) instead: the session is
// created in the DOM handler itself, and only the async callbacks (onvalidatemerchant /
// onpaymentauthorized) go back into .NET.

let applePayConfig = null;
let applePayDotNetRef = null;

function applePayIsAvailable() {
    return typeof window.ApplePaySession !== 'undefined' && ApplePaySession.canMakePayments();
}

// Apple's apple-pay-sdk.js supplies the <apple-pay-button> custom element. Until that script
// loads the tag is an unknown element with no shadow DOM — it lays out as a 0x0 invisible box
// with nothing to tap. Resolves true once the element is defined, false if the CDN never
// answers, so the page can fall back to a plain Bootstrap button instead of rendering nothing.
function applePayButtonElementReady(timeoutMs) {
    if (typeof window.customElements === 'undefined') {
        return Promise.resolve(false);
    }

    if (customElements.get('apple-pay-button')) {
        return Promise.resolve(true);
    }

    return Promise.race([
        customElements.whenDefined('apple-pay-button').then(function () { return true; }),
        new Promise(function (resolve) {
            setTimeout(function () { resolve(false); }, timeoutMs || 5000);
        })
    ]);
}

// Called from OnAfterRenderAsync once the button is in the DOM. Idempotent — repeat calls only
// refresh the amount/currency, they never double-wire the listener.
function attachApplePayButton(dotNetRef, buttonId, amount, currencyCode, countryCode, merchantIdentifier, label) {
    const button = document.getElementById(buttonId);

    if (!button) {
        console.error('[ApplePay] Button element not found: ' + buttonId);
        return false;
    }

    applePayDotNetRef = dotNetRef;
    applePayConfig = {
        amount: amount,
        currencyCode: currencyCode,
        countryCode: countryCode,
        merchantIdentifier: merchantIdentifier,
        label: label
    };

    if (button.dataset.applePayWired === 'true') {
        return true;
    }

    button.dataset.applePayWired = 'true';
    button.addEventListener('click', startApplePaySessionFromGesture);
    return true;
}

// Runs inside the native click handler — everything up to session.begin() must stay synchronous.
function startApplePaySessionFromGesture() {
    if (!applePayConfig || !applePayDotNetRef) {
        console.error('[ApplePay] Button clicked before attachApplePayButton completed.');
        return;
    }

    if (!applePayIsAvailable()) {
        applePayDotNetRef.invokeMethodAsync('OnApplePayUnavailable');
        return;
    }

    const request = {
        countryCode: applePayConfig.countryCode,
        currencyCode: applePayConfig.currencyCode,
        merchantCapabilities: ['supports3DS'],
        supportedNetworks: ['visa', 'masterCard', 'amex', 'discover'],
        total: { label: applePayConfig.label, amount: applePayConfig.amount }
    };

    let session;

    try {
        session = new ApplePaySession(3, request);
    } catch (err) {
        const message = err && err.message ? err.message : String(err);
        console.error('[ApplePay] Could not create ApplePaySession:', err);
        applePayDotNetRef.invokeMethodAsync('OnApplePaySessionError', message);
        return;
    }

    session.onvalidatemerchant = function (event) {
        // Server-side proxy performs the actual mutual-TLS call to Apple's validationURL using
        // the Merchant Identity Certificate — that certificate must never reach the browser.
        // OnMerchantValidationNeeded returns the merchant session JSON as a string, or null on
        // failure.
        applePayDotNetRef.invokeMethodAsync('OnMerchantValidationNeeded', event.validationURL)
            .then(function (merchantSessionJson) {
                if (merchantSessionJson) {
                    const merchantSession = JSON.parse(merchantSessionJson);
                    session.completeMerchantValidation(merchantSession);
                } else {
                    console.error('[ApplePay] Merchant validation failed — aborting session.');
                    session.abort();
                }
            })
            .catch(function (err) {
                console.error('[ApplePay] OnMerchantValidationNeeded threw:', err);
                session.abort();
            });
    };

    session.onpaymentauthorized = function (event) {
        try {
            const token = event.payment.token;
            const paymentDataJson = JSON.stringify(token.paymentData);
            const network = token.paymentMethod && token.paymentMethod.network ? token.paymentMethod.network : '';

            applePayDotNetRef.invokeMethodAsync('OnApplePayAuthorized', paymentDataJson, network)
                .then(function (authorized) {
                    session.completePayment(authorized
                        ? ApplePaySession.STATUS_SUCCESS
                        : ApplePaySession.STATUS_FAILURE);
                })
                .catch(function (err) {
                    console.error('[ApplePay] OnApplePayAuthorized failed:', err);
                    session.completePayment(ApplePaySession.STATUS_FAILURE);
                });
        } catch (err) {
            console.error('[ApplePay] onpaymentauthorized error:', err);
            session.completePayment(ApplePaySession.STATUS_FAILURE);
        }
    };

    session.oncancel = function () {
        applePayDotNetRef.invokeMethodAsync('OnApplePayCancelled');
    };

    try {
        session.begin();
    } catch (err) {
        const message = err && err.message ? err.message : String(err);
        console.error('[ApplePay] session.begin() failed:', err);
        applePayDotNetRef.invokeMethodAsync('OnApplePaySessionError', message);
    }
}
