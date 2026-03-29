# AGENTS.md

## Purpose

`PrompterLive.Web.E2E.Tests` currently holds deterministic host smoke tests for the WebAssembly app.

## Rules

- Use `WebApplicationFactory` for host and static-asset verification.
- Keep these tests serial-friendly when shared static web assets are involved.
- Browser-driven exploratory checks can be done with Playwright outside `dotnet test`, but stable CI tests here should remain fast and deterministic.

## Command

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Web.E2E.Tests/PrompterLive.Web.E2E.Tests.csproj`
