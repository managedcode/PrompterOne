# Blazor UI Review

## Scope

- Routed Razor surfaces, Blazor-owned UI contracts, and page/component maintainability in `PrompterOne.Shared`.

## Fixed Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| Medium | Browser-suite artifacts were not uploaded from CI browser jobs, reducing diagnosability for UI failures. | `.github/workflows/pr-validation.yml`, `.github/workflows/deploy-github-pages.yml` | Fixed by always uploading `output/playwright/` artifacts from browser validation jobs. |

## Open Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | `EditorSourcePanel` still mixes Monaco, semantic overlay, and hidden-textarea behavior instead of a single Monaco-native authoring path. | `src/PrompterOne.Shared/Editor/Components/EditorSourcePanel.razor`, `src/PrompterOne.Shared/wwwroot/editor/editor-source-panel.js` | Open. Requires removal of legacy overlay ownership from the editor surface. |
| High | `index.html` eagerly loads many route-specific scripts and large runtime SDKs globally. | `src/PrompterOne.Web/wwwroot/index.html` | Open. Needs route-aware or lazy module loading. |
| High | `GoLivePage`, `TeleprompterPage`, and `MainLayout` remain well above repo file and type limits. | `src/PrompterOne.Shared/GoLive/Pages/*`, `src/PrompterOne.Shared/Teleprompter/Pages/*`, `src/PrompterOne.Shared/AppShell/Layout/MainLayout.razor.cs` | Open. Requires decomposition, not cosmetic cleanup. |
| Medium | The bootstrap shell in `index.html` still contains hardcoded English loading/error copy outside the localization path. | `src/PrompterOne.Web/wwwroot/index.html` | Open. Needs localized bootstrap ownership. |

## Notes

- This batch focused on correctness and release-boundary fixes first; the large UI decomposition items remain scheduled follow-up work.
