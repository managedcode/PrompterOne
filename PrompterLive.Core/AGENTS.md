# AGENTS.md

## Purpose

`PrompterLive.Core` is the host-agnostic domain layer for TPS, RSVP, preview, workspace, media scene state, and streaming provider contracts.

## Entry Points

- `Services/TpsParser.cs`
- `Services/ScriptCompiler.cs`
- `Services/TpsExporter.cs`
- `Services/Preview/*`
- `Services/Rsvp/*`
- `Services/Workspace/ScriptSessionService.cs`
- `Services/Media/MediaSceneService.cs`
- `Services/Streaming/*`

## Boundaries

- Do not add Blazor, MAUI, or JavaScript dependencies here.
- Keep models serializable and host-neutral.
- If behavior belongs to parsing, compilation, preview, workspace state, or provider descriptions, it belongs here instead of in the UI.

## Commands

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Core/PrompterLive.Core.csproj`
- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`

## Risks

- TPS compatibility with the extracted legacy content is more important than pretty refactors.
- Keep streaming provider descriptors declarative. Publishing transport code does not belong in this project yet.
