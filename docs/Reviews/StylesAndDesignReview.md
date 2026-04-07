# Styles And Design Review

## Scope

- Runtime CSS assets, design manifest ownership, and stylesheet maintainability under `src/PrompterOne.Shared/wwwroot/design` plus route-scoped `.razor.css`.

## Tooling Used

- Repo-local Stylelint bootstrap
- Large-file inventory review

## Fixed Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| Medium | The repo had no checked-in CSS lint entrypoint. | `package.json`, `stylelint.config.mjs` | Fixed by adding repo-owned Stylelint wiring for runtime stylesheets. |

## Open Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | `20-reference.css` in the settings design module is a 1200+ LOC runtime stylesheet hotspot. | `src/PrompterOne.Shared/wwwroot/design/modules/settings/20-reference.css` | Open. Needs decomposition into feature-owned style files. |
| Medium-High | Route and module CSS still carries a large backlog of formatting- and contract-level lint findings after the initial bootstrap. | `src/PrompterOne.Shared/wwwroot/design/**`, `src/PrompterOne.Shared/**/*.razor.css` | Open. Stylelint now exposes the backlog, but the cleanup is not complete in this batch. |
| Medium | Some legacy stylesheet modules look unused and should be confirmed or deleted. | `src/PrompterOne.Shared/wwwroot/design/modules/*` | Open. Needs import graph review before removal. |

## Notes

- The initial Stylelint pass now provides a durable inventory instead of silent CSS drift.
