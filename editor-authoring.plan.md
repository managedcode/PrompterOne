# Editor Authoring Plan

Reference brainstorm: `editor-authoring.brainstorm.md`

## Goal

Close the main authoring gaps between the runtime TPS editor and `new-design` while keeping the app standalone, browser-only, and source-first.

## Scope

### In Scope

- richer floating selection toolbar behavior
- editable structure sidebar for active segment/block headers
- bidirectional synchronization between source, structure, and metadata
- `new-design` right-rail speed-offset fields backed by source metadata
- automated regression coverage for the above

### Out Of Scope

- AI implementation behind the AI buttons
- backend storage
- collaborative editing
- replacing TPS source editing with a WYSIWYG-only model

## Constraints And Risks

- keep the editor backend-free
- preserve `dotnet run` and `dotnet test` without extra env vars or custom ports
- do not break undo/redo, autosave, or outline navigation
- avoid file/type/function growth beyond root AGENTS limits

## Testing Methodology

- bUnit for direct editor component/page interactions and synchronization checks
- Playwright for browser-realistic selection, toolbar, reload, and structure editing flows
- full solution verification through the repo commands after focused passes
- quality bar:
  - changed editor flows must have regression tests
  - positive and negative flows must be asserted
  - no relevant suite may regress

## Baseline

- [x] Run `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- [x] Record any already failing tests here before implementation

Already failing tests:

- [x] None on the baseline full-suite run

## Ordered Plan

- [x] Add failing bUnit tests for source-to-metadata sync from raw TPS edits
  Done criteria: raw source edits to front matter and headers update the metadata rail and structure tree.
  Verification: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`

- [x] Add failing bUnit tests for structure editing to source synchronization
  Done criteria: editing active segment/block fields rewrites TPS headers and refreshes outline/status.
  Verification: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`

- [x] Add failing Playwright tests for full selection toolbar actions and reload persistence
  Done criteria: selecting text shows the floating toolbar, multiple formatting actions mutate source, and reload keeps the saved state.
  Verification: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`

- [x] Implement Core editor services for header parsing and safe segment/block header replacement
  Done criteria: structure edits are handled through reusable Core services, not page-level string hacks.
  Verification: focused core/app tests

- [x] Implement Shared editor UI updates for structure editing, speed-offset metadata fields, and fuller floating-toolbar parity
  Done criteria: the editor page behaves like a practical TPS authoring tool and stays aligned with `new-design`.
  Verification: focused app tests and Playwright tests

- [x] Re-run focused suites and fix any regressions
  Done criteria: editor-focused component and UI suites are green.
  Verification:
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`

- [x] Run final quality pass
  Done criteria: build, test, format, and coverage all pass for the repo.
  Verification:
  - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`

## Root-Cause Tracking

- [x] `PrompterLive.App.Tests.EditorStructureAuthoringTests.EditorPage_ChangingActiveStructureRewritesTpsHeaders`
  Root cause: there is no structure-inspector authoring UI and no source header rewrite service yet.
  Intended fix path: add active segment/block editing controls plus Core header replacement logic.

- [x] `PrompterLive.App.Tests.EditorStructureAuthoringTests.EditorPage_ChangingSpeedOffsetsRewritesFrontMatter`
  Root cause: the metadata rail does not expose the `new-design` speed-offset controls or persist those fields into front matter.
  Intended fix path: add speed-offset metadata inputs and persist them through the front-matter service.

- [x] `PrompterLive.App.UITests.EditorInteractionTests.EditorScreen_FloatingToolbarShowsAiAndPersistsSelectionFormatting`
  Root cause: the floating selection toolbar is missing the AI button and the expanded test hooks for richer authoring behavior.
  Intended fix path: add the missing floating-toolbar affordances and keep selection formatting persisted through autosave.

- [x] `PrompterLive.App.UITests.EditorInteractionTests.EditorScreen_StructureInspectorEditsRewriteHeaders`
  Root cause: there is no browser-visible structure inspector for editing active segment/block header fields.
  Intended fix path: expose structure editing controls and bind them to safe header rewrite operations.

## Final Validation Skills And Commands

- [x] `playwright`
  Reason: verify real browser selection, toolbar, and reload behavior.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
  Reason: fast regression feedback for editor page behavior.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`
  Reason: real DOM and browser behavior for selection and persistence.

- [x] `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  Reason: repo-wide regression gate.
