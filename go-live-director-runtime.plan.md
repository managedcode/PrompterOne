# Go Live Director Runtime Plan

## Goal

Port the full `new-design/golive.html` studio screen into the routed Blazor `Go Live` feature with real browser-backed media/runtime behavior, honest live status data, real camera and microphone switching flows, updated architecture/ADR documentation, and browser-first automated coverage.

## Scope

### In Scope

- Rebuild the routed `Go Live` screen to match `new-design/golive.html` layout, structure, and interaction intent.
- Use real browser media state for camera cards, selected/program source, microphone state, live/record state, and timer-driven status.
- Wire the `Go Live` screen to actual scene cameras, microphones, destination arming, and runtime session state that already live in browser storage and media services.
- Ensure source switching, scene selection, panel toggles, and session controls update the real program/runtime state instead of static demo labels.
- Add or update an ADR and feature/architecture docs when the ownership or runtime contract changes.
- Add or update component and browser tests that prove the design-shaped flow with deterministic browser media.

### Out Of Scope

- Backend relay infrastructure or server-side transcoding.
- New external streaming providers beyond the browser-only/runtime contracts already modeled in the repo.
- Recorded video upload workflows beyond documenting how the `Go Live` runtime will align with the existing browser/cloud-storage direction.

## Constraints And Risks

- The routed UI must stay faithful to `new-design/golive.html`; partial approximation is not acceptable.
- Browser tests must keep using the deterministic synthetic media harness; no hardware dependency may leak into CI.
- `Go Live` already has runtime services and docs; the task must refine that ownership instead of introducing a parallel implementation path.
- Any fake telemetry, fake participants, or fake room state in the right rail must be removed or replaced by honest runtime state.
- `data-testid` hooks must remain stable and new ones must be added in shared contracts for any new acceptance coverage.
- Editor/teleprompter/storage work from prior tasks must not regress while reshaping cross-cutting media/runtime services.

## Testing Methodology

- Start with the existing `GoLive` component and browser suites to establish the real baseline.
- Add failing component tests for the design-accurate routed structure and runtime-driven status rendering.
- Add failing browser coverage for the main studio flow: open `Go Live`, verify real synthetic camera cards, switch program source, arm outputs, start/stop live session, and verify visual/runtime state changes.
- Capture major browser scenario screenshots under `output/playwright/`.
- Use the repo validation order: build, focused tests, browser suite, broader regression suite, coverage, format.

## Ordered Plan

- [x] Step 1. Inspect the current `Go Live` runtime against the design reference and identify exact deltas.
  - Compare `new-design/golive.html` and `styles-golive.css` with the current Blazor page, components, CSS, and existing feature docs.
  - Enumerate which existing cards/rails already map cleanly and which need markup/layout replacement, renamed state, or new contracts.
  - Verification before moving on:
    - A concrete delta list exists in working notes and the implementation path is scoped to `GoLive`, `Media`, and any shared contracts that truly need adjustment.

- [x] Step 2. Run the relevant baseline verification before production changes.
  - Run:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
  - Track any failing test explicitly under `## Baseline Failures` before fixing code.
  - Verification before moving on:
    - Baseline status is recorded and any inherited failure has a root-cause note and a fix path.

- [x] Step 3. Reshape the routed `Go Live` page and child components to match the design shell.
  - Rework `GoLivePage.razor`, child components, and feature CSS so the top bar, inputs panel, center canvas/program area, scenes bar, and right studio/info rail reflect `new-design/golive.html`.
  - Move or split markup into focused components only where that preserves parity and keeps files under repo limits.
  - Add/update shared UI test ids for new stable landmarks and interactions.
  - Verification before moving on:
    - The rendered structure mirrors the design sections.
    - No implementation literals or brittle selectors are introduced.
  - Completed evidence:
    - `GoLivePage.razor` now renders the compact top bar, left inputs rail, center stage, scene controls, and right live/studio rail instead of the old lower routing deck.
    - `GoLiveSourcesCard`, `GoLiveProgramFeedCard`, `GoLiveCameraPreviewCard`, `GoLiveSceneControls`, and `GoLiveStudioSidebar` were reshaped to match the design-owned studio shell.
    - Shared test id contracts were extended with `UiTestIds.GoLive.AddSource`.

- [x] Step 4. Replace placeholder/demo state with honest browser/runtime-driven `Go Live` data.
  - Audit `GoLivePage` partials, `GoLiveSessionService`, runtime services, and any right-rail models to remove fake personas, fake telemetry, and stale demo copy.
  - Bind camera/source cards, microphone routing labels, timer/status badges, and runtime panels to real `IMediaSceneService`, `IScriptSessionService`, and `GoLiveOutputRuntimeService` state.
  - Ensure empty/disabled states are explicit and honest when the browser runtime does not own a metric.
  - Verification before moving on:
    - The page renders only truthful runtime or persisted configuration state.
    - There are no hardcoded fake guests or fabricated network values in `GoLive`.
  - Completed evidence:
    - Fake team/guest-style content was removed from the `Go Live` surface.
    - Destination summaries now derive from real persisted `StudioSettings` readiness instead of inline placeholder fields.
    - The room and runtime tabs render honest local-host and available-runtime state only.

- [x] Step 5. Tighten real browser media and live control behavior.
  - Verify and fix source switching, scene selection, input arming, record/live toggles, and program monitor updates so they drive the browser runtime correctly.
  - Ensure selected camera/program state propagates through the live output request factory/runtime and updates the page immediately.
  - Fix any camera/microphone switching gaps that prevent the design-shaped controls from affecting the actual program/output state.
  - Verification before moving on:
    - Switching sources and toggling live/record state changes the runtime session state and visible program/preview indicators.
    - Real synthetic camera/mic data are used in browser flows without regressions.
  - Completed evidence:
    - `GoLivePage.Bootstrap.cs` now auto-seeds the first available camera into an empty scene.
    - `GoLivePage.Actions.cs` adds the next available browser camera from the real media-device list.
    - The center monitor now tracks the selected source while the right preview rail tracks the on-air source until `TakeToAir`.

- [x] Step 6. Add failing tests for the new contracts, then make them pass.
  - Add or update bUnit tests under `tests/PrompterLive.App.Tests/GoLive/` for the design-shaped top bar, rails, source/program status, and honest runtime info.
  - Add or update Playwright tests under `tests/PrompterLive.App.UITests/GoLive/` for the full director/studio flow with deterministic media, including screenshots under `output/playwright/`.
  - Keep constants and selectors in shared contracts/test constants rather than inline literals.
  - Verification before moving on:
    - New or updated tests fail before the code fix and pass after the implementation.
    - Browser coverage proves real interactions, not just static DOM.
  - Completed evidence:
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --filter "FullyQualifiedName~GoLivePageTests"` passed with `7/7`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~GoLiveFlowTests"` passed with `10/10`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --filter "FullyQualifiedName~StudioWorkflow_SettingsAndGoLiveStudio_CapturesArtifacts"` passed with `1/1`.

- [x] Step 7. Update docs and record the runtime/design decision.
  - Update `docs/Features/GoLiveRuntime.md` if the flow, runtime honesty policy, or contracts changed.
  - Add/update an ADR under `docs/ADR/` covering the design-faithful browser studio surface and the decision to keep `Go Live` truthful to real browser media/runtime state.
  - Update `docs/Architecture.md` if the ownership map or cross-slice boundaries changed.
  - Verification before moving on:
    - ADR and feature docs contain valid Mermaid diagrams.
    - Architecture guidance stays aligned with the implemented runtime.
  - Completed evidence:
    - `docs/Features/GoLiveRuntime.md` now documents the operational studio boundary and source-selection flow.
    - `docs/Architecture.md` now reflects that `Settings` owns provider setup while `Go Live` owns operational arming and switching.
    - `docs/ADR/ADR-0002-go-live-operational-studio-surface.md` records the design/runtime decision with Mermaid diagrams.

- [x] Step 8. Run final validation and prepare the change for shipping.
  - Run the final validation in order:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
    - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - Review the final diff for intentionality and update this plan with the completed validation evidence.
  - Verification before moving on:
    - All relevant tests are green.
    - The working tree contains only intentional `Go Live` task changes.
  - Completed evidence:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror` passed before and after the final cleanup pass.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj` passed with `100/100`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build` passed with `79/79`.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` passed with `34` core tests, `100` app tests, and `79` UI tests green.
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"` passed and emitted fresh Cobertura reports for core, app, and UI suites.
    - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx` completed successfully; the only console note was the existing `IDE0060` no-code-fix message, which did not fail the command.

## Baseline Failures

- [x] No pre-existing baseline failures in the relevant build, component, or browser suites.
  - Build: `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror` passed.
  - App tests: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj` passed with `100/100`.
  - UI tests: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build` passed with `77/77`.
  - Root-cause note: the current baseline is stable enough to start failing-forward on `Go Live` without inherited product failures.
  - Fix status: no baseline failures to clear before implementation.

## Final Validation Skills

- `dotnet-blazor`
  - Reason: keep the `Go Live` routed surface and component state aligned with the Blazor WASM ownership model.
  - Expected outcome: component-first implementation with minimal JS interop and correct state flow.

- `mcaf-testing`
  - Reason: add browser-first and component coverage for the full director/studio flow.
  - Expected outcome: meaningful tests for design structure, runtime honesty, and real media interaction.

- `mcaf-adr-writing`
  - Reason: record the design-faithful browser studio runtime decision and its trade-offs.
  - Expected outcome: an ADR with concrete alternatives, consequences, and verification strategy.

- `playwright`
  - Reason: verify the complete `Go Live` experience with deterministic synthetic media in a real browser.
  - Expected outcome: passing end-to-end browser flow with screenshot artifacts.
