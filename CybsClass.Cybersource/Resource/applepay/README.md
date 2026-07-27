# Apple Pay Payment Processing Certificate

Drop the merchant-decryption key pair here (test environment):

- `apple_pay_payment_processing.pem` — the Payment Processing Certificate downloaded from your
  Apple Developer Merchant ID (public cert; informational only, not loaded by `ApplePayCredentials`).
- `apple_pay_payment_processing_key.pem` — the PRIVATE KEY you generated the CSR from. This is the
  actual merchant-decryption key loaded by `ApplePayCredentials.Initialize` (see
  `CybsClass.WebApi.Service\appsettings.json` → `ApplePaySettings`).

These files are never committed with real key material in a shared repo — treat this folder the
same way `Resource\key\private.pem` (legacy MLE) and `Resource\mle\*` are already treated in this
project: test-only credentials, local to this machine.
