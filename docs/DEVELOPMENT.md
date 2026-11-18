# Development workflow and log

## Working method

1. Check `git status --short --branch` and preserve existing changes.
2. Start the appropriate stage branch from the reviewed base. Never implement directly on `main`.
3. Make focused changes and add meaningful tests for the behavior introduced.
4. Update architecture/decisions when needed, this log for the actual work, and testing evidence for checks performed.
5. Run `./scripts/dev.ps1 check`, inspect the complete diff, and perform relevant Windows UI checks.
6. Make meaningful commits when authorized. Open a PR describing behavior, validation, and known limitations when remote work is authorized.
7. Review the PR and hosted checks before the owner merges it. Do not start the next stage before review of the previous stage.

The repository was initially found on `chore/initialize-project` with no commits and with untracked project scaffolding. The original origin pointed to `alaamadii/YMH219-FlappyBird`. On 2026-11-16 the owner explicitly selected `https://github.com/alaamadii/FlappyBird.git` and authorized pushing the changes. The selected remote was verified as empty and private with owner push access. See the publication record below and [PLAN.md](PLAN.md) for the current workflow.

## Stage plan

| Stage | Branch | Exit criteria |
| --- | --- | --- |
| 00 | `chore/initialize-project` | App/Core/Tests build, meaningful tests pass, shell opens, docs/CI ready, PR reviewed |
| 01 | `feat/game-window` | Background, ground, and bird draw cleanly; check DPI and repaint behavior |
| 02 | `feat/bird-physics` | Gravity and flap behavior tested; KeyDown/KeyUp behavior manually verified |
| 03 | `feat/pipe-system` | Obstacles move and recycle; gap and spacing boundaries tested |
| 04 | `feat/game-rules` | Collision, one-time scoring, Game Over, restart tested and playable |
| 05 | `feat/game-polish` | Required gameplay stable; selected extras tested and documented |
| 06 | `docs/final-submission` | Acceptance checklist complete, screenshots captured, clean checkout verified |

## 2026-11-16 — Stage 00 foundation

### Findings

- Existing solution already referenced App, Core, and Tests correctly.
- SDK 10.0.402 and Windows Desktop runtime 10.0.13 were installed; initial Release build passed with zero warnings/errors.
- The original suite reported one passing test, but its body had no assertions. This was not meaningful evidence of behavior.
- Template `Class1`, generic Designer-based `Form1`, absent documentation/CI, and inherited tool-directory environment overrides required attention.

### Implemented

- Added SDK pinning, shared warning/determinism configuration, editor rules, and generated-file exclusions.
- Normalized source formatting and specified Git line endings for consistent future checkouts.
- Replaced the template form with a programmatic `GameForm` shell and clear initialization status.
- Replaced empty Core/test placeholders with immutable validated `Playfield` dimensions and behavior tests.
- Added a process-scoped .NET environment launcher and VS Code build/test/debug configuration.
- Added Windows GitHub Actions verification and a PR template.
- Added README, architecture, decisions, setup, testing, requirements, submission checklist, and the agent working agreement.

### Review boundary

The complete local check passed: Release build with zero warnings/errors, ten passing test cases, and clean formatting. The hidden Release application created its titled window and closed normally with exit code 0. The environment launcher restored inherited settings, and generated-file exclusions were verified.

This is initialization only. Gameplay remains planned. Detailed local validation is recorded in [TESTING.md](TESTING.md). GitHub CI, visual/DPI review, VS Code debugging, and PR review remain separate review steps; no remote success is implied. No commit, push, PR creation, or merge was performed as part of this local setup/documentation task.

## 2026-11-16 — Instructor-aligned plan and publication preparation

- Read all five pages of the instructor PDF, focusing the plan on Flappy Bird requirements (page 2) and shared submission/assessment rules (pages 1 and 5).
- Added PLAN with staged implementation scope, behavior contracts, tests, review gates, target dates, risks, and final acceptance criteria.
- Corrected the requirements matrix to reflect at least two obstacles, with two pipe pairs chosen for our implementation.
- Added missing submission requirements: public visibility, two screenshots embedded in README, full name/student number, package listing, and another-computer validation.
- Retained the owner's requested repository name `FlappyBird`, recording the instructor's recommended name separately.
- Prepared publication to the empty requested remote using an empty main baseline and an initialization review branch. Remote publication/CI results will be recorded after execution; no gameplay or merge is part of this task.

### Publication record

- Updated `origin` to `https://github.com/alaamadii/FlappyBird.git` as requested.
- Published empty bootstrap commit `997e5f4` to `main` as the review base; application development stays on the stage branch.
- Published initialization/plan commit `dcbd97e` to `chore/initialize-project`, including all source/configuration/docs and no generated output.
- Opened [PR #1](https://github.com/alaamadii/FlappyBird/pull/1) against `main`; its merge was subsequently confirmed on 2026-11-16 before Stage 01 began.
- Re-ran `./scripts/dev.ps1 check`: restore/build/test/format passed, with zero warnings/errors and ten passing tests. Documentation links and the staged whitespace check also passed.
- The first [hosted CI run](https://github.com/alaamadii/FlappyBird/actions/runs/43817906408) was queued when inspected. Queue status is not a passing result. Subsequent documentation commits trigger their own checks; inspect the current PR head before merging.
- Remote visibility remains private. The owner must authorize changing visibility before the public-access submission requirement can be marked complete.

## 2026-11-16 — Stage 01 game window and rendering

### Preparation and implementation

- Confirmed PR #1 merged at commit `0c4f66e`; working tree was clean. Switched to main, pulled with `--ff-only`, and created `feat/game-window`.
- Replaced the initialization Label with an original static GDI+ scene: gradient sky, clouds/hills, yellow bird preview, shared-boundary ground, heading, and Ready panel explaining Stage 02 controls.
- Added App-owned GameRenderer and delegated GameForm.OnPaint to it. Form and renderer share one immutable Playfield; Core remains unchanged and independent of Windows drawing.
- Added uniform client-area scaling, centered borders, pixel-unit text, explicit 96-DPI Form baseline, repaint-on-resize, and accessible Ready text. Kept fixed borders and disabled maximize.
- Used scoped disposal for drawing resources and saved/restored the caller's graphics state. Added no timer, input behavior, gameplay models, pipes, score, or restart.
- Added nine Windows-only renderer regression cases without altering the ten Core cases. Added App.Tests to the solution so existing local/CI commands include it.

### Validation and review boundary

- `./scripts/dev.ps1 check` passed: zero warnings/errors, 19 tests passing, clean formatting.
- Visually inspected off-screen previews at 100%/125%/150% equivalent sizes and a different aspect ratio. Increased heading contrast and used a union cloud path after inspecting the initial preview.
- An invisible actual Form at desktop DPI 120 had a 600×800 client area and identical image output after 30 repaint cycles and minimize/restore. The Release executable created its hidden titled window and exited normally with code 0.
- Live cover/uncover, flicker perception, actual desktop DPI changes/cross-monitor transitions, and VS Code debugger checks remain owner checks. See TESTING for evidence and limits.
- Updated README, architecture, decisions, plan progress, and testing documentation. Stage 01 owner acceptance remains pending; Stage 02 has not begun.
- Pushed implementation `bbc3e55` and documentation `ae0d535` on `feat/game-window`, then opened [PR #2](https://github.com/alaamadii/FlappyBird/pull/2) against main. [Windows CI run 43819540780](https://github.com/alaamadii/FlappyBird/actions/runs/43819540780) passed for ae0d535, including test artifact upload. This publication record is a documentation follow-up; inspect the PR's current checks before owner merge.

## 2026-11-16 — Stage 02 bird physics, controls, and loop

- Confirmed PR #2 merged at `544d50b`; working tree was clean. Fast-forwarded main and created `feat/bird-physics`.
- Added immutable GameSettings, encapsulated Bird motion/reset, Ready/Running state, and the currently needed GameEngine coordination in Core. No Windows APIs or timers entered Core.
- Added App press-edge input, activation-time held-key sampling, inactive pause, monotonic elapsed timing, fixed 1/60-second updates, 100 ms admission/six-update cap, and Form timer disposal.
- Renderer now reads the real Bird and state, hides Ready instructions when Running, and retains Stage 01 scaling/resource guarantees. Ground remains decorative; the bird can leave the screen without ending the run. No Stage 03/04 features were introduced.
- `./scripts/dev.ps1 check` passed with zero warnings/errors and 56 passing cases (34 Core, 22 App). Test coverage includes numeric physics, invalid inputs, ceiling, domain reset, press edges, bounded timing, and current-state rendering.
- Invisible window-message/lifecycle probe passed tap/hold/release, actual timer pumping, inactive freeze, minimize/restore, simulated held-key reactivation, bounded 500 ms stall, and disposal. Actual hidden Release executable opened its main window and closed with exit code 0.
- Inspected Ready/Running previews at current 125% DPI. Physical-key/focus behavior, perceived smoothness, and other unperformed desktop checks remain explicitly listed in TESTING.
- Updated README, architecture, decisions, requirements, plan, and testing evidence. Owner review and hosted validation remain separate gates; Stage 03 has not begun.
- Published implementation `78f0362` and documentation `22bf05c` to `feat/bird-physics`, then opened [PR #3](https://github.com/alaamadii/FlappyBird/pull/3). [Windows CI run 43828515337](https://github.com/alaamadii/FlappyBird/actions/runs/43828515337) passed for 22bf05c, including artifact upload. This evidence update is a documentation follow-up; current-head checks are tracked on the PR. No merge was performed.

## 2026-11-16 — Stage 03 pipes and movement

- Confirmed PR #3 merged at `bd464be`; working tree was clean. Fast-forwarded main and created `feat/pipe-system`.
- Added numeric PipePair/PipeRectangle geometry, a two-pair PipeSystem with a read-only collection, injectable IGapSource, and instance-owned RandomGapSource. Core stays independent of Windows/drawing APIs.
- Extended settings with speed, dimensions, explicit left-edge spacing, initial offset, gap margins, and bird clearance. Scalar and playfield validation reject impossible gaps/spacing, unrepresentable starts, or travel beyond the supported bounded recycling workload.
- Engine now advances bird and pipes together only while Running. Right < 0 triggers ordered recycling and fresh gap sampling; the same objects are reused. Reset primitives refresh gaps and restore starting positions. There is no scoring flag or Stage 04 behavior.
- Renderer draws original green pipe bodies/lips from model rectangles, before the bird, with ground clipping and a compact non-collision instruction banner. It does not generate gaps or change domain state.
- `./scripts/dev.ps1 check` passed with zero warnings/errors and 91 cases (Core 66, App 25), including exact boundaries, largest supported update, invalid sources/settings, update partitioning, non-mutating rendering, and 60,000-step stability.
- Invisible Form probe confirmed timer-driven pipe motion, bird/pipe focus freeze, capped stall, two visible pairs, and 32 recycle events over 60 directly simulated seconds with periodic repaint. Hidden executable launch/normal shutdown passed with exit code 0.
- Inspected pipe previews at 100%/125%/150% equivalent sizes and the actual Form's 125% DPI capture. Real-time human play/visual/input checks remain listed in TESTING.
- Updated README, architecture, decisions, requirements, testing, and plan progress. Stage 03 owner acceptance remains pending; Stage 04 has not begun.

## 2026-11-17 — Stage 03 boundary review

- Added a fractional-spacing/full-cycle regression after identifying a potential extra recycle caused by floating-point undershoot at the exact left boundary.
- Clamped only recycled positions to their mathematical `-Width` lower bound, keeping strict off-screen detection unchanged.
- Re-ran `./scripts/dev.ps1 check`: zero warnings/errors, 67 Core and 25 App tests passing (92 total), formatting clean. Updated current counts and documented the rounding policy separately from the 2026-11-16 visual/lifecycle checks.
- Published implementation `d8ecfee` and documentation `e847e0f` to `feat/pipe-system`, then opened [PR #4](https://github.com/alaamadii/FlappyBird/pull/4). [Windows CI run 44027176448](https://github.com/alaamadii/FlappyBird/actions/runs/44027176448) passed for e847e0f, including artifact upload. This documentation follow-up records that evidence; current-head checks remain visible on the PR. No merge or Stage 04 implementation was performed.

## 2026-11-17 — Stage 04 collision, score, Game Over, restart

- Confirmed PR #4 merged at `f458b10`; clean working tree. Fast-forwarded main and created `feat/game-rules`.
- Added inclusive swept AABB contact, up to 60 bounded rule steps per supported one-second engine update, earliest-contact stopping, and ground/ceiling policies. Existing ground overlap resolves at zero time; unsupported elapsed values are rejected before mutation.
- Added controlled score, per-pair eligibility, contact-before-score-before-recycle ordering, GameOver freeze/ignored flap, and clean Core reset.
- Added live/final score presentation, dimmed Game Over card, one native New game button, Enter reset, focused-button Space suppression, held-Space protection, complete clock/accumulator reset, and state-aware accessible copy.
- Extracted SceneViewport for shared paint/native-control positioning; kept DPI scaling, original shapes, and scoped graphics resource ownership.
- `./scripts/dev.ps1 check` passed with zero warnings/errors, 92 Core + 33 App cases (125 total), and clean formatting. Coverage includes contact boundaries/sweeps, score ordering, 40 Core reset cycles and twelve STA native-button/Enter restart cycles.
- Invisible actual-Form probe passed a controlled scored pass, pipe/ground loss, final score, BM_CLICK/Enter reset, held keys, focus/minimize/restore, bounded stall, and close. The actual hidden Release executable launched/closed with exit code 0.
- Reviewed Ready/Running/GameOver diagnostic previews at 125% DPI and equivalent Running scale images. Human random-gap playtesting and other unperformed checks remain explicit in TESTING.
- Updated README, architecture, decisions, requirements, testing, and plan. Stage 04 owner acceptance remains pending; no Stage 05 extras were implemented.
- First hosted run passed build/Core but failed the repeated-paint bitmap comparison with concurrent native UI tests. Isolated the native Form collection from other test collections while keeping the exact comparison assertion; tracked the failed run and subsequent verification separately in TESTING.
- Published implementation `d419cd2`, documentation `f66ff32`, and test isolation fix `f0a9300` to `feat/game-rules` in [PR #5](https://github.com/alaamadii/FlappyBird/pull/5). Full local validation and [Windows run 44043723178](https://github.com/alaamadii/FlappyBird/actions/runs/44043723178) passed for f0a9300, including all 125 tests, formatting, and artifact upload. This evidence update is documentation-only; final-head checks remain tracked on the PR. No merge or Stage 05 work was performed.

## 2026-11-17 — Stage 05 usability and persistent best

- Confirmed PR #5 merged at `3ea9a69`; clean working tree. Switched to main, pulled with `--ff-only`, and created `feat/game-polish`.
- Added App-owned IHighScoreStore, FileHighScoreStore, and HighScoreService. Production loads a bounded nonnegative integer from LocalApplicationData; new completed bests use flushed same-directory temporary replacement. Core remains unchanged.
- Kept best separate from the current run, retained it across resets, and gated persistence on the presentation transition into GameOver. Missing files are normal; invalid/inaccessible data and failed writes produce cached nonmodal feedback. Failed writes preserve session best and retry at the next completed run.
- Polished Ready/Running/GameOver copy, labeled Score/Best cards, ground controls, inactive pause guidance, accessible descriptions, and native restart hover/press/focus presentation. Reused a disposed/replaced pixel-unit button Font with the same viewport scale.
- Added a resize regression that exposed stale native bounds when fixed Form layout preceded final ClientSize. Remapped after ClientSizeChanged as well as layout, retaining fixed borders and shared logical coordinates.
- Added file/service/native/renderer regression tests; existing Form tests now inject memory storage. Updated all old local probes with isolated paths and added an ignored polish probe for save/reload/failure and scaled preview evidence. Owner score data is untouched.
- Local test run passed 156 cases (92 Core, 64 App). Scripted native probes passed loss/save/restart/reload, locked-file resilience, existing gameplay/input/timing checks, and actual Release executable launch/normal close (exit 0). Detailed full-check/hosted results are recorded below and in TESTING.
- The complete `./scripts/dev.ps1 check` passed: restore, Release build with zero warnings/errors, all 156 tests, and clean formatting verification.
- Inspected scaled native captures for Ready/Running/Paused/GameOver, saved best, warning, and focused restart. Actual physical-input/display checks and human difficulty review remain pending. No tuning changed without human evidence; audio, progressive speed, and difficulty selection are deferred.
- Updated README, ARCHITECTURE, DECISIONS, TESTING, REQUIREMENTS, PLAN, and dedicated HIGH_SCORE documentation. Stage 05 awaits owner review; Stage 06 has not started. No merge is authorized.
- Published implementation `f5fd8a1` and documentation `88626d3` to `feat/game-polish`, then opened [PR #6](https://github.com/alaamadii/FlappyBird/pull/6) against main. [Windows run 44051252291](https://github.com/alaamadii/FlappyBird/actions/runs/44051252291) passed for 88626d3 with all 156 tests, formatting and artifact upload. This evidence follow-up is documentation-only; final-head checks remain tracked on the PR. No merge or Stage 06 work was performed.
- Final storage review found that directly saving zero over corrupt data could return without repairing the file. Restricted the existing-file early return to successful validation, added a reload regression, and reran the full check: zero warnings/errors, 92 Core + 65 App = 157 cases passing, formatting clean. Production new-best policy and all gameplay/UI behavior remain unchanged; final-head CI is tracked in PR #6.

## 2026-11-17 — Stage 06 final QA and submission preparation

- Owner requested final completion, explicitly approved merging PR #6 and public visibility. Rechecked the exact head 5043189 and its passing Windows CI, then merged it at 97416ab. Updated main with a fast-forward pull and brought `docs/final-submission` onto that reviewed base, preserving all work.
- Made alaamadii/FlappyBird public using GitHub CLI (the connector has no visibility-edit tool). Verified anonymous repository metadata/page access and its public profile listing. Reviewed tracked filenames/history and performed a limited credential-pattern scan with no matches.
- Cloned the remote feature head into a fresh ignored folder, ran the complete check (157 cases, zero warnings/errors, clean formatting), and verified that clone's actual Release EXE creates its titled main window and closes normally with exit 0. This does not replace testing on a second computer.
- Re-ran isolated mandatory-rule and persistence probes. Captured/inspected real target-window Running and Game Over images at 125% DPI during a controlled-gap scripted pass followed by natural ground loss. Saved only the actual application window; owner score data and unrelated desktop content are excluded.
- Embedded both images in README and documented provenance/checksums. Reconciled architecture, requirements, decisions, plan and submission checklist; added a source-linked code walkthrough for the oral explanation.
- Application source, gameplay settings, dependencies and feature scope remain unchanged. Identity details, owner physical/second-computer checks, final PR acceptance and Classroom submission are explicitly pending; no result or identity value was invented.
- Final branch `./scripts/dev.ps1 check` passed: restore, zero-warning/error Release build, 157 tests and formatting. Markdown targets across 13 files and whitespace checks passed. Publication/CI will be recorded separately from these local results.
- Published screenshots/code guide in 4dc4d9f and final checklist/evidence in 98bd541 to `docs/final-submission`, then opened [draft PR #7](https://github.com/alaamadii/FlappyBird/pull/7) against main. It remains draft/unmerged while owner identity and manual sign-off are pending. This publication-link follow-up is documentation-only; exact final-head hosted checks are tracked on the PR.

## 2026-11-18 — Stage 06 readiness follow-up

- Inspected the clean working tree, requested documents and PR #7. The PR was already merged on 2026-11-17 at de91ef8, and both its ccf5b35 head CI and post-merge main CI passed. No merge was performed in this follow-up.
- Fetched origin, fast-forwarded main and created `docs/stage06-readiness`. A separate follow-up PR is necessary because PR #7 is already closed/merged; no force-push or history rewrite is used.
- Corrected current README, PLAN and SUBMISSION status to distinguish the merged technical evidence from still-incomplete owner submission acceptance. Historical execution records remain unchanged, with this dated follow-up explaining the new status.
- The submitted name/student-number/manual-evidence fields were still placeholders. Requested the actual public identity details and test evidence; retained existing pending fields and checkboxes rather than inventing values or results.
- Ran the complete check: restore, zero-warning/error Release build, 157 passing cases (92 Core, 65 App) and clean formatting. Reverified Markdown targets across 13 files, unchanged screenshot hashes/embeds, ignored-output exclusions and anonymous repository/image access.
- Application code, dependencies, settings and images remain unchanged. Screenshot owner acceptance, physical/manual/second-computer checks, identity and Classroom submission remain pending. The follow-up stays draft until required owner information is complete; no automatic merge or Classroom action is authorized.
- Published readiness correction 17c32b0 on `docs/stage06-readiness` and opened [draft PR #8](https://github.com/alaamadii/FlappyBird/pull/8) against main. Final exact-head checks and outstanding owner inputs are recorded on that PR; PR #7 remains merged and is not reopened.
- Owner clarified that the student number should not be published in README. Removed its placeholder/publication request, updated current plan/checklist expectations, and recorded ADR-018 plus the original instructor requirement as a separate owner-directed exception. Full name and manual/screenshot acceptance remain pending; no actual student number was collected or published.
