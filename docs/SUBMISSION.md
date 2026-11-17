# Submission and review checklist

Updated **2026-11-17**. Gameplay and optional persistence are implemented; final evidence is prepared on `docs/final-submission`. PR #6 was merged with explicit owner approval. Unchecked items still require owner action; this document does not claim submission is complete. The deadline is **November 30, 2026, 23:59 Asia/Riyadh**.

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
- [x] Repository made public with owner approval; anonymous page/API access and profile listing verified 2026-11-17.
- [x] Initialization PR #1 merged by the owner (confirmed 2026-11-16).
- [x] GitHub Actions passes for reviewed gameplay commit 5043189: [Windows run 44052119853](https://github.com/alaamadii/FlappyBird/actions/runs/44052119853).

## Final game acceptance

- [x] Keyboard/held-key wiring passes native-window tests and synthetic-message probes; physical-key acceptance remains below.
- [x] Gravity/flap arithmetic and timing are regression-tested; human feel/fairness remains below.
- [x] Two pipe pairs move/recycle with valid gaps; long-running deterministic tests pass.
- [x] Pipe/ground contact triggers Game Over under inclusive/swept rule tests and Form probes.
- [x] Scoring occurs once per valid pass, after contact checking and before recycling.
- [x] Repeated Core/native Form resets restore clean state and retain held-key protection.
- [x] Persistent best save/reload/failure/retry verified with isolated storage; audio, progression and difficulty selection deferred.
- [x] Automated repaint/focus/disposal and equivalent-scale checks pass; actual 125% captures inspected. Physical display checks remain below.
- [x] Release build, 157 tests, formatting and reviewed gameplay CI pass; final documentation PR CI is a separate gate below.
- [x] Fresh GitHub checkout at 5043189 restored/built/checked and its Release executable launched/closed normally on this computer.
- [x] Two actual application-window captures in `docs/screenshots/` embedded in README with scripted-run captions.
- [ ] README includes the student's full name and student number.
- [x] README lists actual technologies/NuGet packages; references checked with `dotnet list FlappyBird.slnx package`.
- [x] README explains actual controls, features, storage, limitations and supported environment.
- [x] Original instructor PDF read and requirements reconciled in REQUIREMENTS and PLAN on 2026-11-16.
- [x] Technical documentation reconciled with source/current scope; missing owner information explicitly identified.
- [ ] Project opened and run on another Windows computer (record actual result).
- [x] Tracked files/history reviewed; no generated/private output tracked and no common credential-pattern matches found. This limited scan is not a security certification.
- [x] Source/assets reviewed: original GDI+ artwork and actual window captures; no external game assets. No license selected.
- [ ] Final reviewed commit or release identifier recorded for submission.
- [ ] Final code pushed before 2026-11-30 23:59.
- [ ] Owner submits their GitHub profile URL in Classroom.

## Owner sign-off before calling the assignment complete

- [ ] Supply the full name and student number for README; review those public identity fields.
- [ ] Accept the two screenshots or replace them with owner captures.
- [ ] Play random-gap runs using physical Space. Check tap/hold/release, a valid scored pass, pipe/ground loss, repeated click/Enter restart, and focused-button Space suppression.
- [ ] Hold Space while switching away, release away/return held, and confirm pause/resume and fresh-press protection. Check real-time smoothness, minimize/restore and cover/uncover.
- [ ] Inspect actual 100%, 125% and 150% Windows display scaling and cross-monitor transitions where available; bitmap-size equivalents alone do not complete this check.
- [ ] Verify VS Code F5 debugging on the final checkout.
- [ ] Record another Windows computer's result below. CI/invisible-window tests and a second folder on this computer do not replace this manual check.
- [ ] Review the final documentation PR and exact-head Windows CI; approve merging, then verify/record the resulting main commit and its checks.
- [ ] Submit only [the GitHub profile URL](https://github.com/alaamadii) in Classroom before the deadline; automated Classroom submission is not authorized.

## Another-computer record

After final merging, use a different Windows computer with the selected .NET SDK:

```powershell
git clone https://github.com/alaamadii/FlappyBird.git
Set-Location FlappyBird
git rev-parse HEAD
./scripts/dev.ps1 check
./scripts/dev.ps1 run
```

Try a scored run, loss, Enter/button reset, close/relaunch and saved-best reload. Use that computer's own user profile and record real evidence:

| Field | Actual result |
| --- | --- |
| Date / tester | Pending |
| Windows version / SDK | Pending |
| Tested commit | Pending |
| Check command result | Pending |
| Launch, controls, pass/score, loss, restart | Pending |
| Saved-best reload / shutdown | Pending |
| Problems and resolution | Pending |

## Prepare to explain the code

Use [CODE_GUIDE.md](CODE_GUIDE.md) to identify responsibilities, show encapsulation, explain dependency direction, trace input/timing, demonstrate contact/scoring, and explain reset/best-score storage. Explain a real test and decision with the source open. Practice remains an owner responsibility.

Do not manufacture screenshots, results, dates, or commit activity for assessment. Use the development log and PR discussions as evidence of the actual work.
