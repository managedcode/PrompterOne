# Stabilize Browser Suite Parallelism

## Goal

Make the browser acceptance suites deterministic under the higher in-suite parallelism the repo already allows, then prove the fix with full local baseline runs and the replacement GitHub Actions run on `main`.

## Scope

In scope:
- `tests/PrompterOne.Web.UITests/Infrastructure/*` and `tests/PrompterOne.Web.UITests/Support/*` shared browser harness helpers
- `tests/PrompterOne.Web.UITests.Shell/**/*`
- `tests/PrompterOne.Web.UITests.Studio/**/*`
- `tests/PrompterOne.Web.UITests.Reader/**/*`
- `tests/PrompterOne.Web.UITests.Editor/**/*` where the shared-harness fix or route-wait fix changes expectations
- Push to `main` and watch the replacement Actions run

Out of scope:
- Lowering CI parallelism back to `2`
- Timeout-only “fixes”
- Unrelated product behavior changes outside what is required for test determinism

## Constraints And Risks

- Keep one browser-suite `dotnet test` process at a time locally.
- Do not revert the user’s requested higher CI concurrency posture.
- Prefer harness isolation and stable route contracts over weaker assertions.
- Shared-context tests that intentionally validate storage/session propagation must stay shared.
- Preserve production-owned `data-test` hooks and route contracts.

## Testing Methodology

- Establish the failing baseline from the real browser suites and GitHub Actions logs first.
- Fix one failure class at a time: shared `BrowserContext` teardown interference, then SPA route-wait flakiness.
- Verify by layers:
  - targeted previously failing suites
  - all changed browser suites
  - full `dotnet test --solution ./PrompterOne.slnx --max-parallel-test-modules 1`
- After local green, run `dotnet format`, push `main`, and monitor the new Actions run until green or a concrete external blocker remains.

## Baseline And Failing Tests

- [x] Baseline local browser-suite failures reproduced and correlated with the user-reported red tests.
- [x] Previous failed GitHub Actions run inspected.
  Result:
  - Shell, Studio, and Reader had widespread first-screen visibility failures under parallel load.
  - Solution-level rerun exposed an additional SPA-route timing failure in `GoLiveFlowTests.SettingsPage_LinksIntoGoLiveRoutingAndGoLiveLinksBackToSettings`.

Tracked failures:
- [x] `EditorAiAvailabilityTests.EditorScreen_AiButtonsAreEnabled_WhenAProviderIsConfigured`
  Symptom: `editor-page` not found.
  Root-cause note: direct editor tests reused shared contexts and then closed them from sibling tests.
  Intended fix path: force isolated contexts for direct page-based editor tests.
- [x] `EditorCueRenderingFlowTests.EditorScreen_RendersMonacoCueStylesImmediatelyAfterImport`
  Symptom: `editor-page` / `editor-source-stage` not found.
  Root-cause note: same shared-context teardown interference.
  Intended fix path: isolate direct editor tests.
- [x] `EditorFindFlowTests.EditorScreen_FindBar_UsesStyledChrome`
  Symptom: `editor-source-stage` not found.
  Root-cause note: same shared-context teardown interference.
  Intended fix path: isolate direct editor tests.
- [x] `LibraryScreen_OpenScriptImportsLocalFileAndNavigatesIntoEditor`
  Symptom: wrong imported title shown in header.
  Root-cause note: already fixed in prior import-title pass; kept under regression watch here.
  Intended fix path: no new action unless the broader stabilization regresses it.
- [x] `LibraryScreen_OpenScriptCanImportASecondFile_AfterPickerResets`
  Symptom: wrong imported title shown in header.
  Root-cause note: already fixed in prior import-title pass; kept under regression watch here.
  Intended fix path: no new action unless the broader stabilization regresses it.
- [x] `GoLiveFlowTests.SettingsPage_LinksIntoGoLiveRoutingAndGoLiveLinksBackToSettings`
  Symptom: route wait timed out on `**/settings` during SPA navigation.
  Root-cause note: `WaitForURLAsync` depended on navigation/load semantics that were race-prone for in-app route changes under suite load.
  Intended fix path: replace navigation-event waits with an assertion-based SPA route helper.

## Ordered Plan

- [x] Step 1. Inspect the shared browser harness and identify which tests intentionally share contexts versus accidentally reuse them.
  Verification:
  - Confirmed `StandaloneAppFixture.NewPageAsync()` defaults to shared contexts keyed by caller member name.
  - Confirmed many direct page-based tests in Editor, Shell, Studio, and Reader were not intended to share contexts.

- [x] Step 2. Establish the red baseline locally and from CI logs.
  Verification:
  - Reproduced local editor and browser-suite failures.
  - Pulled CI job logs for the failed `main` run and mapped the failure pattern.

- [x] Step 3. Isolate direct page-based tests that were closing shared contexts.
  Actions:
  - Converted direct `NewPageAsync()` callers in the affected suites to `NewPageAsync(additionalContext: true)`.
  - Preserved the intentionally shared tests in `DynamicHostPortTests` and the existing `NewSharedPagesAsync(...)` cases.
  Verification:
  - `PrompterOne.Web.UITests.Editor` passed under `--maximum-parallel-tests 8`.
  - Shell, Studio, and Reader targeted reruns cleared the widespread first-screen visibility failures.

- [x] Step 4. Replace brittle SPA route waits with a deterministic helper.
  Actions:
  - Added `tests/PrompterOne.Web.UITests/Support/BrowserRouteDriver.cs`.
  - Swapped `WaitForURLAsync(BrowserTestConstants.Routes.Pattern(...))` for `BrowserRouteDriver.WaitForRouteAsync(...)` across the affected browser suites, leaving the one variable-pattern onboarding wait unchanged.
  Verification:
  - The previously failing `GoLiveFlowTests.SettingsPage_LinksIntoGoLiveRoutingAndGoLiveLinksBackToSettings` now passes.
  - Updated Shell, Studio, Reader, and Editor suites build and run successfully.

- [x] Step 5. Run changed-suite verification.
  Verification:
  - `dotnet build ./PrompterOne.slnx -warnaserror` passed.
  - `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Studio/PrompterOne.Web.UITests.Studio.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20` passed.
  - `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Shell/PrompterOne.Web.UITests.Shell.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20` passed.
  - `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Reader/PrompterOne.Web.UITests.Reader.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20` passed.
  - `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Editor/PrompterOne.Web.UITests.Editor.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20` passed.

- [x] Step 6. Run the required full-solution baseline.
  Command:
  - `dotnet test @./tests/dotnet-test-progress.rsp --solution ./PrompterOne.slnx --max-parallel-test-modules 1`
  Verification:
  - Full solution passes after the shared-harness and route-wait fixes.
  - Repeated after `dotnet format` and a fresh `dotnet build ./PrompterOne.slnx -warnaserror`; both full-solution reruns finished green at `1152/1152`.

- [ ] Step 7. Run formatting and publish the fix.
  Actions:
  - Run `dotnet format ./PrompterOne.slnx`.
  - Stage only task-relevant files.
  - Commit on `main`.
  - Push `main`.
  Verification:
  - Local tree is clean except for expected plan artifacts.

- [ ] Step 8. Monitor the replacement GitHub Actions run.
  Verification:
  - The new `main` run finishes green or any remaining blocker is documented with the specific failing job/test.

## Final Validation Skills And Commands

- `dotnet`
  Reason: repo-standard build and test validation for the changed .NET/browser code.
- `github:gh-fix-ci`
  Reason: inspect and monitor the replacement GitHub Actions run on `main`.
- `dotnet build ./PrompterOne.slnx -warnaserror`
  Reason: required repository build gate.
- `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Studio/PrompterOne.Web.UITests.Studio.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20`
  Reason: directly verifies the SPA route-wait fix in the suite that exposed it.
- `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Shell/PrompterOne.Web.UITests.Shell.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20`
  Reason: confirms the broader shell/shared-context fixes stay green.
- `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Reader/PrompterOne.Web.UITests.Reader.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20`
  Reason: confirms the reader suite remains stable after shared-harness changes.
- `dotnet test @./tests/dotnet-test-progress.rsp --project ./tests/PrompterOne.Web.UITests.Editor/PrompterOne.Web.UITests.Editor.csproj --maximum-parallel-tests 8 --maximum-failed-tests 20`
  Reason: confirms editor stability after the context-isolation fix.
- `dotnet test @./tests/dotnet-test-progress.rsp --solution ./PrompterOne.slnx --max-parallel-test-modules 1`
  Reason: required full-solution regression gate from root instructions.
- `dotnet format ./PrompterOne.slnx`
  Reason: required repository formatting gate before finalizing the change.
