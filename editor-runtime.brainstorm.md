# Editor Runtime Brainstorm

## Problem

The current `/editor` screen is not a real script editor. It renders a formatted preview of compiled TPS content, while the design source in `new-design/index.html` expects a writable TPS editor with:

- a real editable text surface
- ribbon-style toolbar commands that mutate selected text
- a floating format bar that appears for active text selection
- line/column and TPS stats in the status bar
- bidirectional sync between raw TPS text, metadata, and structure navigation

## In Scope

- Replace the decorative editor content area with a real TPS editor surface.
- Keep the `new-design` visual shell and classes.
- Use raw TPS text as the editing source of truth.
- Add syntax-highlighted rendering for visible TPS tags and structure markers.
- Support toolbar and floating-bar commands for the main formatting actions.
- Keep metadata edits and source edits synchronized both ways.
- Autosave the active script to browser storage.
- Add component and browser tests for real editing flows.

## Out Of Scope

- AI rewrite behavior beyond the current placeholder button.
- Collaborative editing.
- Rich native undo stacks beyond browser textarea behavior plus app-level source snapshots.
- Backend persistence.

## Constraints

- Standalone WASM only, no backend.
- Visual fidelity should stay aligned with `new-design`.
- No new heavy editor dependency should be introduced for this pass.
- Existing TPS parser/compiler and session pipeline remain the canonical domain path.

## Options

### Option A: Plain textarea with minimal styling

Pros:
- simplest to implement
- easy text selection and mutation

Cons:
- does not match `new-design`
- no inline syntax highlighting
- poor parity with the requested editor experience

### Option B: Contenteditable HTML editor with decorated spans as the source surface

Pros:
- can look closest to the prototype
- floating bar positioning can be DOM-native

Cons:
- hard to keep raw TPS text stable
- fragile editing behavior with nested spans and tags
- expensive DOM normalization logic

### Option C: Textarea as the source of truth plus mirrored syntax-highlighted overlay

Pros:
- raw TPS text stays authoritative
- browser selection/editing behavior stays stable
- structure/metadata sync remains simple through `UpdateDraftAsync`
- design parity can be reached with overlay styling and a floating bar positioned from the textarea selection

Cons:
- needs custom JS interop for selection position and sync scrolling
- syntax highlighting must be rendered separately

## Decision

Choose Option C.

Use a textarea-backed editor with:

- syntax-highlighted overlay behind it
- textarea selection tracking via JS interop
- toolbar and floating-bar actions that mutate raw TPS text through a dedicated editor service
- editor state isolated into small services/components so the page does not become a monolith

## Risks

- textarea/overlay scroll drift
- floating-bar coordinates can be flaky if computed from DOM assumptions
- large Razor file growth if editor state is not split
- browser tests may become flaky if selection handling relies on timing instead of deterministic state

## Risk Mitigation

- move editor logic into a dedicated service and smaller components
- keep JS interop narrow and deterministic
- expose stable `data-testid` hooks for toolbar, textarea, overlay, and floating bar
- add both bUnit and Playwright flows for source editing and formatting commands

## Recommended Direction

1. Introduce a dedicated editor state/service layer for raw source, selection, and toolbar commands.
2. Replace the preview body with a textarea + syntax overlay editor component.
3. Wire metadata updates and source edits through one shared draft update path.
4. Rebuild structure/sidebar from parser output after each source mutation.
5. Add floating bar JS interop and end-to-end tests for selection-driven formatting.
