# Flappy Bird completion plan

**Project:** YMH219 Flappy Bird desktop assignment

**Repository:** https://github.com/alaamadii/FlappyBird

**Plan created:** 2026-11-16

**Submission deadline:** 2026-11-30, 23:59 (working schedule: Asia/Riyadh)

**Current position:** Stages 00–02 merged in PRs #1–#3. Stage 03 pipe geometry, generation, movement, recycling, and rendering are implemented on `feat/pipe-system`; owner acceptance is pending. Collision and scoring rules have not started.

This is the working plan for finishing **Flappy Bird only**. The other four NTP assignments have separate repositories and are outside this plan. Update this file as stages are accepted, recording actual completion dates rather than replacing targets with assumed results.

## 1. Source of requirements

The instructor's `YMH219_Ek_Uygulama_Odevleri_Ogrenci_Foyu.pdf` was read in full on 2026-11-16. Relevant sections are page 1 (general development and GitHub rules), page 2 (Flappy Bird), and page 5 (README, assessment, submission). The classroom announcement supplied by the owner confirms the deadline and profile-link submission.

The PDF defines assignment requirements. Branches, PR reviews, automated tests, VS Code, and the architecture below are our development choices. The PDF names Visual Studio as the environment; our agreed editor is VS Code, while the delivered application remains C#/.NET Windows Forms. Confirm any editor-specific grading expectation with the instructor before submission.

The PDF recommends `YMH219-FlappyBird`; the owner explicitly selected `alaamadii/FlappyBird`. Keep the requested repository and identify the course clearly in its README. The PDF's general naming guidance is a recommendation, whereas public access, source files, `.gitignore`, README content, and screenshots are submission requirements.

## 2. What finished means

The game is complete when a fresh Windows checkout can restore, build, and run; the player can flap, fall under gravity, pass moving obstacles, earn points, lose on collision, and start a clean new run. At least two obstacles must move on screen and recycle. We will implement **at least two pipe pairs**, each containing upper/lower pipes, so the minimum obstacle requirement is unambiguous.

Completion also requires understandable OOP code, meaningful tests, successful Windows CI, reviewed development commits, two real screenshots embedded in README, student identity details, and an accessible public repository before the deadline. A playable game without these deliverables is not submission-ready.

## 3. Working agreement and responsibilities

- The assistant implements the approved stage, runs relevant checks, updates documentation, and publishes changes when authorized. It reports actual evidence and known limitations.
- The owner reviews the PR, tries the application, supplies full name/student number, and approves merging. The owner also submits the GitHub profile URL through Classroom.
- Each stage has its own branch and PR. Review and merge the previous stage before beginning dependent implementation. Never auto-merge.
- Use focused commits that represent real changes; do not backdate activity or split finished work into artificial history.
- Publish actual source and docs, excluding build output, test output, local tool state, and secrets.
- A stage is accepted only after its acceptance criteria and applicable checks pass and the owner reviews it. A local pass is not a hosted CI pass or a visual review.

The new remote initially has no commits or branches. Establish an empty bootstrap commit as the `main` base, then publish initialization code on `chore/initialize-project` for its PR. The bootstrap contains no application changes. Later stage branches start from the reviewed `main` tip. Preserve all existing remote work if any appears before publication; never force-push over it.

## 4. Intended design and behavior contracts

These contracts guide implementation; they are not claims about current functionality. Record any necessary revision in [DECISIONS.md](DECISIONS.md) and update the relevant tests.

| Component | Responsibility | Main verification |
| --- | --- | --- |
| `Playfield` (exists) | Immutable dimensions and ground boundary | Dimension validation tests |
| `Bird` | Position, vertical velocity, flap, elapsed-time gravity | Numeric movement and reset tests |
| `PipePair` | Upper/lower geometry, gap, movement, pass eligibility | Geometry, movement, and recycle tests |
| `GameSettings` | Validated physics, speed, spacing, and gap configuration | Reject invalid or impossible settings |
| `GameEngine` | Entity updates, collision, score, state, and clean restart | Deterministic integration tests in Core |
| `GameState` | Ready, Running, GameOver | Transition tests |
| `GameForm` | Key events, timer, focus, UI controls, and lifecycle | Windows manual checks |
| `GameRenderer` | Draw Core state through GDI+ without changing it | Repaint/DPI/resource checks |
| High-score storage (optional) | Best-score persistence outside the repository | Storage success and failure tests |

Core uses numeric domain geometry rather than WinForms or `System.Drawing` types. App references Core; Core never references App. Provide controllable randomness to pipe generation so Core tests can reproduce arrangements without statistical flakiness.

### Input and state

- Use **Space** to start from Ready and flap while Running. One fresh press produces one flap; holding the key does not repeatedly flap through OS key-repeat. `KeyUp` releases the latch.
- On GameOver, **Enter** or a visible **New Game / Restart** control resets to Ready. A subsequent Space starts the next run. Ignore flap input while GameOver.
- Reset pressed-key state on restart and focus loss. Freeze updates while inactive and reset elapsed-time tracking when focus returns, preventing a large catch-up jump or a stuck key.
- Keep gameplay on the WinForms UI thread. Stop/dispose the timer with the window. Painting never advances physics.

### Physics and timing

- Use top-left origin, X rightward, Y downward. Gravity increases vertical velocity; a flap sets upward velocity. Use seconds and floating-point values in Core.
- The intended App loop uses `System.Windows.Forms.Timer` as a scheduling signal, a monotonic elapsed-time source, and fixed 1/60-second Core updates. Cap elapsed backlog and work per tick so a stalled window cannot trigger unbounded catch-up. Document/tune exact caps with stage 02 tests and manual checks.
- Centralize tuning values in validated settings. Tune gravity, impulse, speed, spacing, and gap through playtesting rather than scattering constants through the Form.
- Keep the bird inside the top boundary with a documented clamp policy. Ground contact or overlap with a pipe ends the run; collision takes precedence over awarding a new point in that update.

### Obstacles and score

- Maintain at least two active pipe pairs moving right to left. Generate gaps entirely above the ground, with sufficient clearance for the bird and pipe ends.
- Recycle a pair only after its right edge is off-screen, positioning it after the furthest remaining pair with consistent spacing. Reset its score eligibility and generate a valid new gap.
- Award one point when a pair's trailing edge passes the bird's left edge in a still-valid run. Never award a second point for the same pass.
- Use a shared collision-boundary policy: pipe/ground touching counts as collision. Test exact contact as well as overlaps and misses. Restrict maximum per-update movement or add swept checks if testing reveals tunneling through thin obstacles.

## 5. Stage-by-stage delivery

### Stage 00 — Initialization and plan

**Branch:** `chore/initialize-project`

**Deliverables:** App/Core/Tests solution, SDK/environment setup, shell, meaningful baseline tests, CI, working agreement, and complete project documentation including this plan.

Current foundation has a programmatic `GameForm`, validated `Playfield`, and ten passing Core test cases. Publish that foundation and this plan for review. Resolve any CI failures before considering the stage accepted.

**Acceptance:** Release build has zero warnings/errors; all existing tests and formatting pass; window creation/shutdown verified; owner completes visual/debugger checks; generated files excluded; initialization PR reviewed and hosted CI passes. Public visibility remains a submission action if still private.

**Documentation:** README, SETUP, ARCHITECTURE, DECISIONS, DEVELOPMENT, TESTING, REQUIREMENTS, SUBMISSION, and PLAN agree on actual status.

**Suggested commit subjects:** `chore: initialize tested WinForms project foundation`; `docs: add instructor-aligned Flappy Bird completion plan`.

### Stage 01 — Game window and rendering

**Branch:** `feat/game-window`

**Work:** Add `Rendering/GameRenderer`, draw a clear background, ground, and bird, and show Ready instructions. Establish coordinate/DPI mapping and flicker-free repainting. Use simple original GDI+ shapes initially; introduce image assets only with clear licensing/attribution and a real visual need.

**Tests/checks:** Validate any new Core geometry behavior with unit tests. Manually inspect 100%, 125%, and 150% display scaling where available, minimize/restore, cover/uncover, and repeated repaint. Ensure fonts/brushes/pens are disposed or reused safely.

**Acceptance:** The scene is readable, correctly scaled, and stable through repaint; drawing does not change state. Gameplay input and movement are not required for this stage.

**Documentation:** Explain rendering ownership and coordinate mapping in ARCHITECTURE; record visual checks in TESTING and stage results in DEVELOPMENT. Temporary screenshots are optional and do not replace final gameplay screenshots.

### Stage 02 — Bird physics, input, and loop

**Branch:** `feat/bird-physics`

**Work:** Add Bird/settings, elapsed-time updates, a timer adapter, Space press/release handling, focus handling, and the Ready → Running transition. Complete ceiling behavior and reset primitives. Keep physics independent of the Form.

**Tests/checks:** Verify gravity direction, flap velocity, controlled time steps, invalid elapsed time, top clamp, reset values, and held-key suppression. Manually check tap/hold/release, focus loss during a held key, reactivation, and window responsiveness during stalls. Document the integration formula and timing limits actually used.

**Acceptance:** Space starts and flaps once per press; the bird rises then falls predictably; no stuck keys or large focus-return jump; timer/resources close cleanly.

**Documentation:** Add controls to README and timing/input decisions to DECISIONS. Update ARCHITECTURE, TESTING, and DEVELOPMENT.

### Stage 03 — Pipe system

**Branch:** `feat/pipe-system`

**Work:** Add at least two PipePair objects, valid gap generation, leftward movement, off-screen recycling, spacing rules, and pipe rendering. Add controllable randomness for deterministic tests.

**Tests/checks:** Verify gap bounds, playable clearance, valid settings, movement over elapsed time, exact off-screen threshold, repeated recycling, large allowed updates, and preserved order/spacing. Play long enough to see multiple cycles without gaps collapsing or obstacle counts growing.

**Acceptance:** At least two pairs participate in the obstacle system and are visible together during normal play. Pairs recycle predictably after leaving the screen, without invalid geometry or unbounded allocation.

**Documentation:** Describe pair count, generation constraints, and randomness contract. Update REQUIREMENTS, TESTING, DEVELOPMENT, and DECISIONS as needed.

### Stage 04 — Collision, score, Game Over, restart

**Branch:** `feat/game-rules`

**Work:** Complete GameEngine coordination and state transitions. Add pipe/ground collision, one-time scoring, live score display, Game Over/final score display, and restart via Enter and a visible UI control.

**Tests/checks:** Verify exact contact, overlap, clear misses, upper/lower pipe collision, ground contact, scoring only after a complete pass, no double scoring, collision-before-score ordering, no updates/scoring after GameOver, input ignored in GameOver, and full reset of entities, score, eligibility, state, timing, and key latches. Test many consecutive restarts.

**Acceptance:** Every mandatory page-2 gameplay requirement passes automated checks where practical and a complete manual playthrough. Restart produces the same clean starting contract each time.

**Documentation:** README accurately describes controls and implemented behavior. Requirements matrix links to the implemented classes/tests. Record edge policies and actual results; keep optional features separate.

### Stage 05 — Usability, polish, and selected extras

**Branch:** `feat/game-polish`

**Required work:** Improve contrast, instructions, score legibility, overlay layout, keyboard/button focus, and overall consistency. Tune a fair initial difficulty through playtesting.

**Optional priority:** Persistent high score first; progressive speed second if the mandatory game is stable and time remains. Sound and selectable difficulty are stretch goals. If these extras do not fit the schedule, explicitly defer them; they are not minimum requirements.

If persistence is selected, use a user-local application-data path, not the checkout. Missing/corrupt/inaccessible files must not prevent play. If speed progression is selected, bound it and preserve playable obstacle spacing and collision reliability.

**Tests/checks:** Regression-test mandatory rules after tuning. For selected extras test storage/reset/failure behavior or speed bounds/thresholds as applicable. Check repeated play/restart and window responsiveness.

**Acceptance:** Mandatory functionality remains stable; included extras work and are documented; deferred extras are clearly listed. No new asset or package is added without recording its source and purpose.

**Documentation:** Update README features/limitations, persistence or progression decisions, test evidence, and development log.

### Stage 06 — Final QA, evidence, and submission readiness

**Branch:** `docs/final-submission`

**Work:** Freeze feature scope, resolve remaining defects in reviewed changes, verify a clean checkout, and reconcile the entire README and docs with the final code.

**Acceptance checklist:**

- Run the complete `check` command and verify hosted Windows CI for the final commit.
- Perform the full manual gameplay checklist, repeated restart/focus tests, and display-scale review.
- Test on another Windows computer, as the instructor's checklist requests. A second folder or clean checkout on the same computer establishes reproducibility but does not replace the second-computer check.
- Capture at least **two real application screenshots**, preferably active gameplay with score and Game Over with final score/restart. Store them in `docs/screenshots/` and embed both in README with captions.
- Fill in the owner's full name and student number in README. The assistant must not guess these values.
- List actual technologies and NuGet dependencies, final controls, setup/run commands, features, known limitations, and asset attribution.
- Verify the repository is public and visible from the owner's GitHub profile without authentication. Obtain owner authorization before changing private visibility; it exposes the entire repository history.
- Confirm generated output/secrets are excluded and the final reviewed code is on the default branch.
- Record the final commit identifier and check that it is pushed before the deadline. Late fixes may count as late submission; do not alter commit dates.
- The owner submits their **GitHub profile URL** in Classroom. This plan does not authorize the assistant to submit to Classroom or modify other repositories.
- Prepare to explain the class responsibilities, one event path, the physics loop, collision/score/reset logic, and a meaningful test; practice a small code change.

Use [SUBMISSION.md](SUBMISSION.md) as the detailed sign-off checklist. A final executable/package can be provided if useful, but it does not replace buildable source or the README.

## 6. Target schedule and review gates

These are planning targets, not recorded completion dates or guarantees. Owner review is required at each gate. If a gate slips, update this table, keep mandatory behavior first, and reduce optional stage-05 scope before reducing validation.

| Target dates (2026) | Stage | Review outcome |
| --- | --- | --- |
| November 16–17 | 00 | Initialization and plan PR reviewed; CI verified |
| November 18 | 01 | Static scene and rendering accepted |
| November 19–20 | 02 | Bird/input/loop accepted |
| November 21–22 | 03 | Moving/recycled pipe pairs accepted |
| November 23–24 | 04 | Mandatory game complete and reviewed |
| November 25–26 | 05 | Usability completed; only feasible extras included |
| November 27–29 | 06 | Final QA, second-computer check, screenshots, README, accessibility verified |
| November 30, before 23:59 | Final buffer | Confirm pushed final state and owner profile-link submission |

## 7. Common definition of done for each PR

1. Implement only the stage scope and preserve the Core/App boundary.
2. Run `./scripts/dev.ps1 check`; include real commands and outcomes in the PR.
3. Perform the stage's manual checks and report anything not performed.
4. Update DEVELOPMENT with the actual date and summary, TESTING with evidence, and requirements/architecture/decisions when affected.
5. Inspect the staged diff; exclude generated/private files and unsupported claims.
6. Push meaningful commits to the stage branch, open/update its PR, and inspect hosted checks.
7. Resolve failures before owner review. Record deferred items without silently calling them complete.
8. Owner reviews and approves merging; begin the next dependent stage from that reviewed base.

## 8. Risks and remaining owner input

| Item | Handling |
| --- | --- |
| Remote currently private | Track public visibility as an unmet delivery requirement; owner authorizes publication before final submission. |
| Missing identity details | Owner supplies full name and student number before final README sign-off. |
| Visual Studio named in PDF | Retain required WinForms output; confirm whether an editor-specific deliverable is expected. |
| Timer delays or held keys | Fixed updates, bounded catch-up, key latch, focus reset, and manual checks. |
| Random obstacle tests | Inject controlled randomness; verify bounds deterministically. |
| Optional features consuming buffer | Complete stage 04 first and defer extras if needed. |
| No second Windows computer available | Arrange access early; record an honest outstanding check until actually performed. |
| Deadline ambiguity | Schedule uses the owner's Asia/Riyadh context; confirm institutional timezone if different. |

## 9. Progress record

| Stage | Implementation | Validation/review status | Actual acceptance date |
| --- | --- | --- | --- |
| 00 | Foundation and plan merged in PR #1 | Owner merged on 2026-11-16; historical manual/debugger checks remain recorded in TESTING | 2026-11-16 (merge) |
| 01 | Static renderer and tests merged in PR #2 | Owner merged 2026-11-16; historical visual checks remain recorded in TESTING | 2026-11-16 (merge) |
| 02 | Bird flight and loop merged in PR #3 | Owner merged 2026-11-16; historical physical input/visual checks remain recorded in TESTING | 2026-11-16 (merge) |
| 03 | Two pairs, validated gap generation, movement/recycling, and rendering implemented | Local 92-test check (2026-11-17) and invisible pipe/lifecycle probe pass; hosted CI and owner review pending | — |
| 04 | Not started | Pending | — |
| 05 | Not started | Pending | — |
| 06 | Not started | Pending | — |

After Stage 03 owner review and merge, the next task is **Stage 04: collision, score, Game Over, and restart**. Stage 03 allows the bird to pass through pipes or fall behind ground without ending the run. Close/relaunch to return to Ready until restart UI is implemented.
