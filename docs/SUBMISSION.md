# Submission and review checklist

This checklist tracks remaining work; unchecked items are not complete. The brief's overall deadline is **November 30, 2026, 23:59 Asia/Riyadh**.

## Initialization review

- [x] Solution contains App, independent Core, and xUnit Tests.
- [x] Programmatic application shell replaces generic Designer scaffolding.
- [x] SDK selection, environment launcher, shared editor settings, and VS Code configuration exist.
- [x] CI workflow, PR template, and working agreement exist.
- [x] Architecture, decisions, setup, testing, and requirements documentation exists.
- [x] Final local build/test/format evidence recorded (10 tests pass).
- [x] Hidden application window creation and normal shutdown verified.
- [ ] Visual appearance, DPI behavior, and VS Code debugging reviewed.
- [x] Requested repository `alaamadii/FlappyBird` inspected: owner push access confirmed; empty and private on 2026-11-16.
- [ ] Repository is public and accessible without authentication before submission.
- [ ] Initialization commits and PR reviewed.
- [ ] GitHub Actions passes for the reviewed commit.

## Final game acceptance

- [ ] Bird responds to documented keyboard controls; held-key behavior is predictable.
- [ ] Gravity and flap feel consistent and are covered by Core tests.
- [ ] Multiple obstacles move, recycle, and leave valid gaps.
- [ ] Pipe/ground collision triggers Game Over.
- [ ] Scoring occurs once per successfully passed obstacle.
- [ ] Restart restores a clean playable state, including after repeated restarts.
- [ ] Selected optional enhancements are verified, or explicitly excluded.
- [ ] Window, focus loss, repainting, DPI scaling, and resource disposal checked.
- [ ] Release build, tests, formatting, and Windows CI pass.
- [ ] Clean checkout can restore/build/run using README instructions.
- [ ] At least two real screenshots of gameplay and Game Over stored in `docs/screenshots/` and embedded in README.
- [ ] README includes the student's full name and student number.
- [ ] README lists actual technologies and NuGet packages.
- [ ] README explains actual controls, features, limitations, and supported environment.
- [x] Original instructor PDF read and requirements reconciled in REQUIREMENTS and PLAN on 2026-11-16.
- [ ] Final documentation and code agree.
- [ ] Project opened and run on another Windows computer (record actual result).
- [ ] No secrets, personal IDE state, `bin`, `obj`, `.local`, or test output committed.
- [ ] Source, assets, and any selected license/attributions reviewed.
- [ ] Final reviewed commit or release identifier recorded for submission.
- [ ] Final code pushed before 2026-11-30 23:59.
- [ ] Owner submits their GitHub profile URL in Classroom.

## Prepare to explain the code

Be ready to identify each class's responsibility, show encapsulation in an actual gameplay model, explain why Core does not reference Forms, trace a keyboard event and timer update, demonstrate collision and one-time scoring, and show how restart clears state. Explain a real test and a real architectural decision using the repository. These gameplay examples become applicable as their stages are implemented.

Do not manufacture screenshots, results, dates, or commit activity for assessment. Use the development log and PR discussions as evidence of the actual work.
