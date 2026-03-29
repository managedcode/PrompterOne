# Editor History Brainstorm

## Problem

The editor now has a real TPS source surface, but the `History` controls from `new-design` are still decorative. A full script editor needs working undo/redo for toolbar-driven edits and manual source changes, and the history actions should preserve text plus caret/selection state closely enough to make editing usable.

## In Scope

- wire the history buttons to real undo/redo behavior
- preserve source text and selection/caret in editor history snapshots
- support keyboard history shortcuts from the editor surface
- add automated tests for toolbar and keyboard flows

## Out Of Scope

- collaborative history
- multi-document history
- long-lived persisted history across page reloads
- full CRDT or operational transform systems

## Constraints

- standalone WASM only
- raw TPS textarea stays the source of truth
- existing autosave flow must continue to save the current draft after history operations
- selection behavior must remain deterministic in Playwright

## Options

### Option A: Rely on native browser textarea undo/redo only

Pros:
- almost no code
- keyboard shortcuts mostly work automatically

Cons:
- Blazor-bound `value` updates can reset native history
- toolbar actions and metadata rewrites do not reliably integrate with browser undo stacks
- hard to test deterministically

### Option B: App-managed editor history stack

Pros:
- deterministic
- can track source plus selection together
- toolbar buttons and shortcuts share one command path
- easier to test

Cons:
- needs deduping and guardrails to avoid noisy snapshots
- requires explicit wiring around source changes and command mutations

## Decision

Choose Option B.

Add an editor history coordinator that stores bounded snapshots of source text plus selection. Push snapshots for meaningful user edits and toolbar mutations, allow undo/redo from buttons and key shortcuts, and keep autosave and outline rebuild on the same state-apply path.

## Risks

- pushing history on every render instead of real edits
- history loops when applying an undo result feeds back into source change handlers
- browser key handling conflicts with textarea defaults

## Mitigation

- centralize source mutations behind one apply method with an explicit history behavior
- suppress history capture while replaying undo/redo
- use targeted browser tests for click and keyboard flows
