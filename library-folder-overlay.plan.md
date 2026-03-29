# library-folder-overlay.plan

Chosen brainstorm: `library-folder-overlay.brainstorm.md`

## Goal

Replace the inline folder-create card with a proper overlay dialog that matches the intended design and keep the library creation flow fully covered by automated tests.

## Scope

In scope:
- move folder creation markup into a modal overlay component
- keep launchers working from the library tile and sidebar button
- add or update automated tests for opening, cancelling, and submitting the overlay

Out of scope:
- broader library redesign outside the folder-create flow
- folder rename or delete UX

## Constraints And Risks

- preserve the existing user flow and selectors where possible
- keep the standalone WASM runtime behavior unchanged outside the create-folder surface
- avoid hardcoded one-off JS for modal behavior

## Testing Methodology

Flows covered:
- open create-folder overlay from the library tile
- cancel the overlay cleanly
- submit the overlay and create a folder in the selected parent

How they are tested:
- bUnit for rendered markup and repository state
- Playwright for live overlay visibility and end-to-end folder creation

Quality bar:
- modal overlay must be visible as a dedicated layer, not as an inline grid card
- create and cancel flows must both be automated

## Ordered Plan

- [x] Step 1. Write the brainstorm and choose the direction.
- [x] Step 2. Write this plan.
- [x] Step 3. Move folder creation from the card grid into a dedicated overlay component.
  Verification: run focused bUnit library tests.
- [x] Step 4. Add or update tests for modal open, cancel, and submit flows.
  Verification: rerun focused bUnit and focused Playwright library tests.
- [x] Step 5. Run final validation.
  Verification:
  - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --filter "FullyQualifiedName~LibraryFolderInteractionTests"`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~ScreenFlowTests"`
  - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
  - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`

## Execution Notes

- Folder creation moved out of `LibraryCardsGrid` and into a dedicated overlay component rendered by `LibraryPage`.
- The grid launcher tile stays visible and now opens a centered modal instead of mutating into an inline card form.
- Focused verification passed:
  - `LibraryFolderInteractionTests`: `4/4`
  - `ScreenFlowTests`: `5/5`
- Final verification passed:
  - `dotnet build`: `0 warnings`, `0 errors`
  - `PrompterLive.App.Tests`: `28/28`
  - `PrompterLive.App.UITests`: `17/17` in `3m49s`
- `dotnet format` completed; the repo still has existing non-auto-fixable analyzer items outside this task (`IDE0060`, `CA1305`, `CA1826`).

## Baseline Failures

- None recorded before this overlay pass.
