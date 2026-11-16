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

The Core suite contains two facts and eight theory cases, totaling ten test cases. It tests the current domain contract. The Windows-only App suite adds nine rendering cases, for **19 total tests**. It checks ground/bird placement at 1×/1.25×/1.5× sizes, centered borders for wide/tall client areas, caller graphics-state restoration, no drawing for empty client areas, and identical image output after repeated painting.

Keyboard input, timing, collision, scoring, restart, and persistence remain unimplemented and untested. There is no coverage-percentage claim; the Core collector package is available but ordinary checks do not collect coverage. Bitmap assertions verify rendering behavior, not human visual acceptance or live desktop flicker.

## Windows UI smoke check

1. Run `./scripts/dev.ps1 run` on Windows.
2. Confirm a window titled `YMH219 — Flappy Bird` appears with sky/clouds/hills, a stationary yellow bird, ground, and a readable Ready panel stating that controls arrive in Stage 02.
3. Confirm the client area uses the Playfield dimensions, allowing for Windows DPI scaling.
4. Confirm the window can move, its border is fixed, maximize is disabled, and closing exits normally.
5. Check text readability and clipping at 100%, 125%, and 150% display scaling where available. Distinguish off-screen render sizes from changing the desktop's actual DPI settings.
6. Minimize/restore and cover/uncover the live window; verify the same complete scene repaints cleanly.

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

### Publication validation — 2026-11-16

Before publishing the foundation and instructor-aligned plan, `./scripts/dev.ps1 check` passed again: zero build warnings/errors, ten passing tests, and clean formatting. All local Markdown links resolved; `git diff --cached --check` passed after removing trailing whitespace. Generated-file exclusions remained in effect.

[PR #1](https://github.com/alaamadii/FlappyBird/pull/1) was opened against the empty bootstrap `main`. Its first [GitHub Actions run](https://github.com/alaamadii/FlappyBird/actions/runs/43817906408) was observed queued at publication. The PR was subsequently confirmed merged on 2026-11-16. The historical queue observation is not a CI pass; inspect current-stage checks independently.

The first updated formatting check caught mixed line endings in the original `Program.cs`. `dotnet format FlappyBird.slnx --no-restore` normalized them, and the subsequent complete `check` passed. The launcher initially found multiple `dotnet` executables on PATH; it now explicitly selects the first resolved application and passes argument arrays by name.

The first UI probe used `Process.MainWindowHandle`, which does not identify a hidden launch's window. A second probe enumerated windows by process ID and title, including hidden windows, then sent `WM_CLOSE`; it passed. This is launch/shutdown evidence only, not a visual review. Temporary probe processes were closed.

Future test records should identify the date, commands, results, and limitations. Do not convert planned checks into executed evidence.

## Stage 01 execution record — 2026-11-16

| Check | Actual evidence |
| --- | --- |
| Full `./scripts/dev.ps1 check` | Passed: restore, Release build (zero warnings/errors), Core 10/10 and App 9/9 tests, formatting clean |
| Preview image inspection | Inspected off-screen 480×640, 600×800, and 720×960 renders (100%/125%/150% equivalents), plus an 800×800 bordered viewport; Ready copy and whole scene visible without distortion |
| Actual WinForms sizing | Invisible Form check at desktop DPI 120 (125%) produced client size 600×800; fixed border, maximize disabled |
| Repaint/minimize/restore | Invisible Form exercised through 30 invalidate/update cycles and minimize/restore; full-window bitmap output identical before/after |
| Release executable launch/close | Hidden executable created its titled window; normal WM_CLOSE shutdown exited with code 0 |
| Live visual desktop checks | Cover/uncover, perceived flicker, physical monitor transitions, desktop DPI changes to 100%/150%, and VS Code F5 not performed; owner checks remain |
| Hosted CI | To be checked on the Stage 01 PR; local results do not establish a hosted pass |

Preview images and the temporary invisible-window probe are in ignored `.local/scene-preview/`; they are diagnostic artifacts, not final gameplay screenshots or committed source. The repeated-paint, placement, and scale assertions are retained in App.Tests and run in CI. The full-window DrawToBitmap preview includes non-client chrome; it is not a desktop screenshot. No claims are made about actual gameplay or final screenshot requirements.
