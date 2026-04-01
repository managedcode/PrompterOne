# PrompterOne Rename Plan

## Goal

Rename the product, solution, projects, folders, namespaces, docs, workflows, and repo-owned runtime identifiers from the legacy product name to `PrompterOne` without regressing the standalone Blazor WebAssembly runtime or the existing browser acceptance gates.

## Scope

### In Scope

- rename the solution file, tracked project folders, project files, and test project folders that still use the legacy product name
- rename C# namespaces, `using` directives, project references, MSBuild properties, workflow variables, run-config names, and documentation references
- rename repo-owned runtime identifiers that still encode the old product name, including storage/database keys, cloud root paths, release artifact names, and browser-side JS globals where the repo owns the contract
- update root and local `AGENTS.md`, `README.md`, and `docs/Architecture.md` so the architecture and command map reflect `PrompterOne`
- preserve the user’s current in-flight teleprompter changes while applying the rename on top

### Out Of Scope

- feature redesigns unrelated to the rename
- behavior changes beyond what is required to keep renamed contracts compiling, booting, and passing tests
- vendored third-party code that only mentions the old name inside upstream assets and is not a repo-owned product contract

## Constraints And Risks

- The worktree is already dirty in teleprompter files and `AGENTS.md`; no existing user edits may be reverted.
- Browser UI verification is the primary acceptance gate, and the browser suite must run as a single `dotnet test` process.
- Renaming browser storage keys and cloud root paths can orphan existing local data if the consuming code or tests miss a contract update.
- Renaming the root workspace folder changes all absolute command paths in docs and AGENTS files and must happen after tracked references are updated.
- Generated `bin/`, `obj/`, and IDE caches are noise for discovery; only tracked repo content drives the rename.

## Testing Methodology

- Validate compile-time fallout first with the full solution build under `-warnaserror`.
- Validate cross-project behavior with the full solution test pass after the build baseline is green.
- Validate the primary acceptance layer with the browser UI suite in the renamed path after the source and folder rename is complete.
- Validate formatting and coverage only after all rename fallout is resolved so the final repo state is the measured state.
- Quality bar:
  - solution build succeeds with zero warnings promoted under `-warnaserror`
  - solution test pass is green
  - browser acceptance suite is green in the renamed path
  - no tracked repo-owned legacy-name references remain unless explicitly documented as intentional compatibility residue

## Ordered Steps

### 1. Baseline Inventory And Plan Freeze

- [x] Search tracked files and tracked file paths for legacy-name variants while excluding generated output.
- [x] Classify the hits into rename layers: filesystem paths, namespaces/project references, docs/AGENTS, workflows/tooling, and runtime/browser contracts.
- [x] Capture this migration plan in `/Users/ksemenenko/Developer/PrompterOne/prompterone-rename.plan.md`.
- Verify before moving on:
  - the tracked-file inventory is complete enough to avoid blind search-and-replace in generated output
  - the plan names the risks and the required validation sequence

### 2. Full-Test Baseline Before Edits

- [x] Run the full solution build on the pre-rename solution path with `-warnaserror`.
- [x] Run the full solution test pass on the pre-rename solution path.
- [x] Record every failing baseline test below with the symptom, suspected cause, and intended fix path before applying the rename.
- Verify before moving on:
  - baseline build status is recorded
  - baseline solution-test status is recorded
  - every pre-existing failing test, if any, is tracked explicitly below

## Baseline Failures

- [x] No baseline failures.
  - Symptom: none. The full solution build passed before the workspace folder was renamed.
  - Suspected cause: not applicable.
  - Intended fix path: preserve this green baseline through the rename and rerun the same gates after path changes.

### 3. Rename Source, Docs, And Repo-Owned Contracts In Place

- [x] Update tracked file contents so source code, Razor imports, project references, MSBuild property names, docs, workflows, and run configs use `PrompterOne`.
- [x] Update repo-owned runtime/browser identifiers that still encode the old product name, including storage keys, cloud root paths, artifact names, and JS globals.
- [x] Re-scan tracked file contents for old-name references and either eliminate them or document each intentional residue.
- Verify before moving on:
  - `git grep` shows no unintended old-name content hits in tracked files
  - all project files and docs reference the future renamed folders/files

### 4. Rename Tracked Filesystem Paths

- [x] Rename the solution file, project files, project folders, test folders, run configs, and any other tracked filesystem paths from legacy-name variants to `PrompterOne*`.
- [x] Rename the root workspace folder from the pre-rename absolute path to `/Users/ksemenenko/Developer/PrompterOne`.
- [x] Re-anchor commands and file references to the new absolute path after the folder rename.
- Verify before moving on:
  - `git status --short` shows the expected renames instead of delete/add drift where possible
  - the workspace is accessible from `/Users/ksemenenko/Developer/PrompterOne`

### 5. Repair Fallout And Run Focused Verification

- [x] Run `dotnet build /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx -warnaserror` and fix rename fallout until it passes.
- [x] Run `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx` and fix failing tests until it passes.
- [x] Run `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --no-build` once the build already succeeded, and fix any browser-contract fallout.
- [x] Re-scan tracked files and tracked paths for legacy-name variants.
- Verify before moving on:
  - compile, unit/component, and browser acceptance checks are green
  - old-name scans are clean or documented
- Repair notes:
  - The rename initially broke editor UI tests because stale scoped CSS assets still referenced an outdated Razor scope attribute in generated static web assets.
  - Cleaning `bin/` and `obj/` under `src/PrompterOne.App` and `src/PrompterOne.Shared`, then rebuilding the solution, regenerated aligned scoped CSS and cleared the browser failures.

### 6. Final Quality Pass

- [x] Run `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx --collect:"XPlat Code Coverage"`.
- [x] Run `dotnet format /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx`.
- [x] Update this plan with final outcomes, completed checklist items, and any documented intentional residue.
- Verify before moving on:
  - coverage command completes successfully
  - format command completes successfully
  - the plan reflects the final verification evidence
- Final outcome:
  - full solution build, full solution tests, browser UI suite, coverage, and `dotnet format` all completed successfully in `/Users/ksemenenko/Developer/PrompterOne`
  - no tracked legacy-name references remain after the final content scan
  - prototype invite URLs now resolve from `window.location.origin` instead of a hardcoded branded domain

## Final Validation Skills And Commands

1. `dotnet-blazor`
   - Action: verify the Blazor WebAssembly host, Razor project references, and static web asset naming still line up after the rename.
   - Outcome: renamed solution builds and the browser-hosted app test harness still resolves the right assets.
2. `playwright`
   - Action: execute the real browser acceptance layer through `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --no-build`.
   - Outcome: major routed flows still work in a browser after the rename.
3. Repo quality commands
   - `dotnet build /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx -warnaserror`
   - `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx`
   - `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx --collect:"XPlat Code Coverage"`
   - `dotnet format /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx`
   - Reason: these are the repo-defined gates and must pass in the renamed workspace.
