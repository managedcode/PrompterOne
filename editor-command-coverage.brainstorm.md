# editor-command-coverage.brainstorm

## Problem Framing

The editor now has a descriptor-driven toolbar, but the user requirement is stronger than structural cleanup:

- every visible editor button and menu action must work
- the editor must behave like a real TPS authoring surface, not decorative markup
- browser tests must prove the command surface is alive
- direct body edits must sync back into structure UI

Current gaps:

- Playwright coverage only proves a subset of toolbar and floating-bar commands
- there is no single regression proving the full catalog has browser-visible command behavior
- source-to-structure synchronization is not proven at the browser level after inline source edits

## Options

### Option 1: Add a few more hand-written UI tests

Pros:
- fast to write
- low production code risk

Cons:
- leaves catalog drift risk
- still easy to miss buttons when new commands are added
- does not scale with the descriptor-driven toolbar

### Option 2: Add catalog-driven browser coverage and source-sync tests

Pros:
- aligns with the descriptor-driven production model
- reduces future drift
- gives explicit proof that toolbar actions mutate TPS text
- covers the user-visible contract instead of only markup presence

Cons:
- test harness is more complex
- generic assertions need careful command-specific expectations

### Option 3: Move to a fully contenteditable editor immediately

Pros:
- closer to literal WYSIWYG semantics

Cons:
- much larger rewrite
- high risk of destabilizing autosave, selection handling, and TPS synchronization
- not required to satisfy the immediate regression and coverage gap

## Recommended Direction

Choose Option 2.

Keep the current body-editor architecture, but make the browser verification exhaustive enough that the toolbar can be trusted as a real command surface. Add an explicit browser regression for source header edits updating the structure inspector.

## Risks

- generic command assertions can become flaky if selection handling is not deterministic
- some insert commands need different setup than wrap commands
- UI tests can become slow if every action opens a fresh browser session

## Mitigations

- reuse a single page per test and reset selection programmatically
- derive expectations from `EditorToolbarCatalog` descriptors instead of duplicating button semantics
- split coverage into stable groups: toolbar commands, floating-bar commands, and source-sync

## Open Questions

- none blocking; all needed command descriptors already exist in production code
