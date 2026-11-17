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

The current suite has **157 cases: Core 92, App 65**. Existing model, pipe, gap/randomness, timing, input, scaling, and repaint coverage remains. Stage 04 adds inclusive/swept contact, loss, score threshold/ordering/eligibility, frozen GameOver, existing ground contact, largest supported updates, 40 loss/reset cycles, score/overlay rendering, shared restart layout, and invisible STA Form tests. Twelve UI restart cycles reuse the timer/pairs and alternate button and focused-button Enter; held-Space and time-debt tests cover App reset wiring.

Stage 05 adds isolated file/service tests for first launch, valid maximum, fresh-instance reload, invalid/oversized/UTF-8 data, retained maximum, locked reads/replacement, unchanged valid files, retry and status. Form tests prove best survives restart/fresh windows, one save per completed best, no writes from repaint/focus/ticks/reset, failure-tolerant gameplay, pause descriptions, native font/bounds at three logical scales, and keyboard focus color. Renderer checks include best, pause, and warnings without model mutation. See [HIGH_SCORE.md](HIGH_SCORE.md) for isolation. Audio/progression remain deferred.

Standalone Bird tests intentionally retain pure arithmetic behavior below ground; GameEngine tests enforce contact and loss. Renderer geometry tests can arrange supplied models directly and do not imply a collision-free playthrough. There is no coverage-percentage claim. Invisible window/bitmap checks do not replace human physical-input/visual acceptance.

## Windows UI smoke check

1. Run `./scripts/dev.ps1 run` on Windows.
2. Confirm a window titled `YMH219 — Flappy Bird` appears with scenery, bird, and a Ready panel saying “Space to start”. Tap Space: the panel disappears and the bird rises, then falls under gravity. Hold Space: do not expect repeated flaps; release and press again to flap.
3. Confirm the client area uses the Playfield dimensions, allowing for Windows DPI scaling.
4. Confirm the window can move, its border is fixed, maximize is disabled, and closing exits normally.
5. Check text readability and clipping at 100%, 125%, and 150% display scaling where available. Distinguish off-screen render sizes from changing the desktop's actual DPI settings.
6. Minimize/restore and cover/uncover the live window; verify the same complete scene repaints cleanly.
7. Hold Space, switch away, release while away or return still holding, then test the next fresh press. Motion freezes while inactive; no catch-up jump or repeated held-key flap should occur on return.
8. Ceiling contact clamps without loss. Pipe/ground contact shows Game Over and final score; motion and score must stop. A safe complete pass adds one point, never two for one pair.
9. Watch multiple pipe cycles in a valid run, then test New game by click and Enter. Confirm Ready/score zero/initial pipes, release Space if held, and start a fresh run. Repeat restarts; focus the button and check Space does not click it. Focus loss/resume on GameOver must leave the run stopped.
10. Confirm distinct Score/Best labels, visible Paused guidance while inactive, and the gold New game focus border. Earn a new best and lose, restart, then relaunch: current score returns to zero and best survives. Use an isolated injected store for automated score-producing diagnostics; normal tests must not write the owner's real score.

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
| Hosted CI follow-up (2026-11-17) | [Windows run 44027176448](https://github.com/alaamadii/FlappyBird/actions/runs/44027176448) passed for e847e0f, including restore/build/test/format and artifact upload |

The temporary probe and images remain ignored in `.local/flight-probe/` (stage03-prefixed images). Simulated seconds were stepped directly while timer paused, so that result does not claim a human watched a real-time minute of play. The probe used domain Bird.Reset solely to place the bird for diagnostic images; it did not add restart UI. No collision, score, or loss behavior was tested or implemented. These previews do not replace final gameplay/Game Over screenshots.

### Stage 03 follow-up — 2026-11-17

Review identified a fractional-spacing case (spacing 200.1, speed 400.2) at the exact left boundary: floating-point addition could place a recycled pair just below `-Width`, prompting a spurious third recycle. Added a regression test and clamped only recycled X to its mathematical lower bound. The original `Right < 0` departure policy remains unchanged.

After the fix, `./scripts/dev.ps1 check` passed again: restore, Release build with zero warnings/errors, **67 Core + 25 App = 92 tests**, and clean formatting. The previous day's visual/lifecycle evidence remains identified separately; it was not reclassified as new manual testing.

[PR #4](https://github.com/alaamadii/FlappyBird/pull/4) was confirmed merged on 2026-11-17 before Stage 04. Its final head c0f215a also passed [Windows run 44027520323](https://github.com/alaamadii/FlappyBird/actions/runs/44027520323). Historical unperformed manual checks are not reclassified as passed.

## Stage 04 execution record — 2026-11-17

| Check | Actual evidence |
| --- | --- |
| Full `./scripts/dev.ps1 check` | Passed: restore, Release build with zero warnings/errors, Core 92/92 and App 33/33, clean formatting |
| Swept/contact rules | Tests cover upper/lower pipe edges, corners/overlap/misses, thin obstacle between disjoint endpoints, diagonal interval separation, largest one-second update, existing ground touch/overlap, ceiling non-loss |
| Score/loss/reset | Tests cover exact pass threshold, no double point, score before recycle, eligibility on a later cycle, collision-step priority, retained score/frozen GameOver, invalid elapsed without mutation, 40 reset cycles |
| Native UI tests | Invisible STA Form, synthetic window messages, twelve repeated button/Enter resets, focused Space suppression, held-Space release requirement, focus-return GameOver preservation, accumulator reset, same timer/pairs |
| Actual Form probe | Controlled gaps and scripted flaps produced a valid pass/score 1, then pipe loss with final score 1, normal ground loss, BM_CLICK and focused Enter restart; focus/minimize/restore, 500 ms bounded stall, and timer disposal passed |
| Release executable | Hidden executable created its titled main window and closed normally with exit code 0 |
| Preview review | Inspected Ready, scored Running, and Game Over full-window DrawToBitmap captures at current 125% DPI; Running 100%/125%/150% equivalents also generated; native button aligns with final-score card |
| Still manual | Human random-gap playthrough/tuning, physical Space/Alt-Tab/restart, perceived smoothness, cover/uncover, actual monitor DPI transitions, VS Code F5, second computer |
| Hosted CI | [Windows run 44043723178](https://github.com/alaamadii/FlappyBird/actions/runs/44043723178) passed for f0a9300 after native-window test isolation; build/test/format and artifact upload successful |

Diagnostic probe/images are ignored under `.local/rules-probe/`. Controlled gaps/direct simulation were used for repeatable score/loss captures; this is not a human random-gap playthrough or a final submission screenshot sign-off. A ground-overlap regression prevents an already overlapping state from producing negative contact time. Initial compile failures from the renderer's new score argument were resolved by updating callers/tests before final verification. Stage 04 acceptance remains pending owner review.

### Hosted test isolation follow-up

The first [Stage 04 CI run](https://github.com/alaamadii/FlappyBird/actions/runs/44043016504) built cleanly and passed all 92 Core cases, but one repeated-paint comparison failed while native Form tests and off-screen renderer tests could run in parallel. The new native-window collection disables parallel execution with other collections, isolating WinForms process-wide DPI/font initialization from exact bitmap comparisons. The exact assertion is retained. After isolation, the full local check and [hosted run 44043723178](https://github.com/alaamadii/FlappyBird/actions/runs/44043723178) passed all 125 tests; the earlier failure remains recorded rather than relabeled.

[PR #5](https://github.com/alaamadii/FlappyBird/pull/5) was subsequently confirmed merged on 2026-11-17 before Stage 05. Its final head fe75efa passed [Windows run 44044135287](https://github.com/alaamadii/FlappyBird/actions/runs/44044135287). Historical unperformed manual checks remain pending.

## Stage 05 execution record — 2026-11-17

| Check | Actual evidence |
| --- | --- |
| `./scripts/dev.ps1 test` | Passed after the layout fix: 92 Core + 64 App = 156 cases, zero failed/skipped |
| Full `./scripts/dev.ps1 check` | Passed: restore, Release build with zero warnings/errors, all 156 tests, and formatting verification |
| Final storage-boundary review/check | Corrected direct Save(0) over corrupt data to write a reloadable zero instead of returning early. Added regression; full `./scripts/dev.ps1 check` passed again with **92 Core + 65 App = 157 cases**, zero warnings/errors/failures/skips, formatting clean |
| File/service behavior | Valid zero/maximum values, missing directories, corrupt/oversized/invalid UTF-8 content, fresh-instance reload, maximum across runs, expected read/write/security failures, session best/retry tested with isolated data |
| Preservation on failed save | A test-owned file held open without delete sharing prevents replacement; old value remains unchanged and no temporary file remains after the failure |
| Native Form regression | Best retained after reset/fresh window; one save on completed new best, none on repeated paints/focus/ticks/reset; failure still allows loss/restart/play. Existing nonparallel native collection retained |
| Scaling/focus | Actual invisible Form forced to 480×640, 600×800, 720×960 client sizes: mapped native button bounds, 16/20/24-pixel bold fonts and focus border validated. These are equivalent sizes, not physical desktop DPI changes |
| `dotnet run --project .local/polish-probe/polish-probe.csproj -c Release` | Passed: isolated missing file, loss/save, restart, fresh-Form reload, locked-file save failure, retained memory best, retry on next completed run, close |
| `dotnet run --project .local/rules-probe/rules-probe.csproj -c Release` | Passed: scripted scored pass, pipe/ground loss, button/Enter reset, held Space, focus/minimize/restore, bounded stall, disposal; actual hidden Release executable launch and WM_CLOSE exit 0 |
| Capture inspection | Inspected native DrawToBitmap Ready with reloaded best at 125%, Running at 100%, Paused at 150%, Game Over at 150%, and unsaved-best Game Over/focused button at 100%. Text/card/control placement is readable in these captures |
| Formatting | `dotnet format FlappyBird.slnx --no-restore` succeeded; full verify recorded below. An initial `dev.ps1 format` attempt was rejected because that command is not supported; use dotnet format directly |
| Hosted CI | [Windows run 44051252291](https://github.com/alaamadii/FlappyBird/actions/runs/44051252291) passed for 88626d3: zero build warnings/errors, 92 Core + 64 App cases, formatting verification and TRX artifact upload successful |
| Pending owner checks | Human random-gap playthrough/fairness, physical Space/Alt-Tab/Tab/Enter/mouse, real-time smoothness, live cover/uncover, real 100%/125%/150% DPI/cross-monitor transitions, VS Code F5, second computer |

The added scale test initially found stale button bounds after a fixed Form changed client size. GameForm now remaps after ClientSizeChanged as well as layout; the same assertions pass without relaxation. Corrupt-data exceptions are explicitly handled in addition to I/O/access/security exceptions. No Core source or tuning changed. Scripted passes and controlled collision placement are not human playtesting.

All existing Form tests now inject memory storage. All earlier score-producing local probes use explicit `.local` storage, and the polish probe uses a unique path there. Diagnostic images remain ignored and do not fulfill Stage 06 screenshot sign-off. The actual executable smoke stays in Ready, reads its production best at startup, and closes without a completed run or score write. Owner data was not edited. Stage 05 acceptance remains pending; Stage 06 has not started.

[PR #6](https://github.com/alaamadii/FlappyBird/pull/6) is open against main and is not merged. This publication-evidence update is documentation-only; its final-head check is tracked on the PR before handoff. The passing run above identifies its exact tested commit rather than implying a result for a later head.

The later direct-store zero/corrupt fix adds one case beyond that initial hosted run. Production still saves only improved completed bests; this correction makes the public storage boundary consistent for its supported zero value. It does not alter gameplay, UI, or prior visual evidence. Final-head hosted results are tracked in PR #6.

## Stage 06 final-evidence record — 2026-11-17

The owner approved merging PR #6 at its checked head 5043189; [Windows run 44052119853](https://github.com/alaamadii/FlappyBird/actions/runs/44052119853) passed all 157 cases, formatting and artifact upload. The merge produced main commit 97416ab. Owner-authorized public visibility was then applied. Historical pending observations above remain as execution history, not current PR status.

| Check | Actual result |
| --- | --- |
| Fresh remote checkout | `git clone --branch feat/game-polish --single-branch https://github.com/alaamadii/FlappyBird.git .local/final-qa-checkout` checked out 5043189 with no reused local source/build output |
| Clean-checkout full check | `./scripts/dev.ps1 check` from that clone passed restore, Release build with zero warnings/errors, 92 Core + 65 App cases, zero failures/skips and formatting verification; checkout remained clean afterward |
| Final branch local check | `./scripts/dev.ps1 check` on `docs/final-submission` also passed restore, Release build with zero warnings/errors, all 157 tests and formatting. All local Markdown targets resolved across 13 documentation files; whitespace check passed |
| Clean-checkout executable | Native-window probe launched that clone's actual Release EXE hidden, found its titled window, sent WM_CLOSE and observed exit 0. It stayed Ready and wrote no personal score |
| Mandatory gameplay regression | Re-ran isolated rules probe: valid scored pass, pipe/ground loss, click/Enter reset, focused-button Space suppression, held Space, focus/minimize/restore, bounded stall, disposal and normal close all passed |
| Optional persistence regression | Re-ran isolated polish probe: first launch, best save, reset retention, fresh-window reload, locked-file failure, memory best/retry and close passed |
| Screenshot capture | Temporary final-evidence probe captured the actual GameForm using PrintWindow at 120 DPI, 600×800 client/618×847 window. Controlled gaps/scripted flaps earned a real point; stopping flaps led to natural ground loss, saved best and successful reset |
| Screenshot inspection | Both target-window PNGs inspected and embedded in README; bird/pipes/gap/Score/Best readable in Running, final/best/restart/focus visible in Game Over. No unrelated desktop content included. [Provenance/checksums](screenshots/README.md) |
| Dependency inventory | `dotnet list FlappyBird.slnx package` confirmed the README's top-level test packages and no App/Core third-party packages |
| Public access | Anonymous repository metadata reported private=false, repository page returned HTTP 200, and repository appeared in anonymous public-profile listing after owner-authorized visibility change |
| Tracked history/content | Reviewed tracked file names across history and current content; limited regex scan found no common GitHub-token/AWS-key/private-key patterns. No .local/bin/obj/TRX/personal score output tracked |
| Remaining owner evidence | Full name/student number, screenshot acceptance, human random-gap fairness, physical input/focus/smoothness/cover-uncover, actual 100%/150% and cross-monitor DPI, VS Code F5, another Windows computer, final PR/main sign-off and Classroom profile-link submission |

The helper commands remain local diagnostics, not required build dependencies. The clone is a second folder on this computer, not the second-computer acceptance test. Actual target-window captures do not claim physical-key input or human playtesting. No game source, tuning, dependencies or assets changed beyond the inspected application captures. [SUBMISSION.md](SUBMISSION.md) separates verified technical checks from final owner sign-off.
