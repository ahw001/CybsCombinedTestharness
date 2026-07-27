# 3-D Secure Challenge Round-Trip — Manual Test Checklist

Run this checklist after **every deploy** to the `*.replit.app` production URL.  
The goal is to confirm that the PA Check Enrollment → Step Up → `/stepup-complete` round-trip  
completes automatically — without the "Continue to Validate" fallback button — and that no CORS  
errors appear in the browser console for the ACS iframe POST.

---

## Pre-flight: Verify the deploy is healthy

1. Open the deployed app (e.g. `https://<repl-name>.replit.app/`).
2. Navigate to **`/api/diag/stepup-config`** and verify:
   - `baseUri` starts with **`https://`** (not `http://`) — ForwardedHeaders are working.
   - `returnUrl` is `https://<repl-name>.replit.app/stepup-callback` — matches the public origin.
   - `corsAllowedOrigins` contains at least one `*.replit.app` entry.
   - `extraCorsOrigin` is non-null if `EXTRA_CORS_ORIGIN` was set at startup (optional).

   > **If `baseUri` shows `http://`** — ForwardedHeaders are not being applied. Check  
   > that `ASPNETCORE_URLS` is set to `http://0.0.0.0:5000` (not an https port) and  
   > that `UseForwardedHeaders` is configured before `UseRouting` in `Program.cs`.

---

## Step 1 — Capture a Transient Token

1. Go to **Home** → start a new transaction (e.g. Unified Checkout).
2. Enter test card details for a **3-D Secure enrolled card** (see CyberSource's Payer Auth test cards — look for cards that return `veresEnrolled = Y`, `paresStatus = C`).
3. Submit the tokenize form.  
   Confirm the session shows a `TransientToken` in the response data card.

---

## Step 2 — PA Check Enrollment

1. Navigate to **`/pacheckenroll`** (Payer Auth Check Enrollment).
2. Confirm the pre-filled `ReturnUrl` in the response data list reads:  
   `https://<repl-name>.replit.app/stepup-callback`  
   (The page displays all `CheckEnrollDto` fields — scan the list for `ReturnUrl`.)
3. Click **SUBMIT FOR CHECK ENROLLMENT**.
4. Confirm the next page is **`/paprocessor`** and the response contains:
   - `consumerAuthenticationInformation.veresEnrolled = "Y"`
   - `consumerAuthenticationInformation.paresStatus = "C"`
   - A non-empty `stepUpUrl` (Cardinal's ACS URL)
   - A non-empty `accessToken` (JWT)

---

## Step 3 — Step Up form submission

1. The app should automatically navigate to **`/pastepup`** (or follow the menu link).
2. Confirm the Step Up iframe appears and the `step-up-form` is submitted automatically  
   (browser console should log `Cardinal Step Up Form found — submitting form.`).
3. The ACS challenge UI should render inside the iframe.

---

## Step 4 — Complete the ACS challenge

1. Complete the challenge inside the iframe (e.g. enter the OTP sent by the test card issuer,  
   or click through the test-mode challenge page).
2. Watch the browser console — **no CORS errors should appear**.  
   Specifically, there must be no messages like:
   - `Access to XMLHttpRequest … blocked by CORS policy`
   - `Cross-Origin-Resource-Policy: cross-origin` blocked
   - `Failed to load resource: net::ERR_BLOCKED_BY_CLIENT` on `/stepup-callback`

   > The ACS iframe POSTs to `https://<repl-name>.replit.app/stepup-callback`.  
   > The `/stepup-callback` endpoint returns an HTML page with  
   > `window.top.location.href = '/stepup-complete?transactionId=…&guid=…'`  
   > which navigates the **top frame** (the Blazor app) — CORS is not involved  
   > in that navigation. If CORS errors appear for the POST itself, the `AllowedOrigins`  
   > list does not include the ACS origin.

---

## Step 5 — Automatic redirect to `/stepup-complete`

1. After the challenge completes, the top-level page should navigate to  
   **`/stepup-complete?transactionId=…&guid=…`** automatically (within ~2 seconds).
2. ✅ **The "Continue to Validate" button must NOT be needed.**  
   If the user has to click it, the automatic redirect fired either:
   - `window.top.location.href` was blocked by the browser (unlikely on `*.replit.app`; this  
     only happens when the app runs on `localhost` behind Chrome's Local Network Access policy).
   - The ACS iframe POST reached `/stepup-callback` but `TransactionId` or `MD` were empty  
     (check the query string on `/stepup-complete` — both params must be non-empty).

---

## Step 6 — PA Validate

1. Confirm `/stepup-complete` loads and calls PA Validate automatically (or via the next step  
   in the UI flow).
2. The validate response should contain `authenticationResult` / `cavv` / `eci` values.
3. Navigate to the authorization step and confirm it succeeds.

---

## Post-test: Sign-off table

| # | Check | Pass / Fail | Notes |
|---|-------|-------------|-------|
| 0 | `/api/diag/stepup-config` → `baseUri` is `https://` | | |
| 0 | `returnUrl` matches `<repl-name>.replit.app/stepup-callback` | | |
| 1 | Transient token captured | | |
| 2 | Check Enrollment returns `veresEnrolled=Y`, `paresStatus=C` | | |
| 2 | `ReturnUrl` in response data is the public `https://` URL | | |
| 3 | Step Up iframe renders and form auto-submits | | |
| 4 | No CORS errors in browser console | | |
| 5 | Top frame redirects to `/stepup-complete` automatically | | |
| 5 | "Continue to Validate" button NOT clicked | | |
| 6 | PA Validate succeeds with `cavv`/`eci` | | |

---

## What breaks the round-trip and how to fix it

| Symptom | Root cause | Fix |
|---------|-----------|-----|
| `baseUri` is `http://` | `ForwardedHeaders` middleware missing or ordering wrong | Ensure `app.UseForwardedHeaders(…)` is first in the pipeline (`Program.cs`) |
| `ReturnUrl` points to `http://` or `localhost` | Same as above | Same fix; ForwardedHeaders propagate the `X-Forwarded-Proto: https` header from the Replit proxy |
| CORS error on ACS → `/stepup-callback` POST | ACS origin not in `AllowedOrigins` | Add the Cardinal/ACS origin to `Cors:AllowedOrigins` in `appsettings.json`, or set `EXTRA_CORS_ORIGIN` at runtime |
| `window.top.location.href` doesn't fire | Browser sandbox policy blocks top-frame navigation from cross-origin iframe | Usually a browser bug / strict CSP; check `Content-Security-Policy` headers on the response from `/stepup-callback` — remove `frame-ancestors` restrictions if present |
| Auto-redirect fires but `transactionId` or `guid` are empty | ACS posted `TransactionId`/`MD` with different casing or field name | Check the raw POST body in `/stepup-callback` logs; adjust the form field names in the endpoint |
| "Continue to Validate" needed only on `localhost` | Chrome Local Network Access policy | Expected behaviour on localhost; the checklist applies to the deployed `*.replit.app` URL only |

---

## Key files that affect this flow

| File | What it controls |
|------|-----------------|
| `CybsClient/Components/Pages/PayerAuthentication/PaCheckEnrollment.razor` line 151 | `ReturnUrl` construction — must produce the public `https://` host |
| `CybsClient/Components/Pages/PayerAuthentication/PaStepUp.razor` | Step-up iframe, form auto-submit, `ContinueToValidate` fallback |
| `CybsClient/Program.cs` — `app.MapPost("/stepup-callback", …)` | Receives ACS POST, emits `window.top.location.href` redirect HTML |
| `CybsClient/Program.cs` — `UseForwardedHeaders` | Makes `NavigationManager.BaseUri` (and therefore `ReturnUrl`) reflect the real public `https://` origin |
| `CybsClient/appsettings.json` — `Cors:AllowedOrigins` | Controls which origins may POST to `/stepup-callback` |
| `EXTRA_CORS_ORIGIN` env var | Runtime escape hatch to add one more CORS origin without touching config |
