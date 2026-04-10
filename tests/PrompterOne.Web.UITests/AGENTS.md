# AGENTS.md

## Project Purpose

`PrompterOne.Web.UITests` is the shared browser-harness and support base for the runnable `PrompterOne.Web.UITests.*` suites.

## Entry Points

- `Infrastructure/*`
- `Media/*`
- `Support/*`

## Boundaries

- This base project owns shared Playwright/browser-host infrastructure and reusable helper code; it must not accumulate routed feature test cases.
- This base project is a shared browser-harness library, not a runnable suite; do not reference the runnable TUnit engine package directly here.
- The runnable browser suites must still work with `dotnet test` only. Do not require env vars, custom ports, or manual app startup.
- The fixture self-hosts the built WASM assets on a dynamically assigned local loopback origin for Playwright.
- Each fixture startup MUST request a fresh OS-assigned loopback port. Never hardcode or reuse a fixed browser-test port across runs.
- The fixture also injects a deterministic synthetic media harness before page scripts run, so browser tests can verify camera and microphone flows without real hardware.
- Keep one authoritative implementation of browser-host constants, asset-path resolution, drivers, seeders, and artifact capture in this base project; do not duplicate them across suite projects.
- The browser suites resolve origin at runtime and use one `dotnet test` process at a time locally; lower the CI worker cap only when repeated full-suite runs prove resource contention.
- The runnable browser suites keep a lower CI worker cap than local runs, but when the user asks for throughput the in-suite CI worker cap should stay in the `6-9` range unless repeated full-suite runs prove a concrete blocker.
- Do not keep separate concurrent `dotnet build` or `dotnet test` processes alive against the same test assets.
- Prefer `PrompterOne.Shared.Contracts.AppRoutes`, `UiTestIds`, and other named constants over inline route or selector strings.
- Use one dedicated test-attribute format only: `data-test`.
- CSS-class selectors, style-driven selectors, and DOM-shape selectors are forbidden in browser assertions; add or reuse a dedicated `data-test` hook instead.
- Browser-test JavaScript snippets must receive dedicated `data-test` ids from C# constants and resolve nodes from those contracts, never from raw style/class selectors.
- Magic numbers in waits, delays, seeded values, and timeouts must be named constants.
- For in-app SPA route transitions that already perform an explicit route-ready wait after the click, prefer `UiInteractionDriver.ClickAndContinueAsync(..., noWaitAfter: true)` over Playwright's default click auto-wait so CI does not hang on client-side-only transitions that never trigger a full navigation event.

## Project-Local Commands

- `node ./tests/PrompterOne.Web.UITests/bin/Debug/net10.0/.playwright/package/cli.js install chromium`
- `dotnet build ./tests/PrompterOne.Web.UITests/PrompterOne.Web.UITests.csproj`

## Applicable Skills

- `playwright`

## Local Risks Or Protected Areas

- Do not let this base project become a dumping ground for feature-specific test cases.
- Keep selectors stable via dedicated test attributes whenever possible.
- Do not introduce or keep CSS-class selector dependencies in shared browser helpers.
- Production media permissions still depend on the stable launch-settings origin, but this synthetic browser harness must use the fixture-resolved dynamic loopback origin. Do not hardcode ports or require manual startup steps.
- Flaky browser tests are failures; fix the cause instead of weakening the assertion.
- Keep screenshot diagnostics first-class in the harness: normal scenario failures and early fixture/bootstrap failures should both leave Playwright screenshot artifacts whenever a page handle is still available.
- Do not duplicate route strings, test ids, or storage keys across tests; centralize them.
- Shared browser contexts may exist only for scenarios that explicitly validate cross-tab behavior, and they must not be published for reuse until storage reset and page priming have completed successfully; on bootstrap failure, evict and dispose the context immediately.
- Every mutating browser scenario must provision or import its own writable script or workspace state for that test run; parallel workers must not edit, autosave, or assert against the same writable document instance.
