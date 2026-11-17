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

The current suite has **92 cases: Core 67, App 25**. It retains Playfield, Bird/settings/state, press-edge, focus policy, fixed timing, and renderer regression coverage. New Core cases cover PipePair geometry, exclusive gap bounds, settings/playfield clearance, seeded source repeatability, movement, exact off-screen thresholds, both-pair recycling, fractional full-cycle roundoff, reset, unsupported updates, invalid provider results, update partitioning, and 60,000 fixed updates with stable instances/count/spacing. New App cases render actual pipe geometry at 1×/1.25×/1.5× sizes and verify ground alignment and that painting neither mutates pipes nor consumes gap samples.

Collision, scoring, Game Over, complete restart, and persistence remain unimplemented/untested. Domain reset primitives are tested but do not imply a restart UI. There is no coverage-percentage claim. Bitmap and helper assertions do not replace human visual/physical keyboard acceptance.

## Windows UI smoke check

1. Run `./scripts/dev.ps1 run` on Windows.
2. Confirm a window titled `YMH219 — Flappy Bird` appears with scenery, bird, and a Ready panel saying “Space to start”. Tap Space: the panel disappears and the bird rises, then falls under gravity. Hold Space: do not expect repeated flaps; release and press again to flap.
3. Confirm the client area uses the Playfield dimensions, allowing for Windows DPI scaling.
4. Confirm the window can move, its border is fixed, maximize is disabled, and closing exits normally.
5. Check text readability and clipping at 100%, 125%, and 150% display scaling where available. Distinguish off-screen render sizes from changing the desktop's actual DPI settings.
6. Minimize/restore and cover/uncover the live window; verify the same complete scene repaints cleanly.
7. Hold Space, switch away, release while away or return still holding, then test the next fresh press. Motion freezes while inactive; no catch-up jump or repeated held-key flap should occur on return.
8. Repeated taps can reach the ceiling: the bird stays within Y=0 and later falls. Ground remains decorative in this stage; falling behind it/off-screen is an expected limitation. Close/relaunch to return to Ready.
9. After starting, confirm two upper/lower pipe pairs move left, are visible together, and recycle with valid gaps. Watch several cycles. Pipe contact currently does not stop play or award points; focus loss must freeze both pipes and bird.

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
| Hosted CI | [Windows run 43819540780](https://github.com/alaamadii/FlappyBird/actions/runs/43819540780) passed for ae0d535: restore/build/test/format and TRX artifact upload all successful |

Preview images and the temporary invisible-window probe are in ignored `.local/scene-preview/`; they are diagnostic artifacts, not final gameplay screenshots or committed source. The repeated-paint, placement, and scale assertions are retained in App.Tests and run in CI. The full-window DrawToBitmap preview includes non-client chrome; it is not a desktop screenshot. No claims are made about actual gameplay or final screenshot requirements.

[Stage 01 PR #2](https://github.com/alaamadii/FlappyBird/pull/2) was confirmed merged on 2026-11-16 before Stage 02. Its final head 8afbd87 also passed [Windows run 43819762617](https://github.com/alaamadii/FlappyBird/actions/runs/43819762617). Historical unperformed visual checks are not silently marked passed by that merge.

## Stage 02 execution record — 2026-11-16

| Check | Actual evidence |
| --- | --- |
| `./scripts/dev.ps1 check` | Passed: restore, Release build with zero warnings/errors, 34 Core + 22 App tests, formatting clean |
| Invisible Form/window-message probe | Sent Space WM_KEYDOWN/WM_KEYUP messages through the Form window: first press starts/flaps, repeat is ignored after a physics update, release/new press flaps again |
| Actual timer pump | Pumped the WinForms message loop for 120 ms; bird moved upward after flap |
| Focus/minimize/restore | Invoked focus lifecycle on the invisible Form, verified timer stops and motion freezes while inactive, exercised minimize/restore, then resumed |
| Held-key reactivation | Simulated the activation-time held-Space sample in FlapInput; repeat ignored until release/new press. Actual physical-key/Alt-Tab behavior remains a manual check |
| Stalled UI | Paused the probe UI thread for 500 ms, then delivered a timer tick: exactly six fixed updates, velocity −320 → −230, no unbounded catch-up |
| Executable smoke | Hidden Release executable created its actual titled main window and exited normally with code 0 |
| Preview inspection | Inspected Ready/Running full-window DrawToBitmap images at desktop DPI 120; updated copy readable and bird position follows simulation |
| Manual checks pending | Physical Space/Alt-Tab, perceived animation smoothness, live cover/uncover, real monitor DPI changes, and VS Code F5 |
| Hosted CI | [Windows run 43828515337](https://github.com/alaamadii/FlappyBird/actions/runs/43828515337) passed for 22bf05c: restore/build/test/format and test-artifact upload successful |

The temporary probe and images are ignored under `.local/flight-probe/`; it uses synthetic messages and lifecycle calls, not physical input. The executable probe initially matched a helper window and then exposed an input-idle/main-window startup race. Restricting the lookup to the titled Flappy Bird window with a bounded startup wait resolved the probe; the actual executable then passed launch/normal shutdown. No product fix was needed for those probe failures. Diagnostic images do not fulfill the final gameplay/Game Over screenshot requirement.

[PR #3](https://github.com/alaamadii/FlappyBird/pull/3) was confirmed merged on 2026-11-16 before Stage 03. Its final head bd393ac passed [Windows run 43828762935](https://github.com/alaamadii/FlappyBird/actions/runs/43828762935). That merge does not convert unperformed physical-input/visual checks into passing evidence.

## Stage 03 execution record — 2026-11-16

| Check | Actual evidence |
| --- | --- |
| `./scripts/dev.ps1 check` | Passed: restore, Release build with zero warnings/errors, Core 66/66 and App 25/25 cases, clean formatting |
| Long-running deterministic model tests | 60,000 fixed steps preserve two original pair instances, valid gaps, left-to-right order, and 260-unit spacing; largest supported one-second step at maximum configured speed recycles both pairs correctly |
| Boundary/provider checks | Right == 0 does not recycle; moving beyond it does. Minimum gap included, maximum excluded. Invalid recycle samples are rejected before pipe positions change |
| Invisible real Form/timer probe | Actual timer message pump moves pipes alongside Bird; synthetic focus lifecycle freezes both; held-key and minimize/restore checks retained |
| Bounded stall | 500 ms UI-thread stall followed by one tick moves pipes only 14 logical units (140 × 0.1), matching six bird steps |
| Multiple cycles and repaint | Directly advanced the actual Form's engine through 60 simulated seconds with periodic Form repaint: 32 recycle events, count/spacing preserved; two full pairs confirmed together |
| Visual preview | Inspected actual full-window DrawToBitmap at current 125% desktop DPI plus 480×640, 600×800, and 720×960 off-screen previews; pipe lips/gaps/bottoms, foreground bird, banner, and ground alignment visible |
| Release executable | Hidden launch created its titled main window; normal close returned exit code 0 |
| Remaining manual checks | Human observation of real-time cycles/smoothness, physical Space/Alt-Tab, live cover/uncover, actual DPI transitions, and VS Code F5 not performed |
| Hosted CI | To be checked on the Stage 03 PR; local validation is separate evidence |

The temporary probe and images remain ignored in `.local/flight-probe/` (stage03-prefixed images). Simulated seconds were stepped directly while timer paused, so that result does not claim a human watched a real-time minute of play. The probe used domain Bird.Reset solely to place the bird for diagnostic images; it did not add restart UI. No collision, score, or loss behavior was tested or implemented. These previews do not replace final gameplay/Game Over screenshots.

### Stage 03 follow-up — 2026-11-17

Review identified a fractional-spacing case (spacing 200.1, speed 400.2) at the exact left boundary: floating-point addition could place a recycled pair just below `-Width`, prompting a spurious third recycle. Added a regression test and clamped only recycled X to its mathematical lower bound. The original `Right < 0` departure policy remains unchanged.

After the fix, `./scripts/dev.ps1 check` passed again: restore, Release build with zero warnings/errors, **67 Core + 25 App = 92 tests**, and clean formatting. The previous day's visual/lifecycle evidence remains identified separately; it was not reclassified as new manual testing.
