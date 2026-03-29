# Test Suite Hardening Plan

Reference: `test-suite-hardening.brainstorm.md`

## Goal

Remove smoke placeholders, tighten browser-suite execution rules, and document the correct standalone WASM test workflow.

## Scope

In scope:

- `PrompterLive.App.Tests` smoke cleanup
- `PrompterLive.App.UITests` execution policy
- root and local AGENTS updates

Out of scope:

- new app behavior
- native test projects

## Constraints And Risks

- Browser tests must stay runnable with plain `dotnet test`.
- UI suite depends on a fixed self-hosted origin.
- Do not reintroduce manual startup or environment-variable setup.

## Baseline

- [x] Review root and local `AGENTS.md`
- [x] Confirm solution contains only WASM-focused test projects under `tests/`
- [x] Run full `PrompterLive.App.Tests`
- [x] Run full `PrompterLive.App.UITests`

## Ordered Steps

- [x] Replace smoke placeholders with contract tests
  - Verification:
    - build `PrompterLive.App.Tests`
    - run full `PrompterLive.App.Tests`

- [x] Make browser-suite non-parallel policy explicit
  - Verification:
    - build `PrompterLive.App.UITests`
    - run focused `PrompterLive.App.UITests`

- [x] Update AGENTS with the correct browser-test workflow
  - Verification:
    - docs review

- [x] Run broader validation
  - Verification:
    - `dotnet build /Users/ksemenenko/Developer/PrompterLive/PrompterLive.slnx`
    - `dotnet test /Users/ksemenko/Developer/PrompterLive/tests/PrompterLive.App.Tests/PrompterLive.App.Tests.csproj --no-build`
    - `dotnet test /Users/ksemenko/Developer/PrompterLive/tests/PrompterLive.App.UITests/PrompterLive.App.UITests.csproj --no-build`
    - `dotnet format /Users/ksemenko/Developer/PrompterLive/PrompterLive.slnx`

## Done Criteria

- [x] no smoke-placeholder test file remains in `PrompterLive.App.Tests`
- [x] browser suite execution model is explicit in repo docs
- [x] focused and broader test passes are green
