# JavaScript Interop Review

## Scope

- Browser interop modules, vendored runtime boundaries, and repo-owned JavaScript contracts under `src/PrompterOne.Shared/wwwroot`.

## Tooling Used

- Repo-local ESLint bootstrap
- Direct module inspection

## Fixed Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | Runtime telemetry attempted to load Google Analytics and Microsoft Clarity from remote URLs at runtime, violating the pinned-vendor rule. | `src/PrompterOne.Shared/wwwroot/app/runtime-telemetry.js` | Fixed by blocking remote vendor loads and keeping only the local stub/instrumentation path. |
| Medium | Dead JavaScript helpers remained in repo-owned modules. | `src/PrompterOne.Shared/wwwroot/editor/editor-monaco.js`, `src/PrompterOne.Shared/wwwroot/media/browser-media.js` | Fixed by removing confirmed unused functions during ESLint bootstrap. |

## Open Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | `editor-monaco.js` and `editor-source-panel.js` are still oversized UI/workflow owners instead of thin browser bridges. | `src/PrompterOne.Shared/wwwroot/editor/editor-monaco.js`, `src/PrompterOne.Shared/wwwroot/editor/editor-source-panel.js` | Open. Needs C#/Blazor ownership pulled back over DOM contracts and authoring state. |
| Medium | Browser file-save support is duplicated across app-specific JS modules. | `src/PrompterOne.Shared/wwwroot/app/file-picker.js`, `src/PrompterOne.Shared/wwwroot/media/go-live-output-support.js` | Open. Extract one browser-file bridge. |

## Notes

- Repo-local `package.json`, `eslint.config.mjs`, and `stylelint.config.mjs` were added so frontend assets can be linted intentionally instead of ad hoc.
