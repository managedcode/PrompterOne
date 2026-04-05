# AGENTS.md

## Project Purpose

`PrompterOne.TpsSdk` is the repo-owned vendored copy of the upstream TPS .NET SDK.

It exists so `PrompterOne` can track upstream TPS semantics from source while keeping the app build self-contained and independent from external local repo paths.

## Entry Points

- `TpsRuntime.cs`
- `TpsSpec.cs`
- `TpsPlayer.cs`
- `TpsPlaybackSession.cs`
- `TpsStandalonePlayer.cs`
- `Internal/*`
- `Models/*`

## Boundaries

- Keep this project as close to the upstream TPS SDK source as practical.
- Prefer adaptation in `PrompterOne.Core` over ad-hoc modifications here unless the vendored source itself needs a repo-owned extension point.
- Do not let Blazor, browser, or app-shell concerns leak into this project.
- Keep the upstream `ManagedCode.Tps` namespace so future syncs stay low-friction.

## Project-Local Commands

- `dotnet build ./src/PrompterOne.TpsSdk/PrompterOne.TpsSdk.csproj`

## Applicable Skills

- no project-specific skill beyond the repo-root .NET workflow

## Local Risks Or Protected Areas

- Namespace churn here creates expensive future sync work; avoid it.
- If repo-owned wrapper APIs are needed, prefer adding them in a clearly separated file so upstream diffs stay readable.
