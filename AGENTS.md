# AGENTS.md

Project: `PrompterLive`
Stack: `.NET 10`, Blazor WebAssembly, MAUI Blazor Hybrid, Razor Class Library, xUnit, bUnit

## Purpose

This repo is the `PrompterLive` rebuild:

- `PrompterLive.Core` holds host-agnostic TPS, RSVP, preview, workspace, media-scene, and streaming contracts.
- `PrompterLive.Shared` holds the shared Blazor UI, browser interop layer, and shared DI composition.
- `PrompterLive.Web` is the primary acceptance runtime.
- `PrompterLive.Maui` is the Hybrid host scaffold for Android, iOS, Mac Catalyst, and Windows.

The old `Teleprompter` repo is an extraction source only. Do not reintroduce Uno-specific UI or platform code here.

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
