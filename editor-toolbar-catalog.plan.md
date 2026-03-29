# Editor Toolbar Catalog Plan

Reference brainstorm: `editor-toolbar-catalog.brainstorm.md`

## Goal

Replace the hardcoded editor toolbar surface with a descriptor-driven command catalog, add the missing actionable parity items, and prove the full toolbar and floating-bar command surface in browser tests.

## Scope

### In Scope

- refactor toolbar and floating-bar action definitions into reusable descriptors
- preserve `new-design` layout and CSS classes while reducing hardcoded command duplication
- add missing actionable items that should work in standalone WASM, including `remove color`
- add broad component and UI coverage for every toolbar section

### Out Of Scope

- migrating to a separate contenteditable JS editor
- backend AI integrations
- non-editor screen work

## Constraints And Risks

- keep the standalone WASM runtime shape
- keep `data-testid` selectors stable or predictably extended
- do not touch `new-design/`
- avoid introducing service doubles into verification

## Testing Methodology

- core tests for deterministic text mutations such as `clear color`
- component tests for catalog rendering and editor command surface presence
- Playwright for full toolbar clicks, dropdown behavior, and floating-bar actions in a real browser
- full solution gates after focused editor suites

## Baseline

- [x] Run `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- [x] Record any already failing tests here before implementation

Already failing tests:

- [x] None recorded at baseline

## Ordered Plan

- [x] Add red tests for missing toolbar parity and full-surface coverage
  Done criteria: tests fail until all sections are wired through stable selectors and the missing `remove color` behavior exists.
  Verification:
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`
  Result:
  - component regression added for `editor-color-clear`
  - browser regression added for color clear, professional emotion, custom WPM, and pronunciation actions

- [x] Introduce an editor toolbar catalog and render the toolbar/floating bar from descriptors
  Done criteria: command tokens, labels, tips, and test ids are centralized instead of repeated across large hand-written markup blocks.
  Verification: focused component tests and compile pass
  Result:
  - toolbar and floating bar now render from `EditorToolbarCatalog`
  - duplicated `@onclick` command markup was removed from `EditorSourcePanel.razor`

- [x] Implement missing deterministic command behavior needed for toolbar parity
  Done criteria: every rendered toolbar action produces a visible source mutation or explicit local response; `remove color` works on selected wrapped text.
  Verification: focused tests and browser flow
  Result:
  - `EditorCommandKind.ClearColor` added
  - `TpsTextEditor.ClearColorFormatting(...)` removes selected or enclosing TPS color tags
  - browser flow proves mutation through the real editor surface

- [x] Re-run focused editor suites and fix regressions
  Done criteria: editor-specific component and UI suites are green.
  Verification:
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --filter "FullyQualifiedName~Editor"`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~Editor"`
  Result:
  - component editor suite green: `16` passed
  - UI editor suite green: `9` passed

- [x] Run final quality pass
  Done criteria: build, test, format, and coverage complete successfully.
  Verification:
  - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
  Result:
  - build green: `0 warnings`, `0 errors`
  - full test green: `60` tests passed (`21` core, `27` component, `12` UI)
  - format completed; repo-wide analyzer items remain without auto-fixes for `IDE0060`, `CA1305`, and `CA1826`
  - coverage collector generated Cobertura artifacts for all three test projects

## Root-Cause Tracking

- [x] No known baseline failures
  Root cause: the pre-change solution test run was already green.
  Intended fix path: keep the wider regression baseline green while adding toolbar parity coverage.

- [x] Toolbar command surface was too hardcoded
  Root cause: the editor toolbar lived as repeated hand-written button markup with duplicated command definitions.
  Intended fix path: centralize toolbar descriptors in a single catalog and render from that catalog.

- [x] `remove color` parity was missing
  Root cause: color actions only supported wrap commands; there was no deterministic mutation for clearing TPS color tags.
  Intended fix path: add a dedicated editor command kind and implement the mutation in `TpsTextEditor`.

- [x] Toolbar selectors collided with metadata selectors
  Root cause: speed-row toolbar ids overlapped with metadata rail ids after catalog extraction.
  Intended fix path: separate toolbar ids from metadata ids and keep all selectors unique.

## Final Validation Skills And Commands

- [x] `playwright`
  Reason: validate the full toolbar and floating-bar surface in a real browser.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
  Reason: fast regression loop for toolbar rendering and command behavior.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`
  Reason: prove real menu clicks and source mutations through the browser runtime.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  Reason: repo-wide regression gate.

## Coverage Notes

- `tests/PrompterLive.Core.Tests/TestResults/131cb396-1e63-4ca9-9ec0-24500d02e14b/coverage.cobertura.xml`: `46.82%` line, `31.89%` branch
- `tests/PrompterLive.App.Tests/TestResults/2f7379e7-f1cd-4c7d-8d6b-5f1dcdd4e622/coverage.cobertura.xml`: `55.97%` line, `44.09%` branch
- `tests/PrompterLive.App.UITests/TestResults/d67a9ce3-3ec4-4dac-bd9f-575158f8a69e/coverage.cobertura.xml`: `0.00%` line, `100.00%` branch

These are the current collector outputs for the repo baseline after the toolbar-catalog pass. The editor command surface gained new regression coverage, but the repo still needs a broader coverage-raising effort if it wants to meet the aspirational AGENTS thresholds module-wide.
