# Apple Pay Merchant ID Domain Verification

Drop the domain-association file you download from the Apple Developer portal (Merchant ID →
Merchant Domains → add `www.ahw001.com` → Download) directly into this folder, named exactly:

```
apple-developer-merchantid-domain-association
```

(no file extension). Once deployed to Azure, it must be reachable at:

```
https://www.ahw001.com/.well-known/apple-developer-merchantid-domain-association
```

This is **not** the same URL as the Apple Pay checkout page (`/applepaycheckout`) — Apple's
servers fetch this exact fixed path to verify domain ownership before the domain can use the
Merchant ID at all; it has nothing to do with the app's routes.

`Program.cs` registers a second `UseStaticFiles()` scoped to `/.well-known` with
`ServeUnknownFileTypes = true`, because the default static-file middleware refuses to serve files
with no recognized extension (which is exactly what this file is).

Do not rename or add an extension to the file Apple gives you — the filename above is fixed by
Apple's own domain-verification protocol.
