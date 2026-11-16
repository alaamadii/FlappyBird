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
