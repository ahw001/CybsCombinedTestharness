// Apple Pay JS (web) — merchant decryption flow.
// NOTE: window.ApplePaySession is a native Safari API; only Safari on an Apple-verified HTTPS
// domain will ever satisfy canMakePayments(). This will not run on localhost or any browser
// other than Safari — see the "Local testing" decision in ApplePayWebPlanning.md.

function applePayIsAvailable() {
    return typeof window.ApplePaySession !== 'undefined' && ApplePaySession.canMakePayments();
}

function beginApplePaySession(dotNetRef, amount, currencyCode, countryCode, merchantIdentifier, label) {
    if (!applePayIsAvailable()) {
        dotNetRef.invokeMethodAsync('OnApplePayUnavailable');
        return;
    }

    const request = {
        countryCode: countryCode,
        currencyCode: currencyCode,
        merchantCapabilities: ['supports3DS'],
        supportedNetworks: ['visa', 'masterCard', 'amex', 'discover'],
        total: { label: label, amount: amount }
    };

    const session = new ApplePaySession(3, request);

    session.onvalidatemerchant = function (event) {
        // Server-side proxy performs the actual mutual-TLS call to Apple's validationURL using
        // the Merchant Identity Certificate — that certificate must never reach the browser.
        // OnMerchantValidationNeeded returns the merchant session JSON as a string, or null on
        // failure.
        dotNetRef.invokeMethodAsync('OnMerchantValidationNeeded', event.validationURL)
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

            dotNetRef.invokeMethodAsync('OnApplePayAuthorized', paymentDataJson, network)
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
        dotNetRef.invokeMethodAsync('OnApplePayCancelled');
    };

    session.begin();
}
