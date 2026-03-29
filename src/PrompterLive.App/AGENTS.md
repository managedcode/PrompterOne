# AGENTS.md

## Purpose

`PrompterLive.App` is the standalone Blazor WebAssembly host.

## Entry Points

- `Program.cs`
- `App.razor`
- `wwwroot/index.html`

## Boundaries

- Keep this project as a thin host.
- Do not add a server runtime here.
- Prefer putting routed UI in `PrompterLive.Shared`.

## Commands

- `dotnet build /Users/ksemenenko/Developer/PrompterLive/src/PrompterLive.App/PrompterLive.App.csproj`
- `dotnet run --project /Users/ksemenenko/Developer/PrompterLive/src/PrompterLive.App/PrompterLive.App.csproj --urls http://127.0.0.1:5187`

## Risks

- Any dependency that assumes ASP.NET server hosting is a red flag.
- Keep static asset references aligned with `PrompterLive.Shared`.
