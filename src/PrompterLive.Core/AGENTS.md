# AGENTS.md

## Purpose

`PrompterLive.Core` is the host-neutral domain layer.

It owns:

- TPS parsing, compilation, export
- RSVP helpers
- preview and workspace state
- media scene models
- streaming provider descriptors

## Entry Points

- `Services/TpsParser.cs`
- `Services/ScriptCompiler.cs`
- `Services/TpsExporter.cs`
- `Services/Rsvp/*`
- `Services/Workspace/ScriptSessionService.cs`
- `Services/Media/MediaSceneService.cs`
- `Services/Streaming/*`

## Boundaries

- No Blazor dependencies.
- No JavaScript interop.
- No browser or server runtime assumptions.
- Keep types serializable and reusable from the WebAssembly app.

## Commands

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/src/PrompterLive.Core/PrompterLive.Core.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/tests/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`

## Risks

- TPS compatibility matters more than cosmetic refactors.
- Do not let UI-specific shortcuts leak into domain parsing or RSVP behavior.
