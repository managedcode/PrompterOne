# Editor Authoring Brainstorm

## Problem

The standalone TPS editor already supports raw-source editing, autosave, highlighting, and basic undo/redo, but it still falls short of the `new-design` editor contract in three important ways:

1. the selection toolbar is still narrower than the reference behavior
2. structure is readable but not directly editable
3. metadata/source/structure synchronization is incomplete in the authoring direction

The user expectation for this pass is a real script editor, not a decorative preview shell.

## In Scope

- keep the editor source-first and TPS-first
- keep the runtime backend-free
- keep the current raw source textarea plus syntax-highlight overlay architecture
- add missing authoring behavior needed for a practical editor workflow
- tighten source-to-structure and structure-to-source synchronization
- add regression coverage in bUnit and Playwright

## Out Of Scope

- real AI rewriting
- collaborative editing
- backend persistence
- replacing the browser-first WASM runtime
- a full WYSIWYG rich-text editor that abandons TPS source

## Constraints

- match `new-design/index.html` closely
- do not break the plain `dotnet run` and `dotnet test` workflow
- keep logic reusable in `PrompterLive.Core` when it is not Blazor-specific
- do not mutate the user-owned `new-design/` folder

## Options

### Option A: Replace the source textarea with a full contenteditable TPS editor

Pros:

- closer to visual WYSIWYG editing

Cons:

- much higher complexity
- harder TPS source fidelity
- larger JS/runtime surface
- more fragile browser behavior

### Option B: Keep the textarea + overlay architecture and deepen authoring behavior

Pros:

- preserves raw TPS as the real source of truth
- lower risk to autosave, history, and parser flows
- easier to test and keep backend-free
- aligns with the current runtime shape

Cons:

- some editing interactions stay source-oriented rather than fully WYSIWYG

### Option C: Split the editor into separate source and rendered-preview panes

Pros:

- simple implementation

Cons:

- drifts away from `new-design`
- does not solve the user's request for a real editor surface

## Recommended Direction

Choose Option B.

Implement a stronger TPS authoring workflow on top of the current raw-source editor:

- expand the floating selection toolbar to the full `new-design` affordance set used in the app
- add structure editing for the active segment and block so source, structure, and metadata all sync both ways
- improve source highlighting for TPS header parts and inline markers
- add speed-offset metadata fields matching the right rail in `new-design`

This keeps the editor TPS-native, testable, and browser-safe while materially improving the authoring experience.

## Risks

- structure edits may desynchronize offsets if header replacement is careless
- source highlighting can regress selection positioning if markup diverges too far from textarea metrics
- new metadata fields can create parser drift if key naming is inconsistent

## Mitigations

- centralize header parsing and replacement in Core editor services
- keep the textarea as the only editable DOM source of truth
- add regression tests for source edits, structure edits, metadata edits, reload persistence, and toolbar mutations
