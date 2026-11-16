# YMH219 — Flappy Bird

A C# Windows Forms desktop game project for **Nesne Tabanlı Programlama (YMH219)**. The project demonstrates object-oriented design, event-driven programming, testing, and a reviewed Git workflow.

**Current stage: project initialization (00).** The application opens a fixed-size foundation window. Gameplay is planned and is not yet implemented. The Core library currently provides validated playfield dimensions with unit tests.

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
  FlappyBird.App/           Windows entry point and Forms/GameForm.cs
  FlappyBird.Core/          UI-independent domain code; Models/Playfield.cs
tests/
  FlappyBird.Core.Tests/    xUnit behavior tests
scripts/dev.ps1            Local setup diagnostics and development commands
docs/                      Architecture, decisions, workflow, QA, and requirements
.github/                   Windows CI and pull request template
.vscode/                   Shared build, test, and debug configuration
FlappyBird.slnx             Solution containing App, Core, and Tests
global.json                SDK selection
```

Rendering, physics, services, and assets folders will be added when they contain implemented features. The dependency direction is App → Core and Tests → Core. Core must not reference App or Windows Forms.

## Roadmap

| Stage | Deliverable | Branch | Status |
| --- | --- | --- | --- |
| 00 | Solution, shell, tests, CI, and documentation | `chore/initialize-project` | Implemented locally; review pending |
| 01 | Window rendering, background, ground, bird | `feat/game-window` | Planned |
| 02 | Gravity, flap, KeyDown/KeyUp input | `feat/bird-physics` | Planned |
| 03 | Moving and recycled pipe obstacles | `feat/pipe-system` | Planned |
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

There are no gameplay controls yet. Bird movement, pipe obstacles, collision, scoring, restart, persistence, audio, and difficulty changes remain future work. Unit tests do not establish that the full game is playable. No distribution package or third-party game assets are included. A license has not yet been selected; do not assume public-domain or open-source reuse rights.

## Technologies and NuGet packages

C#/.NET 10, Windows Forms, and an independent .NET Core domain library. Custom GDI+ game rendering and a WinForms timer are planned for the gameplay stages. Tests currently reference `Microsoft.NET.Test.Sdk` 17.14.1, `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, and `coverlet.collector` 6.0.4. App and Core have no third-party package dependencies.

## Screenshots and student details

Two real application screenshots must be embedded here before submission: active gameplay with score, and Game Over with final score/restart. They are pending because gameplay is not implemented. See the [submission checklist](docs/SUBMISSION.md).

Student full name and student number are awaiting the owner's input. These required README details must be filled before submission.

## Repository and submission

The owner's selected repository is [alaamadii/FlappyBird](https://github.com/alaamadii/FlappyBird). The instructor recommends the name `YMH219-FlappyBird`; the selected name is retained. The PDF was checked on November 16, 2026 and requires public access, meaningful commits, at least two screenshots, student details, and a second-computer check. This remote was private when inspected; public visibility remains a delivery requirement.

The owner submits their GitHub profile link in Classroom. The full [completion plan](docs/PLAN.md) tracks the remaining work and acceptance gates.
