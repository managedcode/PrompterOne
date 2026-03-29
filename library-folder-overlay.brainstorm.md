# library-folder-overlay.brainstorm

## Problem Framing

The current folder creation flow reuses a library card slot and expands into an inline form. That is functional, but it does not match the intended `new-design` feel and it produces a cramped layout that looks like a broken card instead of a deliberate create-folder interaction.

The requested behavior is a popup-style overlay:

- the grid stays intact
- creating a folder opens a centered dialog
- the dialog sits above a dimmed backdrop
- existing folder creation actions still work
- automated tests prove the overlay flow

## Options

### Option 1: Restyle the existing inline card

Pros:
- fewer markup changes
- minimal event rewiring

Cons:
- the form still lives in the grid flow
- harder to achieve true overlay behavior
- keeps layout pressure inside the card grid

### Option 2: Move folder creation into a dedicated modal component

Pros:
- matches the requested popup interaction
- clearer separation between grid content and modal action
- simpler to test backdrop, dialog, cancel, and submit behavior

Cons:
- requires moving the form markup out of `LibraryCardsGrid`
- needs new modal styles and test coverage

## Recommended Direction

Choose Option 2.

Create a dedicated library folder modal component, render it from `LibraryPage`, keep the create tile as a launcher only, and add focused tests for the overlay lifecycle and submission flow.

## Risks

- changing where the form lives can break existing test selectors
- modal overlay styles can accidentally cover or shift other library content

## Mitigations

- preserve the existing `data-testid` values for the input and action controls
- add a new overlay test id for explicit modal assertions
- update both bUnit and Playwright coverage so the flow is exercised through the real UI
