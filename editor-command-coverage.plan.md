# editor-command-coverage.plan

Chosen brainstorm: `editor-command-coverage.brainstorm.md`

## Goal

Prove that the editor toolbar and floating-bar command surface works end-to-end in the browser, and prove that direct TPS body edits synchronize back into the structure UI.

## Scope

In scope:
- exhaustive browser verification for toolbar and floating-bar command actions
- browser verification for direct source edit -> structure sync
- any production fixes needed to make existing editor commands browser-correct
- targeted doc updates if the changed behavior or test strategy needs to be documented

Out of scope:
- replacing the textarea/overlay architecture with a contenteditable editor rewrite
- backend or AI service integration
- non-editor screens

## Constraints And Risks

- must preserve the body-only editor surface; front matter stays hidden from the source editor
- must preserve stable `data-testid` selectors already used by Playwright
- tests must stay deterministic and avoid flaky selection behavior
- do not touch `new-design/`

## Testing Methodology

Flows covered:
- toolbar command buttons mutate TPS body text in-browser
- floating toolbar command buttons mutate TPS body text in-browser
- AI buttons open the deterministic local assistant panel
- menu trigger buttons open the expected dropdown panels
- direct source header edits update structure inspector values in-browser

How they are tested:
- Playwright UI tests for real browser mutation and DOM-visible sync
- focused component tests only if a production regression needs fast isolation
- full solution regression after focused editor verification

Quality bar:
- every toolbar/floating action has explicit browser verification or explicit non-command rationale
- edited TPS text persists through the existing autosave/session pipeline
- relevant test suites are green after the change

## Ordered Plan

- [x] Step 1. Write this brainstorm and choose the direction.
  Done criteria: recommendation recorded and constraints understood.

- [x] Step 2. Write this plan.
  Done criteria: scope, risks, and verification approach are explicit.

- [x] Step 3. Establish the baseline.
  Verification: run `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`.
  Done criteria: current failures, if any, are recorded below.

- [x] Step 4. Add failing browser regressions for exhaustive command coverage and source-sync.
  Verification: run focused editor UI tests and capture failures.
  Done criteria: at least one new regression fails for the right reason.

- [x] Step 5. Implement any production fixes needed for command execution or editor synchronization.
  Verification: rerun focused editor UI tests and related component/core tests.
  Done criteria: new regressions pass and no existing editor behavior regresses.

- [x] Step 6. Update docs if the verification contract or editor behavior changed materially.
  Verification: review updated docs for accuracy.
  Done criteria: architecture/feature docs remain current.

- [x] Step 7. Run final validation.
  Verification:
  - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` because build is a separate gate.
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~Editor"` for focused browser proof.
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` for broader regressions.
  - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` for repo formatting/analyzer pass.
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"` for coverage visibility.
  Done criteria: all relevant tests are green and verification results are recorded.

## Baseline Failures

- Baseline run completed on `PrompterLive.slnx`; no pre-existing editor failures were recorded before adding the new regressions.

## Regression Notes

- Added browser regressions for full toolbar command coverage, floating-bar coverage, and direct `source -> structure` synchronization.
- First red state found a real floating AI regression: the post-click selection event closed the AI panel immediately. Fixed with one-shot selection-close suppression in `EditorSourcePanel`.
- First red state also showed floating-bar browser clicks racing the selection animation. Fixed in test flow with an explicit settle delay after selection, after confirming the live browser interaction worked.
- The `source -> structure` regression initially failed because the test replaced stale seed header text. The test now rewrites the first segment and block headers by regex so it validates actual synchronization instead of a brittle sample string.

## Final Validation Skills And Commands

- `playwright`: use real browser verification for the editor command surface and source-sync behavior.
- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`: prove the solution compiles after production changes.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~Editor"`: prove the changed browser flows.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`: prove broader regressions stay green.
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`: apply repo-defined formatting/analyzer gate.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`: record post-change coverage visibility.

## Final Validation Results

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` succeeded. The repo still has baseline analyzer warnings outside this task scope.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --filter "FullyQualifiedName~Editor"` passed with `16/16` editor component tests green.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~EditorToolbarCoverageTests|FullyQualifiedName~EditorSourceSyncTests"` passed with `4/4` focused browser tests green.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj --no-build` passed with `21/21` green.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --no-build` passed with `27/27` green.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build` passed with `16/16` green.
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` completed, but the repo still has non-auto-fixable analyzer findings outside this task scope.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"` completed and produced coverage artifacts for all three test projects.
