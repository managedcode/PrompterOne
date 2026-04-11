# AGENTS.md

## Project Purpose

`PrompterOne.Core` is the host-neutral domain layer.

It owns TPS parsing, compilation, export, RSVP helpers, preview and workspace state, media scene models, and streaming provider descriptors.

## Entry Points

- `Tps/*`
- `Editor/*`
- `Workspace/*`
- `Library/*`
- `Rsvp/*`
- `Media/*`
- `Streaming/*`
- `Localization/*`
- `AI/*`

## Boundaries

- No Blazor dependencies.
- No JavaScript interop.
- No browser or server runtime assumptions.
- Do not add browser-only, WebAssembly-only, or host-specific NuGet overrides here; keep those package pins in the runnable host project.
- Keep abstractions, models, preview helpers, and services colocated inside their owning feature slices.
- Keep types serializable and reusable from the WebAssembly app.

## Project-Local Commands

- `dotnet build ./src/PrompterOne.Core/PrompterOne.Core.csproj`
- `dotnet test ./tests/PrompterOne.Core.Tests/PrompterOne.Core.Tests.csproj`

## Applicable Skills

- no special skill is required for most core work; follow the root repo policy and architecture map first

## Local Risks Or Protected Areas

- TPS compatibility matters more than cosmetic refactors.
- `Models/` and legacy `Services/` roots should not become new flat dumping grounds; prefer the owning feature slice first.
- Do not let UI-specific shortcuts leak into domain parsing or RSVP behavior.
- AI agent infrastructure belongs under `AI/*`; keep one focused class per concrete agent type, colocate that agent's system prompt and skill references with the agent class, prefer one factory that assembles predefined agents and workflows over separate catalog layers, and wire skills or article context through official `AgentSkillsProvider` or `AIContextProvider` support instead of hand-built prompt concatenation.
- Respect root maintainability limits. Large parser/compiler edits need explicit decomposition or documented exceptions.
