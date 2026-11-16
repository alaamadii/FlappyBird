# Testing and validation

## Reproducible checks

From PowerShell at the repository root:

```powershell
./scripts/dev.ps1 check
```

This restores packages, builds all projects in Release, runs xUnit tests without rebuilding, writes TRX results to ignored `TestResults/`, and verifies formatting without changing files. A failing command stops the script. `./scripts/dev.ps1 test` builds and runs tests on its own; `./scripts/dev.ps1 build` builds without running tests.

CI uses Windows and the same `check` command. It uploads available TRX files even when a step fails. Hosted workflow status must be checked on GitHub for the relevant commit; adding workflow YAML does not prove it has run.

## Automated coverage

`PlayfieldTests` checks:

- Default dimensions and the ground boundary.
- Custom dimensions and the corresponding boundary.
- Zero and negative width, height, and ground height.
- Ground height equal to or greater than total height.
- The parameter name reported by rejected input.

The suite contains two facts and eight theory cases, totaling ten test cases. It tests the current domain contract. It does not test rendering, keyboard input, timing, collision, scoring, restart, or persistence because those features are not implemented. There is no coverage-percentage claim; the collector package is available but ordinary checks do not collect coverage.

## Windows UI smoke check

1. Run `./scripts/dev.ps1 run` on Windows.
2. Confirm a window titled `YMH219 — Flappy Bird` appears with the initialization label.
3. Confirm the client area uses the Playfield dimensions, allowing for Windows DPI scaling.
4. Confirm the window can move, its border is fixed, maximize is disabled, and closing exits normally.
5. Check text readability and clipping at the display scales used for submission.

Automated process/window inspection can establish that the app launches and closes; it does not establish visual correctness or debugger behavior. Record those distinctions explicitly. Do not mark future gameplay scenarios as passing during initialization.

## Execution record — 2026-11-16

Environment: Windows x64, SDK 10.0.402, .NET/Windows Desktop runtime 10.0.13. Git 2.53.0.windows.1 and GitHub CLI 2.101.0 were available.

| Check | Observed result |
| --- | --- |
| Original scaffold Release build | Passed; zero warnings/errors |
| Original scaffold test run | One passing empty test; replaced because it did not verify behavior |
| Updated foundation `check` | Passed: restore, Release build with zero warnings/errors, 10 tests passed / 0 failed / 0 skipped, formatting clean |
| Updated application launch/close | Passed: launched the Release executable hidden, found its titled window via Windows APIs, sent normal close, exit code 0 |
| Launcher environment restoration | Passed: original `DOTNET_ROOT` and `DOTNET_CLI_HOME` restored after `doctor` |
| Generated/local file exclusions | Passed: `git check-ignore` confirmed App binary, obj assets, personal `.csproj.user`, TRX, and `.local` exclusions |
| VS Code F5 and visual DPI review | Not performed |
| GitHub Actions | Not run/verified in this local task |

The first updated formatting check caught mixed line endings in the original `Program.cs`. `dotnet format FlappyBird.slnx --no-restore` normalized them, and the subsequent complete `check` passed. The launcher initially found multiple `dotnet` executables on PATH; it now explicitly selects the first resolved application and passes argument arrays by name.

The first UI probe used `Process.MainWindowHandle`, which does not identify a hidden launch's window. A second probe enumerated windows by process ID and title, including hidden windows, then sent `WM_CLOSE`; it passed. This is launch/shutdown evidence only, not a visual review. Temporary probe processes were closed.

Future test records should identify the date, commands, results, and limitations. Do not convert planned checks into executed evidence.
