# AGENTS.md

## Purpose

`PrompterLive.Core.Tests` verifies the pure domain layer with deterministic xUnit tests.

## Scope

- parser and exporter round-trips
- workspace session behavior
- media scene state transitions
- streaming provider readiness and descriptors
- RSVP helpers

## Rules

- Prefer in-memory repositories and pure service construction.
- Avoid browser, MAUI, or filesystem-heavy tests unless the contract specifically requires it.
- When production behavior is tolerant or lossy, assert the stable contract instead of incidental internal shape.

## Command

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Core.Tests/PrompterLive.Core.Tests.csproj`
