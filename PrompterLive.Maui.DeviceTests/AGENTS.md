# AGENTS.md

## Purpose

`PrompterLive.Maui.DeviceTests` validates the MAUI host scaffold through deterministic project and wiring checks.

## Rules

- Keep these tests environment-light.
- Prefer project-file and host-registration contracts until full device runners are introduced.

## Command

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui.DeviceTests/PrompterLive.Maui.DeviceTests.csproj`
