# AGENTS.md

## Purpose

`PrompterLive.Shared.Tests` covers the shared Blazor UI with bUnit and lightweight fakes.

## Rules

- Test through public page behavior, not private implementation details.
- Prefer real `ScriptSessionService` and `MediaSceneService` with fake boundaries around storage, media permissions, devices, and JS runtime.
- Keep tests deterministic and host-independent.

## Command

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Shared.Tests/PrompterLive.Shared.Tests.csproj`
