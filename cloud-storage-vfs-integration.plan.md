# Cloud Storage VFS Integration Plan

## Task Goal

Integrate `ManagedCode.Storage.Browser` plus `ManagedCode.Storage.VirtualFileSystem` for cloud-import/export and future browser-local blob workflows, add runtime-configurable cloud provider connections in Settings for Browser, CloudKit, Dropbox, Google Cloud Storage, Google Drive, and OneDrive, persist provider credentials in browser `localStorage`, and enable import/export of scripts and app settings through those providers without regressing the primary editor/library runtime flows. Record the architectural decision in an ADR before the task is closed.

## Scope

### In Scope

- Add the required `ManagedCode.Storage.*` packages and wire the Storage library into the Blazor WASM host.
- Introduce a storage abstraction for cloud/import-export and browser-local blob workflows built on `ManagedCode.Storage.Browser` and `IVirtualFileSystem`, while preserving a stable primary runtime store for scripts and folders.
- Add Settings UI and state for the requested providers:
  - `ManagedCode.Storage.Browser`
  - `ManagedCode.Storage.CloudKit`
  - `ManagedCode.Storage.Dropbox`
  - `ManagedCode.Storage.Google`
  - `ManagedCode.Storage.GoogleDrive`
  - `ManagedCode.Storage.OneDrive`
  - `ManagedCode.Storage.VirtualFileSystem`
- Persist provider connection credentials and metadata in browser `localStorage`.
- Implement import/export flows for scripts and settings through the configured providers.
- Add automated coverage for the new storage/runtime contracts, Settings UI, and at least one real browser flow through Settings.
- Write an ADR that documents the local-browser-plus-cloud storage direction and its trade-offs.

### Out Of Scope

- Uploading or archiving recorded video streams.
- Server-side secret storage, backend APIs, or hosted sync services.
- End-user OAuth popup or redirect flows beyond fields and provider wiring; user-supplied credentials/tokens remain the auth source for this task.
- Reworking unrelated Library, Editor, Teleprompter, or Go Live UI beyond storage-backed behavior needed by this change.

## Constraints And Risks

- The app must remain browser-only WASM; no backend may be introduced to support cloud storage.
- Browser `localStorage` is the canonical store for provider keys/tokens/connection metadata in this runtime shape, even though that increases client-side credential exposure; the ADR must document that risk explicitly.
- Settings and Library flows already depend on stable `data-testid` contracts; any new cloud actions must add production-owned test ids instead of brittle selectors.
- `ManagedCode.Storage.Browser` uses IndexedDB/OPFS, so the browser-suite acceptance tests must continue to self-host and run without manual setup.
- Cloud providers have heterogeneous credential models; the runtime service must validate missing required fields safely and surface clear diagnostics instead of crashing the page.
- `ManagedCode.Storage.VirtualFileSystem` metadata updates are provider-dependent; this task should keep file layout simple and avoid over-relying on metadata-only directory semantics.

## Testing Methodology

- Use core/component tests to lock down:
  - browser-local script/settings persistence through the new storage-backed services
  - provider credential persistence in browser `localStorage`
  - import/export orchestration for scripts and settings
  - Settings UI rendering and state transitions for provider cards and actions
- Use Playwright to verify:
  - Settings Cloud section exposes provider controls and persisted values
  - provider credentials survive reload via `localStorage`
  - script/settings export and import round-trips through the browser-backed local storage provider
  - screenshot artifacts land under `output/playwright/`
- Quality bar:
  - new storage services covered by automated tests with meaningful success and failure assertions
  - browser Settings flow green with real clicks and persisted state
  - repo build/test/format/coverage remain green

## Ordered Plan

- [x] Step 1. Establish the exact baseline for the affected solution areas.
  - Read the current browser repositories, Settings Cloud UI, storage keys/method names, and app DI registration.
  - Run the relevant baseline commands in order:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
  - Verification before moving on:
    - Record any pre-existing failures below with symptoms and intended handling.
    - Confirm the current Settings Cloud section is still only stub content before refactoring.

- [x] Step 2. Add the Storage package dependencies and runtime wiring.
  - Update `Directory.Packages.props` and project files with `ManagedCode.Storage.Browser`, `ManagedCode.Storage.CloudKit`, `ManagedCode.Storage.Dropbox`, `ManagedCode.Storage.Google`, `ManagedCode.Storage.GoogleDrive`, `ManagedCode.Storage.OneDrive`, and `ManagedCode.Storage.VirtualFileSystem`.
  - Extend `PrompterLiveServiceCollectionExtensions` with browser storage and VFS registration that matches the WASM runtime and keeps local browser storage as the default local provider.
  - Verification before moving on:
    - Build succeeds.
    - New services resolve in the bUnit harness without startup failures.

- [x] Step 3. Rework the local persistence boundary so storage integration does not regress runtime editing flows.
  - Introduce a focused storage service layer that owns provider wiring, local browser containers, and snapshot transfer logic for scripts, folders, and settings.
  - Keep `BrowserScriptRepository` and `BrowserLibraryFolderRepository` on an authoritative browser JSON/localStorage path with explicit materialization/versioning once the VFS-backed primary path proved unstable under real editor autosave flows.
  - Keep `localStorage` limited to provider credentials/metadata, lightweight settings, and the authoritative runtime repository payloads that must remain stable in the browser-only app shape.
  - Verification before moving on:
    - Add or update tests for local script/folder/settings persistence and legacy bootstrap behavior.
    - Confirm Library and Settings still load their existing defaults in tests.

- [x] Step 4. Add provider connection models and secure-enough browser persistence for runtime credentials.
  - Add provider configuration models, browser persistence services, and validation for Browser, CloudKit, Dropbox, Google Cloud Storage, Google Drive, and OneDrive.
  - Persist only provider credentials/metadata in browser `localStorage` behind named storage keys and services.
  - Verification before moving on:
    - Add/update component tests that prove connection models save/load correctly and invalid configurations are rejected safely.
    - Verify no raw credential literals are duplicated in implementation code.

- [x] Step 5. Build cloud storage runtime services for scripts/settings import-export.
  - Create a cloud storage orchestration service that can instantiate the requested provider at runtime from stored options and copy scripts/settings between local browser storage and the selected cloud provider.
  - Use `IVirtualFileSystem` for local file layout and for cloud-facing file/directory operations where it simplifies implementation.
  - Keep the feature limited to scripts and settings for now.
  - Verification before moving on:
    - Add automated tests for provider selection, import/export success, missing-credentials failures, and path layout.
    - Validate at least the browser-local provider round-trip without mocks.

- [x] Step 6. Replace the stub Settings Cloud UI with real provider configuration and actions.
  - Rewrite `SettingsCloudSection.razor` and the owning `SettingsPage` partials so the Cloud section exposes provider cards, credential fields, connect/disconnect state, sync preferences, and import/export actions for scripts/settings.
  - Add stable `UiTestIds` for the new controls and results.
  - Keep the layout aligned with the Settings design language instead of shipping a purely utilitarian panel.
  - Verification before moving on:
    - Add/update bUnit assertions for Cloud section state, provider toggles, credential fields, and actions.
    - Confirm the new UI contracts are all test-id addressable.

- [x] Step 7. Document the architecture decision in an ADR and update architecture docs if boundaries changed.
  - Write `docs/ADR/ADR-0001-browser-local-and-cloud-storage-vfs.md` documenting the browser-local default storage, localStorage credential policy, provider integration path, and deferred video-stream use case.
  - Update `docs/Architecture.md` if the new storage service layer changes where contributors should place persistence and cloud-sync code.
  - Verification before moving on:
    - ADR has at least one Mermaid diagram and explicit verification methodology.
    - Architecture doc reflects the new persistence boundary if ownership changed.

- [x] Step 8. Add browser acceptance coverage and ship.
  - Add or update a real Playwright Settings scenario that exercises the Cloud section, persists provider values, and proves scripts/settings import-export through the browser-backed provider.
  - Capture screenshots under `output/playwright/`.
  - Run the final validation in order:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
    - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
  - Verification before moving on:
    - All checklist items are complete.
    - Working tree contains only intentional changes.
  - Completed validation:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
    - `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx --collect:"XPlat Code Coverage"`
    - `dotnet format /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`

## Baseline Failures

- [x] No pre-existing baseline failures in the relevant build, core, component, or browser suites.
  - Build: `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx -warnaserror` passed.
  - Core tests: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj --no-build` passed with `34/34`.
  - App tests: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --no-build` passed with `95/95`.
  - UI tests: `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build` passed with `76/76`.
  - Root-cause note: an initial baseline attempt produced file-lock errors because multiple `dotnet` commands were launched in parallel against the same output folders; rerunning sequentially confirmed the repo baseline is green.
  - Fix status: no inherited product failures need to be cleared before implementation.

## Intended Fix Tracking

- [x] Browser storage package and VFS wired into the WASM runtime.
- [x] Local script and settings persistence now uses the new storage boundary without regressing runtime editor/library flows.
- [x] Provider credentials stored via dedicated browser localStorage services.
- [x] Settings Cloud section exposes real provider configuration instead of stub cards.
- [x] Scripts and settings import/export work through the configured providers.
- [x] ADR documents the new persistence/cloud boundary and risks.

## Review Notes

- [x] UI-test harness now resolves package `_content/*` assets from `staticwebassets.development.json`, which cleared the storage-package bootstrap failure in `PrompterLive.App.UITests`.
- [x] The selected primary cloud provider now auto-opens its settings card so reload and first-use flows expose the active provider state without manual accordion recovery.

## Validation Failures To Clear

- [x] `LibraryScreenFlowTests.LibraryScreen_CreatesFolderAndMovesScript`
  - Symptom: `library-new-folder-overlay` remained visible after the cancel/create flow in the full browser suite.
  - Root cause: folder creation and move flows were relying on full reload timing instead of immediate local view-state updates.
  - Fix status: cleared by local folder/script state application and explicit view-state persistence.

- [x] `EditorInteractionTests.EditorScreen_FloatingToolbarShowsAiAndPersistsSelectionFormatting`
  - Symptom: applying `Slow` after selecting `transformative moment` used stale editor selection state and did not update the intended text span.
  - Root cause: toolbar commands could run before the textarea selection state was refreshed from the DOM.
  - Fix status: cleared by explicit selection refresh before non-toggle toolbar actions.

- [x] `EditorInteractionTests.EditorScreen_FullToolbarSurfaceSupportsExtendedCommands`
  - Symptom: applying `Professional` after reselecting `transformative moment` wrapped the earlier `welcome` selection instead of the current target.
  - Root cause: the same stale selection race as the floating-toolbar failure.
  - Fix status: cleared by the same source-panel selection refresh path.

- [x] `EditorTypingTests` and `EditorSourceSyncTests` full browser regressions
  - Symptom: structure tree and first untitled-draft route assignment were unstable after the storage integration work.
  - Root cause: the VFS-backed primary repository path caused first-save hangs in the real WASM autosave flow, and the untitled autosave path needed an explicit minimum-content guard for stability under parallel browser load.
  - Fix status: cleared by restoring an authoritative browser JSON/localStorage repository path for runtime scripts/folders and by deferring untitled autosave until the draft has at least two non-whitespace characters.

## Final Validation Skills

- `dotnet-blazor`
  - Reason: keep the storage and Settings integration aligned with the Blazor WASM runtime and component boundaries.
  - Expected outcome: routed UI and service wiring stay component-owned and browser-safe.

- `mcaf-adr-writing`
  - Reason: record the browser-local plus cloud-provider storage decision, risks, and future video-upload direction.
  - Expected outcome: an ADR with explicit trade-offs, implementation impact, and verification strategy.

- `playwright`
  - Reason: prove the Settings Cloud flow and browser-local storage behavior in a real browser.
  - Expected outcome: passing acceptance flow with screenshot artifacts.
