# C# Domain Review

## Scope

- Domain and runtime logic in `PrompterOne.Core` and `PrompterOne.TpsSdk`.

## Tooling Used

- `dotnet build ./PrompterOne.slnx -warnaserror`
- Focused core test suite
- Targeted TPS runtime fuzzing on real compiler inputs

## Fixed Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | TPS compilation crashed when inline control words such as `[breath]` or `[edit_point]` split spoken words inside one phrase. | `src/PrompterOne.TpsSdk/Internal/TpsContentCompiler.TagHandling.cs`, `src/PrompterOne.TpsSdk/TpsRuntime.Compilation.cs`, `tests/PrompterOne.Core.Tests/Tps/TpsPhraseBoundaryTests.cs` | Fixed by flushing phrases before breath/edit-point control words and hardening phrase-word resolution. |
| High | `ScriptSessionService.SaveAsync` could leave derived state stale after saving a draft that had only raw fields staged. | `src/PrompterOne.Core/Workspace/Services/ScriptSessionService.cs`, `tests/PrompterOne.Core.Tests/Workspace/ScriptSessionServiceTests.cs` | Fixed by rebuilding the full derived draft state after persistence. |
| Medium | RSVP timing missed sentence-ending punctuation when the word ended in smart quotes. | `src/PrompterOne.Core/Rsvp/Services/RsvpPlaybackEngine.cs`, `tests/PrompterOne.Core.Tests/Rsvp/RsvpPlaybackEngineTests.cs` | Fixed by normalizing trailing smart and straight quotes before punctuation timing. |

## Open Findings

| Severity | Finding | Evidence | Status |
| --- | --- | --- | --- |
| High | `ScriptSessionService.UpdateDraftAsync` previously dropped last-known-good derived state on non-cancellation failures. The code now preserves it, but this path still lacks a stable real-input regression because the known compiler crash was fixed in the same batch. | `src/PrompterOne.Core/Workspace/Services/ScriptSessionService.cs` | Mitigated in code, partially open for stronger regression coverage once another stable failure fixture is isolated. |
| High | `ScriptPreviewService` still swallows runtime exceptions and reduces them to an empty preview result. | `src/PrompterOne.Core/Workspace/Preview/ScriptPreviewService.cs` | Open. Needs an explicit failure contract or caller-visible error surface. |

## Notes

- The TPS runtime crash was reproduced through the real compiler path with the body `hello [breath] hello` before the fix.
