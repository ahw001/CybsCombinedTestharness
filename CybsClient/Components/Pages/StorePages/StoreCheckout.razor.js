// Unified Checkout (v1 JS library) mount flow for the store checkout page.
// UnifiedCheckoutPlanning.md, Phase 0 + Execution Goal 1 — deliberately NOT the deprecated
// v0 pattern used by Components/Pages/UnifiedCheckout/UnifiedForm.razor.js (Accept(jwt)/
// unifiedPayments()/show()).
//
// Real v1 API surface (confirmed live 2026-07-21/22 via direct browser console probing AND by
// downloading and reading the actual minified UnifiedCheckout.js bundle — CyberSource's public
// docs describe a differently-shaped API that does not match what VAS.UnifiedCheckout actually
// returns):
//
//   const client   = await VAS.UnifiedCheckout(jwt)
//   const checkout = await client.createCheckout()   // takes NO args — a selector here is
//                                                     // silently ignored, not an error
//
//   Valid checkout.on(...)/off(...) event names (confirmed from the SDK's own `Kr` constant —
//   there is NO "complete"/"success" event):
//     mounted, ready, unready, unmounted, destroyed,
//     paymentMethodSelected, paymentMethodCancelled, paymentMethodUpdate, error
//
//   checkout.mount({ paymentSelection: '#id', paymentScreen: '#id' })
//     - BOTH keys required; omitting either throws UnifiedCheckoutError
//       (reason: MOUNT_INVALID_CONTAINER).
//     - This account has a merchant-level completeMandate default (confirmed live: the decoded
//       capture-context JWT already contains a completeMandate object — {type: "CAPTURE",
//       decisionManager: "SKIP", ...} — even though our request never sends one), which puts the
//       SDK in "autoProcessing" mode.
//     - checkout.complete(...) THROWS ("Checkout.complete() cannot be called when autoProcessing
//       is enabled") in this mode — do not call it. Confirmed by reading the SDK source:
//       `complete(e){ if(l(),i) throw Error("Checkout.complete() cannot be called when
//       autoProcessing is enabled"); ... }` where `i` is the autoProcessing flag.
//     - Per the SDK source, the customer-driven internal completion handler does
//       `return i ? await s.complete(e) : e` — i.e. when autoProcessing is on, the mount()
//       promise itself does not resolve until the customer finishes the full wizard AND
//       CyberSource has auto-processed the payment (confirmed live: a real POST to
//       testflex.cybersource.com/flex/v2/complete returned 201 while mount() was still pending).
//       So: await mount() for the final, already-processed result — do NOT poll isMounted() for
//       "done" (isMounted() only reports whether the UI is currently displayed, not whether the
//       transaction is finished) and do NOT show a separate "Complete Purchase" button.
//
//   checkout.{unmount, isMounted() [function, not property], on, off, destroy, isDestroyed()}

window.storeCheckoutUC = window.storeCheckoutUC || {};

// ── Clipboard helper for the test-value panels ───────────────────────────────────────────
// The CyberSource widget renders inside cross-origin iframes, so a test card number or test
// bank account can only be transferred into it by hand. This copies one value on click.
// navigator.clipboard requires a secure context — https://localhost qualifies, as does every
// deployed origin — and the execCommand path covers anything that does not.
window.storeCheckoutUC.copyText = async function (text) {
    if (!text) {
        return false;
    }

    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch (e) {
        console.warn("[StoreCheckoutUC] navigator.clipboard.writeText failed, falling back:", e);
    }

    try {
        var textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.setAttribute('readonly', '');
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        document.body.appendChild(textArea);
        textArea.select();
        var copied = document.execCommand('copy');
        document.body.removeChild(textArea);
        return copied;
    } catch (e) {
        console.error("[StoreCheckoutUC] clipboard fallback failed:", e);
        return false;
    }
};

// manualToken=true asks the v1 SDK for createCheckout({ autoProcessing: false }) — the UC
// guide's "Flow 3 / No Service Orchestration" mode: mount() then resolves with the raw
// transientTokenJwt and NOTHING is processed; every follow-on service is the merchant's own
// REST call. Live-verified 2026-07-28: this client-side option overrides the merchant-level
// Business Center completeMandate default (which otherwise forces autoProcessing on this
// account) — despite our 2026-07-21 note that createCheckout() ignores its arguments (that
// probe passed a selector, not an options object).
window.storeCheckoutUC.loadScriptAndMount = function (dotNetRef, jwt, clientLibraryUrl, containerId, manualToken) {
    console.log("[StoreCheckoutUC] clientLibraryUrl:", clientLibraryUrl, "manualToken:", !!manualToken);

    if (!clientLibraryUrl) {
        console.error("[StoreCheckoutUC] No clientLibrary URL found in the capture context JWT.");
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'No clientLibrary URL found in the capture context JWT.');
        return;
    }

    var existing = document.querySelector('script[data-uc-client-library]');
    if (existing) {
        existing.remove();
    }

    var script = document.createElement('script');
    script.src = clientLibraryUrl;
    script.setAttribute('data-uc-client-library', 'true');
    script.onload = function () {
        console.log("[StoreCheckoutUC] client library script loaded.");
        window.storeCheckoutUC.mount(dotNetRef, jwt, containerId, manualToken);
    };
    script.onerror = function () {
        console.error("[StoreCheckoutUC] Failed to load client library script:", clientLibraryUrl);
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'Failed to load Unified Checkout client library script.');
    };
    document.head.appendChild(script);
};

// ── v0 manual-token path (SecureAcceptance.js, /up/v1/capture-contexts ctx) ─────────────
// Unlike the v1 flow above, the v0 widget never auto-processes: show()'s promise resolves
// with the raw transientTokenJwt once the customer confirms, and the page then submits the
// follow-on /pts/v2/payments server-side. Same surface the legacy UnifiedForm.razor uses
// (Accept(jwt) → accept.unifiedPayments() → up.show({containers:{paymentSelection}})).
window.storeCheckoutUC.loadV0ScriptAndMount = function (dotNetRef, jwt, clientLibraryUrl, containerId) {
    console.log("[StoreCheckoutUC:v0] clientLibraryUrl:", clientLibraryUrl);

    if (!clientLibraryUrl) {
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'No clientLibrary URL found in the v0 capture context JWT.');
        return;
    }

    var existing = document.querySelector('script[data-uc-client-library-v0]');
    if (existing) {
        existing.remove();
    }

    var script = document.createElement('script');
    script.src = clientLibraryUrl;
    script.setAttribute('data-uc-client-library-v0', 'true');
    script.onload = function () {
        console.log("[StoreCheckoutUC:v0] SecureAcceptance client library loaded.");
        window.storeCheckoutUC.mountV0(dotNetRef, jwt, containerId);
    };
    script.onerror = function () {
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'Failed to load the v0 SecureAcceptance client library script.');
    };
    document.head.appendChild(script);
};

window.storeCheckoutUC.mountV0 = function (dotNetRef, jwt, containerId) {
    if (typeof Accept !== 'function') {
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'Accept() is not available after SecureAcceptance script load.');
        return;
    }

    Accept(jwt).then(function (accept) {
        console.log("[StoreCheckoutUC:v0] accept:", accept);
        return accept.unifiedPayments();
    }).then(function (up) {
        console.log("[StoreCheckoutUC:v0] unifiedPayments instance:", up);
        // The widget is rendering at this point; show()'s promise stays pending until the
        // customer confirms, then resolves with the transient token (no auto-processing in v0).
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutReady');
        return up.show({
            containers: {
                paymentSelection: '#' + containerId
            }
        });
    }).then(function (transientToken) {
        console.log("[StoreCheckoutUC:v0] show() resolved with transient token.");
        dotNetRef.invokeMethodAsync('ReceiveV0TransientToken', String(transientToken));
    }).catch(function (err) {
        console.error("[StoreCheckoutUC:v0] mount flow failed:", err);
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', String(err && err.message ? err.message : err));
    });
};

window.storeCheckoutUC.mount = function (dotNetRef, jwt, containerId, manualToken) {
    if (typeof VAS === 'undefined' || typeof VAS.UnifiedCheckout !== 'function') {
        console.error("[StoreCheckoutUC] VAS.UnifiedCheckout is not available after script load.");
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', 'VAS.UnifiedCheckout is not available after script load.');
        return;
    }

    VAS.UnifiedCheckout(jwt).then(function (client) {
        console.log("[StoreCheckoutUC] client:", client);
        window.storeCheckoutUC._client = client;

        // Flow 3 (manual token): autoProcessing:false makes mount() resolve with the
        // transientTokenJwt instead of an auto-processed result.
        return manualToken ? client.createCheckout({ autoProcessing: false }) : client.createCheckout();
    }).then(function (checkout) {
        console.log("[StoreCheckoutUC] checkout:", checkout);
        window.storeCheckoutUC._checkout = checkout;

        if (checkout && typeof checkout.on === 'function') {
            checkout.on('error', function (err) {
                console.error("[StoreCheckoutUC] checkout error event:", err);
                dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', String(err && err.message ? err.message : err));
            });
            checkout.on('ready', function () {
                console.log("[StoreCheckoutUC] checkout ready event — widget is interactive.");
                dotNetRef.invokeMethodAsync('OnUnifiedCheckoutReady');
            });
        }

        // This promise resolves only once the customer completes the whole embedded wizard and
        // (given this merchant's default completeMandate/autoProcessing) CyberSource has already
        // processed the payment — see the notes above. It can be pending for as long as the
        // customer takes to fill out the form.
        return checkout.mount({
            paymentSelection: '#' + containerId,
            paymentScreen: '#' + containerId
        });
    }).then(function (result) {
        console.log("[StoreCheckoutUC] mount() resolved — checkout complete:", result);
        var raw;
        try {
            raw = JSON.stringify(result);
        } catch (e) {
            raw = String(result);
        }
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutComplete', raw);
    }).catch(function (err) {
        console.error("[StoreCheckoutUC] mount flow failed:", err);
        dotNetRef.invokeMethodAsync('OnUnifiedCheckoutError', String(err && err.message ? err.message : err));
    });
};
