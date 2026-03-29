# AGENTS.md

## Purpose

`PrompterLive.Maui.UITests` currently guards UI automation contracts needed for future Appium coverage.

## Rules

- Keep selector checks focused on stable `data-testid` contracts.
- Use these tests to prevent accidental removal of shared assets or automation hooks before real device automation is added.

## Command

- `dotnet test /Users/ksemenenko/Developer/PrompterLive/PrompterLive.Maui.UITests/PrompterLive.Maui.UITests.csproj`
