# YMH219 — Flappy Bird

A C# Windows Forms desktop game project for **Nesne Tabanlı Programlama (YMH219)**. The project demonstrates object-oriented design, event-driven programming, testing, and a reviewed Git workflow.

**Current stage: pipes and movement (03), awaiting review.** Press Space to start bird flight and two moving pipe pairs with randomized gaps and automatic recycling. Gravity, ceiling clamping, focus pause, and fixed-step timing remain intact. PRs #1–#3 are merged; collision, scoring, and Game Over are future work.

## Quick start

Requirements: Windows 10/11, the .NET **10.0.402 SDK** (or a newer patch in the 10.0.4xx feature band), Git, and optionally VS Code with the recommended C# extensions. A runtime alone cannot build the project. NuGet access is required for the first restore. No Unity, database, API key, or additional .NET workload is required.

From the repository root in PowerShell:

```powershell
./scripts/dev.ps1 doctor
./scripts/dev.ps1 check
./scripts/dev.ps1 run
```

The launcher resolves `dotnet` from `PATH`, uses that installation as `DOTNET_ROOT`, and keeps CLI state in the ignored `.local/dotnet` folder. It restores inherited environment values when it exits. See [setup and troubleshooting](docs/SETUP.md).

Open the root folder in VS Code, install the recommended extensions, and use **Ctrl+Shift+B** to build or **F5** to debug. The application uses programmatically constructed controls rather than Designer files.

## Project layout

```text
src/
  FlappyBird.App/           Window, rendering, Input/FlapInput, Timing/FixedStepLoop
  FlappyBird.Core/          Domain models, GameEngine/PipeSystem, injectable gap generation
tests/
  FlappyBird.Core.Tests/    xUnit behavior tests
  FlappyBird.App.Tests/     Windows renderer, input-edge, and timing tests
scripts/dev.ps1            Local setup diagnostics and development commands
docs/                      Architecture, decisions, workflow, QA, and requirements
.github/                   Windows CI and pull request template
.vscode/                   Shared build, test, and debug configuration
FlappyBird.slnx             Solution containing App, Core, and Tests
global.json                SDK selection
```

App and Core.Tests reference Core; App.Tests references App for rendering/input/timing checks. Core must not reference App or Windows Forms. Additional folders will be added when they contain implemented features.

## Roadmap

| Stage | Deliverable | Branch | Status |
| --- | --- | --- | --- |
| 00 | Solution, shell, tests, CI, and documentation | `chore/initialize-project` | Merged in PR #1 |
| 01 | Window rendering, background, ground, bird | `feat/game-window` | Merged in PR #2 |
| 02 | Gravity, flap, KeyDown/KeyUp input | `feat/bird-physics` | Merged in PR #3 |
| 03 | Moving and recycled pipe obstacles | `feat/pipe-system` | Implemented; owner review pending |
| 04 | Collision, score, Game Over, restart | `feat/game-rules` | Planned |
| 05 | UI polish and optional enhancements | `feat/game-polish` | Planned |
| 06 | Final QA, screenshots, submission review | `docs/final-submission` | Planned |

Every stage uses a separate branch and pull request. Review the diff, test results, and documentation before merging. The owner decides when to merge. The overall brief targets five separate assignments by **November 30, 2026, 23:59 Asia/Riyadh**; this repository covers Flappy Bird only.

## Documentation

- [Completion plan](docs/PLAN.md): stage-by-stage tasks, acceptance criteria, target dates, and review gates for finishing Flappy Bird.
- [Setup and troubleshooting](docs/SETUP.md): tools, environment, VS Code, common failures.
- [Architecture](docs/ARCHITECTURE.md): current responsibilities and intended game design.
- [Decisions](docs/DECISIONS.md): choices, rationale, and tradeoffs.
- [Development workflow and log](docs/DEVELOPMENT.md): stages, branching, review, actual changes.
- [Testing](docs/TESTING.md): commands, assertions, UI checks, and evidence.
- [Requirements](docs/REQUIREMENTS.md): acceptance criteria and implementation status.
- [Submission checklist](docs/SUBMISSION.md): final verification and oral explanation preparation.
- [Agent working agreement](AGENTS.md): rules for automated project work.

## Validation and limitations

Use `./scripts/dev.ps1 check` to restore, build Release, run tests, and verify formatting. The Windows GitHub Actions workflow runs the same command on pull requests and pushes to `main`, then uploads TRX results. Workflow configuration does not imply that a hosted CI run has passed; execution evidence is recorded in [TESTING.md](docs/TESTING.md).

This is pipe practice, not yet a complete game. Ground/pipe collision, scoring, Game Over, restart, persistence, audio, and difficulty changes remain future work. The bird can pass through pipes or fall behind the ground/off-screen while simulation continues; no loss or automatic reset occurs. Close and relaunch to return to Ready. No distribution package or third-party game assets are included. A license has not yet been selected; do not assume public-domain or open-source reuse rights.

## Technologies and NuGet packages

C#/.NET 10, Windows Forms, original GDI+ drawing, a WinForms timer with Stopwatch timing, and an independent .NET domain library. Both test projects reference `Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, and `xunit.runner.visualstudio` 3.1.4; Core.Tests also references `coverlet.collector` 6.0.4. App and Core have no third-party package dependencies.

## Controls and flight

| Action | Behavior |
| --- | --- |
| Space while Ready | Starts Running and flaps upward |
| Release, then press Space while Running | Flaps once; holding Space does not repeat |
| Switch away or minimize | Freezes motion and clears input/timing state |
| Return | Resumes without inactive-time catch-up; release Space first if held on return |
| Close and relaunch | Returns to Ready; no in-game restart yet |

Gravity is 900 logical units/s² and flap velocity is −320 units/s. Motion uses fixed 1/60-second updates; each timer tick accepts at most 100 ms and performs at most six updates. Excess stall time is discarded. At the ceiling, Y is clamped and upward velocity cleared. See [architecture](docs/ARCHITECTURE.md) for the formula and policies.

## Pipe behavior

Two pairs (four pipe shapes) remain still while Ready and move left at 140 logical units/s while Running. Their initial left edges are X=520 and X=780, giving about 1.69 seconds before the first pair reaches the bird's initial right edge. Both pairs can be fully visible together.

Each pair is 64 units wide with a 180-unit vertical opening. Spacing is **260 units between left edges**, leaving 196 units of empty horizontal space. Gap tops are sampled in **[40, 340)**: minimum included, maximum excluded. Upper/lower rectangles end at the gap edges and shared ground boundary. Generation uses an instance-owned random source that tests can replace.

A pair recycles only when its right edge is strictly below X=0. It moves after the other pair at the same spacing and receives a fresh gap sample; the same two objects are reused. Focus pause freezes the whole engine. Domain reset primitives also restore pipes, but no restart control exists yet.

Settings are finite/positive and validated against Playfield when creating the engine/pipe system. Core obstacle updates support up to one second, with speed limited to two spacings per second, so at most two pairs recycle in one call. The UI still uses only 1/60-second steps.

## Scene behavior

The renderer uses the 480 × 640 logical playfield with ground beginning at Y=560. Shapes, strokes, and pixel-unit fonts scale together to the actual client area. Uniform scaling and centered borders preserve proportions when the client aspect ratio differs. WinForms DPI autoscaling uses a 96-DPI baseline; the window remains fixed-size with maximize disabled.

The renderer reads Bird, Ready/Running state, and the existing PipePair geometry. It paints pipes before the bird, keeps their lips within the model bounds, and places a compact instruction banner over the Running scene. Ready instructions disappear when flight starts. Painting never changes positions or consumes randomness. Ground remains decorative. The Form exposes accessible control descriptions.

## Screenshots and student details

Two real screenshots must be embedded before submission: gameplay with score, and Game Over with final score/restart. They remain pending until those rules are implemented. See the [submission checklist](docs/SUBMISSION.md).

Student full name and student number are awaiting the owner's input. These required README details must be filled before submission.

## Repository and submission

The owner's selected repository is [alaamadii/FlappyBird](https://github.com/alaamadii/FlappyBird). The instructor recommends the name `YMH219-FlappyBird`; the selected name is retained. The PDF was checked on November 16, 2026 and requires public access, meaningful commits, at least two screenshots, student details, and a second-computer check. This remote was private when inspected; public visibility remains a delivery requirement.

The owner submits their GitHub profile link in Classroom. The full [completion plan](docs/PLAN.md) tracks the remaining work and acceptance gates.
