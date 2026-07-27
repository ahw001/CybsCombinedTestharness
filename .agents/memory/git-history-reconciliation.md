---
name: Git history reconciliation
description: How to recover when gitPull fails with MERGE_CONFLICT due to the shallow/grafted clone and diverged remote history.
---

Replit imports repos as shallow/grafted clones. When the remote diverges (e.g. user force-pushes a regenerated repo), `gitPull` returns `MERGE_CONFLICT` and `gitPush` returns `PUSH_REJECTED` — even after resetting to the graft root — because git cannot find a common ancestor.

**Rule:** When both gitPull and gitPush fail due to diverged grafted history, do `git reset --hard origin/main` (the fetched remote ref is already in the object store from the failed pull attempt), then reapply Replit-specific files and commit.

**Why:** The gitPull callback fetches remote objects before attempting the merge, so `origin/main` is resolvable locally even when the merge fails. Hard-resetting to it bypasses the ancestry problem.

**How to apply:**
1. `git reset --hard origin/main` — moves HEAD to the remote commit
2. Re-apply Replit files: `global.json` (latestPatch), `.replit`, `replit.md`, and any code-behind workaround files
3. `git add` + `git commit` + `gitPush({})` — now a clean fast-forward

Replit-specific files to preserve across regenerations: `global.json`, `.replit`, `replit.md`, `PayByLinkList.razor.cs`.
