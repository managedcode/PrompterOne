# About And Teleprompter Fidelity Plan

## Task Goal

Remove invented hardcoded About content, replace it with factual Managed Code attribution and official links, then bring the teleprompter reading experience in line with `new-design/teleprompter.html` and the TPS specification so playback is visually smooth, timing-aware, and fully covered by automated tests.

## Scope

### In Scope

- Update the Settings About section so it no longer shows invented people and instead shows factual Managed Code ownership plus official links, including GitHub.
- Tighten teleprompter rendering and playback so active reading does not visibly jump while words/cards advance.
- Make teleprompter controls materially more visible during use while staying aligned with the design direction.
- Extend teleprompter rendering to reflect the TPS cues already produced by the compiler, including color, emotion, highlight, pronunciation cues, and speed-sensitive visual spacing.
- Add or update bUnit and Playwright coverage for About and teleprompter fidelity, including at least one end-to-end teleprompter scenario.
- Run build and relevant verification, then format, commit, and push.

### Out Of Scope

- Changing TPS parsing rules in `PrompterLive.Core` unless a concrete renderer gap requires a narrowly scoped compatibility fix.
- Redesigning routes or app shell behavior outside Settings/About and Teleprompter.
- Adding a backend or changing runtime hosting shape.

## Constraints And Risks

- `new-design/teleprompter.html` remains the visual reference; parity work should preserve its structure and interaction tone.
- Browser UI tests are the primary acceptance gate; teleprompter changes are not done until real-browser checks pass.
- The UI suite must run in a single `dotnet test` process and not overlap with other build/test commands.
- `About` must stay factual: no invented team members or stale attribution.
- Teleprompter smoothing must not create delayed input, unreadable word spacing, or layout churn from width-changing state changes.
- Files should stay within maintainability limits; if teleprompter logic needs more space, split it into focused partials/helpers instead of growing a large file further.

## Testing Methodology

- Use bUnit to verify About content contracts and teleprompter rendered markup/state mappings for TPS styling classes and metadata-driven output.
- Use Playwright UI tests to verify real browser playback, focal alignment, control visibility, timing continuity, and a full teleprompter scenario with screenshots under `output/playwright/`.
- Validate both static fidelity and dynamic behavior:
  - About shows factual Managed Code attribution and official links.
  - Teleprompter words preserve TPS-driven styling and pacing hints.
  - Playback advances without vertical jump artifacts on active text.
  - Card transitions stay smooth and time/progress continue advancing.
  - Controls remain visibly usable against live camera/gradient backgrounds.
- Quality bar:
  - No teleprompter regression in relevant bUnit coverage.
  - Relevant Playwright teleprompter flows green.
  - Repo build green under `-warnaserror`.

## Ordered Plan

- [x] Step 1. Establish baseline context and failures.
  - Read the exact About and teleprompter implementation/test files that own this work.
  - Run the relevant baseline commands in order:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
  - Verification before moving on:
    - Record every failing test and symptom below.
    - Confirm whether teleprompter failures reproduce before code changes.

- [x] Step 2. Replace hardcoded About content with factual Managed Code metadata.
  - Update `src/PrompterLive.Shared/Settings/Components/SettingsAboutSection.razor` and its code-behind to remove invented people and add factual company attribution plus official links, including GitHub.
  - Keep the section visually aligned with Settings design patterns and preserve existing test ids or add stable new ones if needed.
  - Verification before moving on:
    - Add/update bUnit assertions in `tests/PrompterLive.App.Tests/Settings/SettingsInteractionTests.cs`.
    - Confirm no stale invented names remain via targeted search.

- [x] Step 3. Expand teleprompter word rendering to honor TPS visual cues.
  - Update teleprompter reader models/rendering so words expose the cues already emitted by `ScriptCompiler`, including stronger distinctions for emphasis/highlight, pronunciation/tooltips, emotion/color mappings, and speed-sensitive spacing.
  - Keep speed-based letter spacing bounded so words never become mush or split into visually disconnected letters.
  - Verification before moving on:
    - Add/update bUnit tests under `tests/PrompterLive.App.Tests/Teleprompter/`.
    - Use TPS-backed sample scripts to prove slow/fast/xslow/xfast, highlight, pronunciation, and emotion styling render as expected.

- [x] Step 4. Remove teleprompter playback jumpiness and align transitions with the design.
  - Refine reader alignment/transition logic and any supporting browser interop so active-word tracking and card transitions stay smooth during playback.
  - Match the intended movement profile from `new-design/teleprompter.html` while avoiding text jumps during per-word advancement.
  - Verification before moving on:
    - Add/update Playwright assertions for continuity and alignment.
    - Capture a teleprompter scenario screenshot artifact under `output/playwright/`.

- [x] Step 5. Make teleprompter controls visibly usable.
  - Update the relevant reader CSS modules so sliders, edge info, and control bar stay visible enough against the background instead of fading into near-invisibility.
  - Keep the visual language aligned with the design reference while improving usability.
  - Verification before moving on:
    - Add/update browser checks that confirm control opacity/visibility at runtime.

- [x] Step 6. Add a full teleprompter browser scenario.
  - Add or extend a real Playwright scenario that opens a TPS-backed teleprompter script, starts playback, verifies styling/timing/progress/controls, and saves screenshots.
  - Verification before moving on:
    - Scenario passes in the browser suite.
    - Screenshot artifacts are written under `output/playwright/`.

- [x] Step 7. Run final validation and ship.
  - Run the required verification in order:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
    - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - If green, commit with a focused message and push the current branch.
  - Verification before moving on:
    - All planned checklist items are complete.
    - Working tree is clean except for intentional artifacts, if any.

## Baseline Failures

- [x] No pre-existing baseline failures in the relevant build, bUnit, or UI suites.
  - Build: `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror` passed.
  - bUnit: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj` passed with `94/94`.
  - UI: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build` passed with `75/75`.
  - Root-cause note: current repo baseline is green; this task will add targeted regression coverage for the requested About and teleprompter behavior.
  - Fix status: no inherited failures to clear before implementation.

## Intended Fix Tracking

- [x] About invented team content removed and replaced with factual Managed Code attribution.
- [x] Teleprompter TPS styling parity expanded beyond the current reduced class mapping.
- [x] Teleprompter playback jumpiness eliminated or reduced to non-visible smooth motion.
- [x] Teleprompter controls made clearly visible during live use.
- [x] Full teleprompter browser scenario added or extended with screenshot artifacts.

## Final Validation Skills

- `dotnet`
  - Reason: enforce repo-compatible build, test, and format commands for this Blazor/.NET solution.
  - Expected outcome: green build/test/format evidence aligned with `AGENTS.md`.

- `playwright`
  - Reason: validate teleprompter behavior in a real browser and capture screenshot artifacts for the changed flow.
  - Expected outcome: passing teleprompter scenario coverage with browser-realistic evidence.

## Final Validation Results

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
  - Result: passed after implementation and again after formatting.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
  - Result: passed with `34/34`.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
  - Result: passed with `95/95`.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
  - Result: passed with `76/76`.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - Result: passed for the full solution.
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
  - Result: passed for the full solution with coverage artifacts emitted for Core, App, and UI suites.
- `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - Result: passed.

## Notes

- `About` now uses only Managed Code and PrompterLive official links and removes invented roster content from both production UI and the design reference.
- Teleprompter playback now pre-centers upcoming content before card activation to avoid visible jump on live reading transitions.
- TPS shorthand inline speed tags like `[180WPM]...[/180WPM]` are now preserved by the compiler so reader timing matches TPS input.
- End-to-end browser evidence includes the teleprompter full-flow screenshots under `output/playwright/teleprompter-product-launch/`.
