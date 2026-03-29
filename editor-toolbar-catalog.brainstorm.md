# Editor Toolbar Catalog Brainstorm

## Problem

The editor authoring flow is now body-only and interactive, but the toolbar surface is still mostly hand-written markup with duplicated command wiring. That leaves three problems:

- too much hardcoded button/token behavior in the Razor file
- incomplete proof that every toolbar section works in the browser
- missing parity details like the "remove color" action from `new-design`

The next pass should make the editor toolbar maintainable and prove that the full command surface is actually usable.

## In Scope

- move the toolbar and floating-bar command definitions into a reusable catalog or descriptor layer
- keep the rendered markup visually aligned with `new-design`
- add missing actionable items that are currently only visual parity gaps
- add broad browser tests that exercise every toolbar section and the floating bar

## Out Of Scope

- replacing the editor with a JS-heavy contenteditable implementation
- backend AI or collaborative authoring
- redesigning the editor visuals away from `new-design`

## Constraints

- keep the runtime standalone WASM
- preserve existing `data-testid` stability where it already exists
- do not mutate `new-design/`
- keep editor commands deterministic and testable without service doubles

## Options

### Option A: Keep the current hand-written Razor toolbar and just bolt on more tests

Pros:

- smallest code change

Cons:

- keeps duplication
- leaves the hardcoded surface largely intact
- makes future additions brittle

### Option B: Introduce a catalog-driven toolbar model and render sections/actions from descriptors

Pros:

- removes a large amount of repeated command markup
- centralizes tokens, labels, tips, test ids, and command semantics
- makes it practical to test and extend the whole toolbar surface

Cons:

- moderate refactor across the editor component tree

## Recommended Direction

Choose Option B.

Create a descriptor-driven toolbar model for:

- toolbar sections
- main-row actions
- dropdown actions
- floating selection actions

Use the catalog to render the toolbar and floating bar while preserving `new-design` classes. Add a deterministic `remove color` command and cover the full editor command surface with browser tests.

## Risks

- refactoring markup can accidentally drift visually from `new-design`
- generated test ids can break existing Playwright flows
- command abstraction can get too generic and harder to debug

## Mitigations

- keep the exact CSS classes and rendered structure
- preserve existing test ids and extend them predictably
- keep descriptor types simple and editor-specific
