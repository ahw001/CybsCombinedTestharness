---
name: Razor SDK 10.0.101 bug
description: Bug in SDK 10.0.101 Razor source generator that misplaces @code blocks; workaround is a code-behind file.
---

SDK `10.0.101`'s `Microsoft.CodeAnalysis.Razor.Compiler` has a bug: when a `.razor` file has an `@if/else` chain followed by static markup (e.g. `<footer>`) and then a `@code { }` block, the generator places the `@code` content inside `BuildRenderTree()` instead of at class level. This causes ~125 cascading CS errors (CS1513, CS0201, CS0103 `get`/`set` not in context).

**Rule:** Any `.razor` file matching that pattern must use a `.razor.cs` code-behind partial class instead of an inline `@code` block.

**Why:** The Razor source generator in 10.0.101 has this structural bug. The project was developed with 10.0.300 which has it fixed.

**How to apply:** Move `@code` content to `<ComponentName>.razor.cs`, change `@inject` directives to `[Inject]` properties. This is already done for `PayByLinkList.razor` / `PayByLinkList.razor.cs`. If new pages hit the same errors, apply the same fix.
