# Editor Runtime Plan

Related brainstorm: `editor-runtime.brainstorm.md`

## Goal

Turn `/editor` into a real TPS editor that matches `new-design` closely enough to be used for writing and editing scripts, with bidirectional sync across raw source text, parsed structure, metadata, and status state.

## Scope

In scope:
- raw TPS editing surface
- syntax-highlighted editor presentation
- ribbon toolbar and floating format bar actions
- metadata/source sync
- structure/sidebar sync
- autosave to browser storage
- component and browser tests for real edit flows

Out of scope:
- backend persistence
- AI editing workflows
- collaborative editing
- full custom undo/redo engine beyond browser editing behavior

## Constraints And Risks

- Must stay standalone WASM with no backend.
- Must keep `new-design` classes and visual fidelity as the reference.
- Editor page must be decomposed to stay within repo maintainability limits.
- Selection/floating-bar behavior can be fragile across browser updates.

## Testing Methodology

Flows to cover:
- opening an existing script in the editor
- editing raw TPS text and seeing structure/status update
- changing metadata and seeing front matter/source update
- selecting text and applying toolbar formatting
- showing and hiding the floating format bar based on selection
- autosaving and reloading the edited script

How they are tested:
- focused domain/component tests for source mutation and page-state sync
- Playwright browser tests for textarea selection, floating bar, toolbar actions, and reload persistence
- full repo regression after focused suites pass

Quality bar:
- new editor behavior covered by automated tests with positive and regression assertions
- changed production code must pass build, tests, format, and coverage commands
- no flaky browser assertions or timing-only checks without state verification

## Baseline

1. [x] Run full baseline: `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
2. [x] Record any already failing tests below before implementation continues

## Failing Tests Baseline

- [x] Baseline before editor TDD was green. `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` passed before new editor tests.
- [x] `PrompterLive.App.Tests.EditorSourceInteractionTests.EditorPage_UsesRawSourceTextareaAndRebuildsStructureWhenSourceChanges`
Failure symptom:
`[data-testid='editor-source-input']` is missing.
Root cause notes:
Current editor still renders preview markup instead of a writable raw source surface.
Intended fix path:
Add a real TPS source editor component and bind it to session draft text.
- [x] `PrompterLive.App.Tests.EditorSourceInteractionTests.EditorPage_MetadataChangesRewriteRawSourceFrontMatter`
Failure symptom:
`[data-testid='editor-source-input']` is missing, so metadata cannot be verified against raw source text.
Root cause notes:
Metadata updates only affect session/front matter internally; the editor surface does not expose the draft text.
Intended fix path:
Expose the raw source textarea and keep it synchronized with metadata/front matter updates.
- [x] `PrompterLive.App.UITests.EditorInteractionTests.EditorScreen_ShowsFloatingBarAndAppliesFormattingToSelectedSourceText`
Failure symptom:
Playwright cannot find `editor-source-input`.
Root cause notes:
The browser UI has no selectable editor text surface and no floating selection bar runtime.
Intended fix path:
Implement textarea-backed editor surface, selection tracking, floating bar, and formatting commands.

## Ordered Implementation Plan

1. [x] Introduce editor task documentation updates.
Done criteria:
- brainstorm and plan exist
- architecture doc updated for editor runtime boundary if needed
Test/verification:
- docs read and kept in sync with implementation direction

2. [x] Add failing tests first for real editor behavior.
Done criteria:
- component test proves raw source editing updates session and structure
- component or integration test proves metadata writes update source text
- browser test proves selection shows floating bar and formatting mutates source
Test/verification:
- run `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
- run `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`

3. [x] Build editor state and command layer.
Done criteria:
- raw text, selection, cursor, and toolbar command logic live outside the page markup
- text mutation APIs cover wrap, insert, replace, and metadata-front-matter sync
Test/verification:
- focused tests for editor state/service pass

4. [x] Replace preview surface with a real TPS editor component.
Done criteria:
- editor uses textarea-backed raw source editing
- syntax-highlight overlay matches `new-design` styling closely
- line/column and status bar derive from real cursor/source state
Test/verification:
- component tests for source rendering and status updates pass
- manual browser smoke via Playwright assertions passes

5. [x] Add floating selection bar and toolbar command wiring.
Done criteria:
- toolbar buttons mutate selected text in source
- floating bar appears only when there is an active selection in the editor
- key formatting actions work for TPS tags from the design
Test/verification:
- Playwright test covers select text -> bar visible -> format applied -> source updated

6. [x] Keep structure and metadata synchronized from raw text.
Done criteria:
- source edits rebuild structure tree and stats
- metadata controls update front matter and source text
- source/front matter changes preserve current script identity and autosave path
Test/verification:
- component tests for structure/sidebar sync pass
- browser reload flow proves persistence

7. [x] Run final validation and close the task.
Done criteria:
- focused suites pass
- repo-wide regression passes
- formatting passes
- coverage command passes
- plan checklist fully checked
Test/verification:
- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`

## Final Validation Skills And Commands

1. `playwright`
Reason:
- verify the editor as a real browser flow with text selection and floating UI
Command/artifact:
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj`

2. repo quality pass
Reason:
- prove the repo remains green after editor refactor
Commands:
- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
