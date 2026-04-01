# Teleprompter Jump Fix Plan

## Goal

Fix teleprompter text jumping in read mode so the opening card positions smoothly, line tracking stays stable while words advance, and card transitions match `/Users/ksemenenko/Developer/PrompterOne/new-design/teleprompter.html` without visible vertical stutter.

## Scope

### In Scope

- Reproduce the teleprompter jump in the real browser runtime and in automated tests.
- Compare current teleprompter alignment and playback logic against `new-design/teleprompter.html` and related JS.
- Fix initial card pre-positioning so the first visible reader block does not appear late or snap into place after render.
- Fix per-word vertical alignment updates so words on the same visual line do not cause visible paragraph jumps.
- Add or update browser and supporting tests that prove the bug is gone.
- Update reader feature docs only if the runtime contract changes materially.

### Out Of Scope

- TPS parsing changes unrelated to teleprompter alignment.
- Go Live, Learn, Editor, or Settings changes unless they are required to support the teleprompter fix.
- Visual redesign outside the existing `new-design` teleprompter reference.

## Constraints And Risks

- `new-design/teleprompter.html` and the reader logic in `new-design/app.js` remain the motion and positioning source of truth.
- Browser acceptance is the main gate; this fix is not done if the browser still shows stutter even when component tests pass.
- Teleprompter still has to preserve TPS styling, pronunciation, colors, and speed-derived spacing while fixing motion.
- Avoid introducing JS-owned state that duplicates Blazor playback ownership.
- Keep files and methods inside repo maintainability limits unless an explicit exception is documented.

## Testing Methodology

- Reproduce the bug in a real browser against the actual app runtime before changing code.
- Add a failing browser regression that detects transform or focal-line instability across word steps and card transitions.
- Add a focused supporting test only where it helps isolate the alignment contract or rendered style state.
- Verify the final fix with focused teleprompter suites first, then broader solution validation.

## Ordered Plan

- [x] Step 1. Inspect the current teleprompter runtime against the design reference and locate the jump path.
  - Read `docs/Architecture.md`, `docs/Features/ReaderRuntime.md`, `new-design/teleprompter.html`, and the reader functions in `new-design/app.js`.
  - Inspect the current `TeleprompterPage` playback, rendering, alignment, and CSS modules that affect `rd-card` and `rd-cluster-text`.
  - Verification before moving on:
    - The suspected causes for first-card lag and per-word jump are written down with the owning files.
  - Findings:
    - `src/PrompterOne.Shared/wwwroot/teleprompter/teleprompter-reader.js` mutates `text.style.transform` to `none` during every alignment measurement and then restores it, unlike the intended no-flash reference behavior from `new-design/app.js`.
    - `src/PrompterOne.Shared/Teleprompter/Pages/TeleprompterPage.ReaderAlignment.cs` requests alignment on each playback state change, so the helper mutation is exercised on every word advance.
    - The visible bounce is on the existing `p.rd-cluster-text` node, not from Blazor replacing the paragraph element.

- [x] Step 2. Run the relevant baseline and reproduce the jump in the browser.
  - Run:
    - `dotnet build /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.Tests/PrompterOne.App.Tests.csproj --filter "FullyQualifiedName~Teleprompter"`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --filter "FullyQualifiedName~Teleprompter"`
  - Reproduce the issue in a real browser runtime and capture the current transform/position behavior for the first card and several advancing words.
  - Track any existing failures under `## Baseline Failures`.
  - Verification before moving on:
    - The jump is reproduced with concrete evidence, not only inferred from code.
  - Evidence:
    - `dotnet build /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx -warnaserror` passed.
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.Tests/PrompterOne.App.Tests.csproj --filter "FullyQualifiedName~Teleprompter"` passed with `7/7`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --filter "FullyQualifiedName~Teleprompter"` passed with `9/9`.
    - Live browser probing against `http://localhost:5040/teleprompter?id=test-product-launch-script` reproduced the issue: the same `#rd-card-text-0` element kept its identity, but on each word click its `top` jumped from the settled aligned position back to the unshifted paragraph position before animating down again.
    - Example reproduction for the existing paragraph node:
      - before click: `transform=translateY(-72.21px)`, `top=278.10`
      - immediately after click: `transform=translateY(-72.21px)`, `top=350.31`
      - after settle: `transform=translateY(-72.21px)`, `top=278.10`

- [x] Step 3. Add a failing regression for the visible jump.
  - Extend the teleprompter browser tests to assert stable line position during same-line word progression and smooth first-card/card-transition alignment.
  - Add supporting component coverage only if a render-state contract needs direct protection.
  - Verification before moving on:
    - At least one new or updated test fails on the current implementation for the reported jump.
  - Result:
    - Added `TeleprompterDemo_KeepsParagraphStableWhenFirstWordsAdvance` in `tests/PrompterOne.App.UITests/Teleprompter/TeleprompterFidelityTests.cs`.
    - The new regression failed on the old implementation with `Assert.InRange() Failure` and a measured paragraph motion delta of `36.41px`.
    - Added `TeleprompterDemo_KeepsParagraphStableWhenFontSizeChanges` in `tests/PrompterOne.App.UITests/Teleprompter/TeleprompterFidelityTests.cs`.
    - The font-size regression failed on the old implementation with `Assert.InRange() Failure` and a measured paragraph motion delta of `9.31px`.

- [x] Step 4. Fix alignment and playback sequencing to match the reference.
  - Adjust teleprompter alignment measurement and/or playback sequencing in `TeleprompterPage.ReaderAlignment.cs`, `TeleprompterPage.ReaderPlayback.cs`, `teleprompter-reader.js`, and any necessary reader CSS so:
    - first-card positioning is ready before the card becomes visibly active
    - same-line word changes do not trigger visible paragraph jumps
    - card transitions stay pre-centered and smooth
  - Keep Blazor as the playback/state owner and JS as a measurement helper only.
  - Verification before moving on:
    - The new browser regression passes and manual browser inspection no longer shows the reported jumping.
  - Result:
    - Reworked `src/PrompterOne.Shared/wwwroot/teleprompter/teleprompter-reader.js` to compute the desired paragraph `translateY` from the current measured layout and current transform matrices without resetting DOM transforms during measurement.
    - Updated `src/PrompterOne.Shared/Teleprompter/Pages/TeleprompterPage.ReaderAlignment.cs` to support one-shot no-transition alignment application for layout-driven updates, then restore normal transition behavior on the following render.
    - Updated `src/PrompterOne.Shared/Teleprompter/Pages/TeleprompterPage.ReaderPlayback.cs` so font-size changes request instant alignment rather than animated realignment.
    - Live probing reported `delta=0` for the first three manual word advances on the opening card before the font-size follow-up work, and the paragraph node no longer returns to the unshifted position between steps.

- [x] Step 5. Run focused teleprompter validation.
  - Run the updated teleprompter component and browser suites.
  - Capture updated screenshot artifacts if the browser scenario changes materially.
  - Verification before moving on:
    - Teleprompter-focused tests are green and the visual bug is cleared in the browser.
  - Result:
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.Tests/PrompterOne.App.Tests.csproj --filter "FullyQualifiedName~Teleprompter"` passed with `7/7`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --filter "FullyQualifiedName~Teleprompter" --no-build` passed with `11/11`.
    - Both browser regressions for per-word advance and font-size changes are green locally.

- [ ] Step 6. Update durable docs if the runtime contract changed.
  - Update `docs/Features/ReaderRuntime.md` only if the alignment contract or verification expectations changed materially.
  - Verification before moving on:
    - Documentation stays accurate, with Mermaid still valid if touched.

- [ ] Step 7. Run final validation and prepare the local fix for user review.
  - Run:
    - `dotnet build /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.Tests/PrompterOne.App.Tests.csproj --filter "FullyQualifiedName~Teleprompter"`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/tests/PrompterOne.App.UITests/PrompterOne.App.UITests.csproj --filter "FullyQualifiedName~Teleprompter"`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx`
    - `dotnet test /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx --collect:"XPlat Code Coverage"`
    - `dotnet format /Users/ksemenenko/Developer/PrompterOne/PrompterOne.slnx`
  - Update this plan with the completed evidence.
  - Verification before moving on:
    - All relevant tests are green and the working tree contains only intentional teleprompter-fix changes.

## Baseline Failures

- [x] No baseline test failures in the focused teleprompter suites, but the reported browser jump reproduces outside current assertions.
  - Root-cause note: `teleprompter-reader.js` resets the paragraph transform during alignment measurement, which produces a visible bounce even when the target `translateY(...)` value does not change.
  - Fix status: fixed locally and ready for user verification before broader validation and git work.

## Final Validation Skills

- `dotnet-blazor`
  - Reason: keep playback, render sequencing, and JS interop ownership correct for the Blazor WASM teleprompter.
  - Expected outcome: a Blazor-owned fix with minimal and targeted JS measurement changes.

- `mcaf-testing`
  - Reason: add a reliable regression for the visible jump and validate the real user flow.
  - Expected outcome: failing-first regression coverage and green focused suites after the fix.

- `playwright`
  - Reason: reproduce and clear the teleprompter jump in a real browser.
  - Expected outcome: browser evidence that first-card and per-word playback are smooth.
