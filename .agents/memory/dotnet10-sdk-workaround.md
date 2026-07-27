---
name: .NET 10 SDK workaround
description: How to run .NET 10 on Replit where only SDK 10.0.101 is available and PATH is not updated by the module.
---

The `dotnet-10.0` Nix module installs SDK `10.0.101` but does not update PATH in running shells or workflows.

**Rule:** Workflow command must use the full nix store path:
`/nix/store/5hfn7q3adjwa8dh4yhhw1ip8njcbs7vs-dotnet-sdk-wrapped-10.0.101/bin/dotnet`

**Why:** The module updates PATH only for new login shells, not for already-running workflow processes.

**How to apply:** Use the full path in `configureWorkflow` command and in `[deployment].run` in `.replit`. Also set `global.json` to `"version": "10.0.100"` + `"rollForward": "latestPatch"` so SDK 10.0.101 is accepted.

When Replit makes 10.0.300+ available, revert `global.json` and simplify the command to plain `dotnet run`.
