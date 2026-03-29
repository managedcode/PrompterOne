# Editor History Plan

Related brainstorm: `editor-history.brainstorm.md`

## Goal

Turn the editor `History` controls into real undo/redo operations for the standalone WASM TPS editor, including keyboard shortcuts and regression coverage.

## Scope

In scope:
- toolbar undo/redo behavior
- keyboard history shortcuts inside the editor source surface
- source plus selection snapshot history
- tests for history replay

Out of scope:
- persisted history after reload
- cross-tab history sync
- collaborative editing history

## Constraints And Risks

- source text and selection must stay synchronized during history replay
- history replay must not duplicate entries or create infinite loops
- browser shortcut behavior must be deterministic enough for Playwright

## Testing Methodology

Flows to cover:
- edit source, undo, redo from toolbar
- apply formatting command, undo, redo
- use keyboard history shortcuts from textarea
- preserve selection/caret position after replay

How they are tested:
- focused shared/component tests for history state transitions
- Playwright browser tests for toolbar and keyboard flows on the live editor
- full repo regression after focused suites pass

Quality bar:
- new history behavior has regression coverage in both component and browser tests
- editor source, structure, and status remain correct after replay
- full repo regression stays green after the change

## Baseline

1. [x] Run full baseline: `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
2. [x] Record failing tests below before implementation continues

## Failing Tests Baseline

- [x] Baseline before history TDD was green. `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` passed before adding history tests.
- [x] `PrompterLive.App.Tests.EditorSourceInteractionTests.EditorPage_HistoryButtonsReplaySourceChanges`
Failure symptom:
`[data-testid='editor-undo']` is missing.
Root cause notes:
History buttons exist only as decorative markup with no command wiring or state service.
Intended fix path:
Add undo/redo controls backed by an editor history service and replay state through the same source-apply path.
- [x] `PrompterLive.App.UITests.EditorInteractionTests.EditorScreen_UndoAndRedoWorkFromToolbarAndKeyboard`
Failure symptom:
Playwright times out waiting for `editor-undo`.
Root cause notes:
No visible undo/redo controls or keyboard history bindings exist in the live editor.
Intended fix path:
Expose toolbar controls, add history replay runtime, and bind keyboard shortcuts to the same commands.

## Ordered Implementation Plan

1. [x] Add failing tests for editor history flows.
Done criteria:
- component test proves undo/redo replays source and selection
- browser test proves toolbar history buttons work on the live editor
- browser or component test proves keyboard shortcuts replay the same history
Test/verification:
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`

2. [x] Build an editor history state service.
Done criteria:
- bounded undo and redo stacks exist
- snapshots store source and selection state
- replay can apply prior and next states without duplicating history
Test/verification:
- focused tests for history service pass

3. [x] Wire toolbar buttons and keyboard shortcuts to history commands.
Done criteria:
- undo/redo buttons mutate the source surface
- keyboard shortcuts in the textarea trigger the same commands
- selection and status bar update after replay
Test/verification:
- browser history flow passes
- component tests pass

4. [x] Keep autosave and outline rebuild compatible with history replay.
Done criteria:
- undo/redo updates saved draft state
- structure/sidebar and metadata remain synchronized after replay
- no history-loop regressions
Test/verification:
- focused editor tests pass
- full repo regression passes

5. [x] Run final validation and close the task.
Done criteria:
- focused suites pass
- repo-wide regression passes
- format passes
- coverage pass runs
Test/verification:
- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
