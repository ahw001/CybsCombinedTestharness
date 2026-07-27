# CybsCombinedTestharness — Replit Notes

## Project Overview

Single-host .NET 10 Blazor Server + Minimal API test harness for CyberSource payments.

- **CybsClient/** — Blazor Server front-end (entry point, port 5000)
- **CybsClass.WebApi.Service/** — ASP.NET Core Minimal API (CyberSource transactions, SQLite)
- **CybsClass.DataContext.Sqlite/**, **CybsClass.EntityModels.Sqlite/**, **CybsClass.Cybersource/** — support libraries

## Running on Replit

The workflow resolves `dotnet` at startup to avoid breakage if the nix store hash changes (e.g. on SDK upgrades):

```
sh -c 'DOTNET=$(which dotnet 2>/dev/null || find /nix/store -name dotnet -path "*/dotnet-sdk-wrapped*" | head -1) \
  && cd CybsClient && ASPNETCORE_URLS=http://0.0.0.0:5000 $DOTNET run --no-launch-profile'
```

When Replit upgrades to .NET 10.0.300+, `which dotnet` will resolve correctly and the `find` fallback will be unused — at that point the command can be simplified to `dotnet run`.

## SDK Notes

- Replit provides .NET SDK **10.0.101** (not 10.0.300 which the project was developed on)
- `global.json` is set to `10.0.100` + `rollForward: latestPatch` to accept 10.0.101
- When Replit upgrades to 10.0.300+, revert `global.json` and simplify the workflow command to `dotnet run`

## Razor SDK Bug Workaround

SDK 10.0.101's Razor source generator has a bug: `@code` blocks placed after an `@if/else` chain followed by static markup get emitted inside `BuildRenderTree()` instead of at class level.

**Fix:** `PayByLinkList.razor.cs` (code-behind partial class). The `@code` block was moved there permanently — it's cleaner anyway.

## CORS for Deployment

Before publishing, set the `EXTRA_CORS_ORIGIN` environment variable to the `.replit.app` production URL. The app reads this at startup and adds it to the allowed origins list.

## User Preferences

- Keep Replit-specific workarounds documented in this file
