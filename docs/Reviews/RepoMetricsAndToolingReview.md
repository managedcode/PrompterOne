# Repo Metrics And Tooling Review

## Scope

- Repo footprint, analysis-tool availability, and local audit tooling coverage.

## Metrics Snapshot

| Metric | Value |
| --- | --- |
| Total code files reviewed (vendor excluded for hotspot scan) | 863 |
| Total code lines | 104,693 |
| C# LOC | 53,232 |
| CSS LOC | 10,929 |
| Razor LOC | 7,589 |
| JavaScript LOC | 6,719 |

## Tooling Status

| Tool / Signal | Status | Notes |
| --- | --- | --- |
| `dotnet build -warnaserror` | Available | Repo analyzers are active through shared props. |
| `cloc` | Available | Used for repo footprint and hotspot review. |
| ESLint | Added in this batch | Repo-local `package.json` and `eslint.config.mjs` now exist. |
| Stylelint | Added in this batch | Repo-local `stylelint.config.mjs` now exists; CSS backlog remains. |
| `quickdup` | Not available | User-local bootstrap is still needed. |
| Complexity analyzers (`CA1501/2/5/6`) | Not configured | No checked-in `CodeMetricsConfig.txt` or explicit CA150x policy yet. |

## Current Hotspots

| Area | Evidence | Status |
| --- | --- | --- |
| Large JS module | `src/PrompterOne.Shared/wwwroot/editor/editor-monaco.js` | Open |
| Large CSS module | `src/PrompterOne.Shared/wwwroot/design/modules/settings/20-reference.css` | Open |
| Large test support file | `tests/PrompterOne.Web.UITests/Support/BrowserTestConstants.cs` | Open |
| Large UI contract file | `src/PrompterOne.Shared/Contracts/UiTestIds.cs` | Open |

## Notes

- The repo now has durable frontend lint entrypoints, but not every surfaced issue was remediated in this batch.
