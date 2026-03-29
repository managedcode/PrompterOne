# AGENTS.md

Project: `PrompterLive`
Stack: `.NET 10`, Blazor WebAssembly, MAUI Blazor Hybrid, Razor Class Library, xUnit, bUnit

## Purpose

This repo is the `PrompterLive` rebuild:

- `PrompterLive.Core` holds host-agnostic TPS, RSVP, preview, workspace, media-scene, and streaming contracts.
- `PrompterLive.Shared` holds the shared Blazor UI, browser interop layer, and shared DI composition.
- `PrompterLive.Web` is the primary acceptance runtime.
- `PrompterLive.Maui` is the Hybrid host scaffold for Android, iOS, Mac Catalyst, and Windows.

## Rules to Follow (Mandatory)

### Commands

- `build`: `...`
- `test`: `...`
- `format`: `...`
- `analyze`: `...` (delete if not used)
- `coverage`: `...` (delete if not used)

If the stack is `.NET`, also document:

- whether tests run on `VSTest` or `Microsoft.Testing.Platform`
- whether `format` is `dotnet format --verify-no-changes` or a checked-in wrapper over it
- whether coverage uses a VSTest collector, `coverlet.MTP`, or an MSTest SDK extension
- explicit `LangVersion` only when the repo intentionally differs from the SDK default

### Project AGENTS Policy

- Multi-project solutions MUST keep one root `AGENTS.md` plus one local `AGENTS.md` in each project or module root.
- Each local `AGENTS.md` MUST document:
  - project purpose
  - entry points
  - boundaries
  - project-local commands
  - applicable skills
  - local risks or protected areas
- If a project grows enough that the root file becomes vague, add or tighten the local `AGENTS.md` before continuing implementation.

### Maintainability Limits

These limits are repo-configured policy values. They live here so the solution can tune them over time.

- `file_max_loc`: `400`
- `type_max_loc`: `200`
- `function_max_loc`: `50`
- `max_nesting_depth`: `3`
- `exception_policy`: `Document any justified exception in the nearest ADR, feature doc, or local AGENTS.md with the reason, scope, and removal/refactor plan.`

Local `AGENTS.md` files may tighten these values, but they must not loosen them without an explicit root-level exception.

### Task Delivery

- Start from `docs/Architecture.md` and the nearest local `AGENTS.md`.
- Treat `docs/Architecture.md` as the architecture map for every non-trivial task.
- If the overview is missing, stale, or diagram-free, update it before implementation.
- Define scope before coding:
  - in scope
  - out of scope
- Keep context tight. Do not read the whole repo if the architecture map and local docs are enough.
- If the task matches a skill, use the skill instead of improvising.
- Analyze first:
  - current state
  - required change
  - constraints and risks
- For non-trivial work, create a root-level `<slug>.brainstorm.md` file before making code or doc changes.
- Use `<slug>.brainstorm.md` to capture the problem framing, options, trade-offs, risks, open questions, and the recommended direction.
- Think through the task in the brainstorm before committing to implementation details.
- After the brainstorm direction is chosen, create a root-level `<slug>.plan.md` file.
- Keep the `<slug>.plan.md` file as the working plan for the task until completion.
- The plan file MUST contain:
  - a link or reference to the chosen brainstorm
  - task goal and scope
  - a detailed implementation plan with detailed ordered steps
  - constraints and risks
  - explicit test steps as part of the ordered plan, not as a later add-on
  - the test and verification strategy for each planned step
  - the testing methodology for the task: what flows will be tested, how they will be tested, and what quality bar the tests must meet
  - an explicit full-test baseline step after the plan is prepared
  - a tracked list of already failing tests, with one checklist item per failing test
  - root-cause notes and intended fix path for each failing test that must be addressed
  - a checklist with explicit done criteria for each step
  - ordered final validation skills and commands, with reason for each
- Use the Ralph Loop for every non-trivial task:
  - brainstorm in `<slug>.brainstorm.md` before coding or document edits
  - think through options and choose the intended direction before planning
  - turn the chosen direction into a detailed `<slug>.plan.md`
  - include test creation, test updates, and verification work in the ordered steps from the start
  - once the initial plan is ready, run the full relevant test suite to establish the real baseline
  - if tests are already failing, add each failing test back into `<slug>.plan.md` as a tracked item with its failure symptom, suspected cause, and fix status
  - work through failing tests one by one: reproduce, find the root cause, apply the fix, rerun, and update the plan file
  - include ordered final validation skills in the plan file, with reason for each skill
  - require each selected skill to produce a concrete action, artifact, or verification outcome
  - execute one planned step at a time
  - mark checklist items in `<slug>.plan.md` as work progresses
  - review findings, apply fixes, and rerun relevant verification
  - update the plan file and repeat until done criteria are met or an explicit exception is documented
- Implement code and tests together.
- Run verification in layers:
  - changed tests
  - related suite
  - broader required regressions
- If `build` is separate from `test`, run `build` before `test`.
- After tests pass, run `format`, then the final required verification commands.
- The task is complete only when every planned checklist item is done and all relevant tests are green.
- Summarize the change, risks, and verification before marking the task complete.

### Documentation

- All durable docs live in `docs/` (or `.wiki/` if the repo already uses it).
- `docs/Architecture.md` is the required global map and the first stop for agents.
- `docs/Architecture.md` MUST contain Mermaid diagrams for:
  - system or module boundaries
  - interfaces or contracts between boundaries
  - key classes or types for the changed area
- Keep one canonical source for each important fact. Link instead of duplicating.
- Public bootstrap templates are limited to root-level agent files. Authoring scaffolds for architecture, features, ADRs, and other workflows live in skills.
- Update feature docs when behaviour changes.
- Update ADRs when architecture, boundaries, or standards change.
- For non-trivial work, the plan file, feature doc, or ADR MUST document the testing methodology:
  - what flows are covered
  - how they are tested
  - which commands prove them
  - what quality and coverage requirements must hold
- Every feature doc under `docs/Features/` MUST contain at least one Mermaid diagram for the main behaviour or flow.
- Every ADR under `docs/ADR/` MUST contain at least one Mermaid diagram for the decision, boundaries, or interactions.
- Mermaid diagrams are mandatory in architecture docs, feature docs, and ADRs.
- Mermaid diagrams must render. Simplify them until they do.

### Testing

- TDD is the default for new behaviour and bug fixes: write the failing test first, make it pass, then refactor.
- Bug fixes start with a failing regression test that reproduces the issue.
- Every behaviour change needs new or updated automated tests with meaningful assertions. New tests are mandatory for new behaviour and bug fixes.
- Tests must prove the real user flow or caller-visible system flow, not only internal implementation details.
- Tests should be as realistic as possible and exercise the system through real flows, contracts, and dependencies.
- Tests must cover positive flows, negative flows, edge cases, and unexpected paths from multiple relevant angles when the behaviour can fail in different ways.
- Prefer integration/API/UI tests over isolated unit tests when behaviour crosses boundaries.
- Do not use mocks, fakes, stubs, or service doubles in verification.
- Exercise internal and external dependencies through real containers, test instances, or sandbox environments that match the real contract.
- Flaky tests are failures. Fix the cause.
- Changed production code MUST reach at least 80% line coverage, and at least 70% branch coverage where branch coverage is available.
- Critical flows and public contracts MUST reach at least 90% line coverage with explicit success and failure assertions.
- Repository or module coverage must not decrease without an explicit written exception. Coverage after the change must stay at least at the previous baseline or improve.
- Coverage is for finding gaps, not gaming a number. Coverage numbers do not replace scenario coverage or user-flow verification.
- The task is not done until the full relevant test suite is green, not only the newly added tests.
- If the stack is `.NET`, document the active framework and runner model explicitly so agents do not mix VSTest and Microsoft.Testing.Platform assumptions.
- If the stack is `.NET`, after changing production code run the repo-defined quality pass: format, build, analyze, focused tests, broader tests, coverage, and any configured extra gates such as architecture, security, or mutation checks.

### Code and Design

- Everything in this solution MUST follow SOLID principles by default.
- Every class, object, module, and service MUST have a clear single responsibility and explicit boundaries.
- SOLID is mandatory.
- SRP and strong cohesion are mandatory for files, types, and functions.
- Prefer composition over inheritance unless inheritance is explicitly justified.
- Large files, types, functions, and deep nesting are design smells. Split them or document a justified exception under `exception_policy`.
- Hardcoded values are forbidden.
- String literals are forbidden in implementation code. Declare them once as named constants, enums, configuration entries, or dedicated value objects, then reuse those symbols.
- Avoid magic literals. Extract shared values into constants, enums, configuration, or dedicated types.
- Design boundaries so real behaviour can be tested through public interfaces.
- If the stack is `.NET`, the repo-root `.editorconfig` is the source of truth for formatting, naming, style, and analyzer severity. Use nested `.editorconfig` files when they serve a clear subtree-specific purpose. Do not let IDE defaults, pipeline flags, and repo config disagree.

### Critical

- Never commit secrets, keys, or connection strings.
- Never skip tests to make a branch green.
- Never weaken a test or analyzer without explicit justification.
- Never introduce mocks, fakes, stubs, or service doubles to hide real behaviour in tests or local flows.
- Never introduce a non-SOLID design unless the exception is explicitly documented under `exception_policy`.
- Never force-push to `main`.
- Never approve or merge on behalf of a human maintainer.

### Boundaries

Always:

- Read root and local `AGENTS.md` files before editing code.
- Read the relevant docs before changing behaviour or architecture.
- Run the required verification commands yourself.

Ask first:

- changing public API contracts
- adding new dependencies
- modifying database schema
- deleting code files

## Preferences

### Likes

### Dislikes

## Stable Repo Rules

- Treat `/Users/ksemenenko/Developer/Teleprompter/new-design/index.html` as the visual and interaction spec for the rebuilt UI.
- Keep all reusable script, RSVP, preview, media-scene, and streaming logic in `PrompterLive.Core`.
- Keep `PrompterLive.Shared` host-agnostic. Do not reference MAUI-only APIs from the RCL.
- WebAssembly is the first acceptance target. MAUI is scaffolded now, but deeper native certification is a later step.
- Streaming in this repo is provider-adapter driven. There is no backend service in this solution.
- Preserve and extend stable UI selectors (`data-testid`) on core user flows because the test scaffolding depends on them.

## Architecture Map

- Start with [docs/Architecture.md](/Users/ksemenenko/Developer/PrompterLive/docs/Architecture.md) before non-trivial work.
- Read the nearest local `AGENTS.md` before editing inside that project.

## Local AGENTS

- [PrompterLive.Core/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Core/AGENTS.md)
- [PrompterLive.Core.Tests/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Core.Tests/AGENTS.md)
- [PrompterLive.Shared/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Shared/AGENTS.md)
- [PrompterLive.Shared.Tests/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Shared.Tests/AGENTS.md)
- [PrompterLive.Maui/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui/AGENTS.md)
- [PrompterLive.Web.E2E.Tests/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Web.E2E.Tests/AGENTS.md)
- [PrompterLive.Maui.DeviceTests/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui.DeviceTests/AGENTS.md)
- [PrompterLive.Maui.UITests/AGENTS.md](/Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui.UITests/AGENTS.md)

## Commands

- `dotnet build PrompterLive.Core/PrompterLive.Core.csproj`
- `dotnet build PrompterLive.Web/PrompterLive.Web.csproj`
- `dotnet build PrompterLive.Maui/PrompterLive.Maui.csproj -f net10.0-android`
- `dotnet test PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
- `dotnet test PrompterLive.Shared.Tests/PrompterLive.Shared.Tests.csproj`
- `dotnet test PrompterLive.Web.E2E.Tests/PrompterLive.Web.E2E.Tests.csproj`
- `dotnet test PrompterLive.Maui.DeviceTests/PrompterLive.Maui.DeviceTests.csproj`
- `dotnet test PrompterLive.Maui.UITests/PrompterLive.Maui.UITests.csproj`
- `dotnet run --project PrompterLive.Web/PrompterLive.Web.csproj`
- `dotnet format PrompterLive.sln`

## Verification Policy

- Default green gate is the Web build plus the Core, Shared, Web.E2E, MAUI.DeviceTests, and MAUI.UITests projects.
- Local full-solution MAUI validation is constrained by the installed Apple toolchain.
- On this machine, iOS and Mac Catalyst builds require Xcode `26.2`; the installed version is `26.0.1`.
- If MAUI native builds fail on iOS or Mac Catalyst, report the Xcode/toolchain gap explicitly instead of masking it.

## Testing Guidance

- Use xUnit for pure logic and service tests.
- Use bUnit for shared page/component tests and keep JS interop mocked at the boundary.
- Use `WebApplicationFactory` host smoke tests in `PrompterLive.Web.E2E.Tests`.
- Keep MAUI test projects deterministic until real Appium/device runners are added.

## Skills

- `playwright` for browser verification of the running WebAssembly app.
