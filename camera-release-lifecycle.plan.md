# Camera Release Lifecycle Plan

## Goal

Fix browser camera lifecycle leaks so PrompterOne releases local camera captures when the user leaves camera-owning UI surfaces, with explicit regression coverage for Settings, Teleprompter, and Go Live.

## Scope

In scope:
- `browser-media.js` camera track release behavior
- browser media synthetic harness lifecycle assertions
- Studio media acceptance tests for Settings, Teleprompter, and Go Live cleanup
- Go Live route-owned camera surface cleanup

Out of scope:
- unrelated Settings/About feedback work
- unrelated Editor/browser-suite failures
- parallel import, telemetry, or shell work outside camera lifecycle ownership

## Constraints And Risks

- Only touch camera lifecycle code and its tests.
- Do not remediate unrelated compile or test failures outside owned files.
- Go Live renders multiple camera surfaces at once, including source-rail thumbnails, so route exit must clean up every attached preview surface instead of only program/preview cards.
- Current worktree contains unrelated incomplete Settings changes that can block repo-wide verification.

## Ordered Plan

- [x] Reproduce the camera leak in owned browser-media surfaces and identify current attach/detach paths in Settings, Teleprompter, and Go Live.
  Verification:
  Read the owning Blazor components and browser-media runtime to confirm how captures are acquired and released.

- [x] Harden shared browser media release semantics so local tracks stop underlying `mediaStreamTrack` instances during cleanup.
  Verification:
  Re-run owned Studio media acceptance coverage for Settings and Teleprompter and confirm synthetic active tracks drain to zero after disable/navigation.

- [x] Extend the synthetic media harness with active-track accounting that can detect leaked camera tracks by kind or device.
  Verification:
  Use browser-side assertions in Studio tests instead of DOM-only checks.

- [x] Add Settings and Teleprompter regression coverage for releasing synthetic camera tracks.
  Verification:
  `dotnet test --project ./tests/PrompterOne.Web.UITests.Studio/PrompterOne.Web.UITests.Studio.csproj --maximum-parallel-tests 1`

- [x] Add Go Live route-exit cleanup ownership and a Go Live regression test that asserts all synthetic video tracks are released after leaving an idle Go Live session.
  Verification:
  Run the Studio acceptance suite or the narrowest possible Go Live media coverage and confirm the new route-exit assertion passes.

- [x] Establish a solution test baseline before the later user-owned worktree changes interfered.
  Verification:
  `dotnet test --solution ./PrompterOne.slnx --max-parallel-test-modules 1`
  Result:
  Passed earlier in this task before unrelated Settings/About work changed the worktree.

- [ ] Re-run final owned verification on the current worktree.
  Verification:
  `dotnet test --project ./tests/PrompterOne.Web.UITests.Studio/PrompterOne.Web.UITests.Studio.csproj --maximum-parallel-tests 1`
  Blocker:
  Current build is blocked by unrelated `SettingsAboutSection.razor` references to missing `UiTestIds.Settings.AboutFeedbackCard` and `UiTestIds.Settings.AboutFeedbackOpen`.

- [ ] Stage only owned camera lifecycle files and create a local commit.
  Verification:
  Inspect `git diff --staged` to ensure no unrelated files are included.

## Failing Or Blocked Verification Inventory

- [ ] `dotnet test --project ./tests/PrompterOne.Web.UITests.Studio/PrompterOne.Web.UITests.Studio.csproj --maximum-parallel-tests 1`
  Symptom:
  Build stops in `src/PrompterOne.Shared/Settings/Components/SettingsAboutSection.razor` because `UiTestIds.Settings.AboutFeedbackCard` and `UiTestIds.Settings.AboutFeedbackOpen` are missing.
  Root-cause note:
  This is outside the camera lifecycle diff and came from unrelated Settings work already present in the current worktree.
  Intended fix path:
  Do not fix inside this task; wait for the owning Settings change to be completed or removed, then rerun the Studio suite.
