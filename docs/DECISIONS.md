# Decision log

Record the date, context, decision, and consequences when changing an important design choice. Entries are chronological; later stages supersede earlier temporary limitations. Stage 04 implements mandatory game rules, with owner acceptance still pending.

## ADR-001 — Windows Forms on .NET 10

**Date:** 2026-11-16. **Status:** Accepted.

The assignment calls for C# desktop work with Windows Forms. Use `net10.0-windows` for App, `net10.0` for Core/Tests, and SDK 10.0.402 with `latestPatch` roll-forward. The installed Windows SDK supports this setup. It constrains the UI to Windows and requires a supported SDK feature band. A framework change must be explicit and documented.

## ADR-002 — Independent domain library

**Date:** 2026-11-16. **Status:** Accepted.

Keep gameplay/domain code in Core and UI code in App. This enables headless unit tests and makes responsibilities explainable during assessment. The initial shared contract is immutable `Playfield` dimensions. Separate projects add some structure but prevent the game from becoming one large Form class.

## ADR-003 — Programmatic UI and later custom drawing

**Date:** 2026-11-16. **Status:** Accepted.

Use programmatically constructed WinForms UI for the VS Code workflow. Replace the generated `Form1`/Designer shell with `Forms/GameForm.cs`. The initialization window uses a label; custom GDI+ game rendering is reserved for stage 01. This removes Designer dependence while requiring deliberate layout and resource management later.

## ADR-004 — Local environment normalization

**Date:** 2026-11-16. **Status:** Accepted.

Inherited .NET variables point to a different tool location from the installed executable. Development commands normalize `DOTNET_ROOT` from the resolved executable and put CLI state in ignored `.local/dotnet`, restoring previous values afterward. This avoids changing machine/user settings and makes commands reproducible. Direct commands and external debugger sessions still use their own environment.

## ADR-005 — Real tests and one validation command

**Date:** 2026-11-16. **Status:** Accepted.

Remove the empty template test and verify actual Playfield validation and boundary behavior with xUnit. Use `scripts/dev.ps1 check` locally and in Windows CI for restore/build/test/format checks. Treat build warnings as errors. This establishes a meaningful baseline but does not validate future gameplay or visual quality.

## ADR-006 — Reviewed development stages

**Date:** 2026-11-16. **Status:** Accepted.

Use the seven-stage branch/PR plan from the brief. Finish required behavior before optional polish. Log real changes and results, and leave merges to the owner. Avoid manufactured history or claiming remote checks from local evidence. No remote resources need to be changed merely to complete local initialization.

## ADR-007 — Uniform logical rendering and DPI ownership

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 01 PR #2.

Keep Playfield dimensions as the logical coordinate system. GameForm delegates paint events to an App-only GameRenderer, passing its actual client size. The renderer scales uniformly, centers the scene, and uses borders when aspect ratios differ. WinForms autoscaling starts at 96 DPI; fonts use pixel units and the same transform as shapes, avoiding duplicate DPI scaling. The window remains fixed with maximize disabled.

Use original GDI+ primitives for the scenery and bird preview. No gameplay Bird model or external assets are needed in this stage. Save/restore graphics state and dispose drawing resources per paint; the renderer holds no disposable native resources between calls. This favors explicit ownership and straightforward repaint behavior over premature caching. The composition is designed for the default 480 × 640 playfield; arbitrary game-layout customization remains future work.

## ADR-008 — Windows renderer regression tests

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 01 PR #2.

Add a separate `net10.0-windows` App.Tests project rather than coupling Core tests to System.Drawing. Bitmap tests cover common scale factors, centered borders, ground/bird placement, graphics-state restoration, empty client areas, and deterministic repaint. Keep visual review separate: these assertions do not prove live cover/uncover behavior, smoothness, or per-monitor DPI transitions. The existing Windows CI runs both suites through the solution without changing the Core target.

## ADR-009 — Domain bird motion and initial state

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 02 PR #3.

Bird owns top-left position and vertical velocity, with private mutation through Flap/Update/Reset. GameSettings centralizes finite positive dimensions and tuning: gravity 900 logical units/s², flap-speed magnitude 320 units/s, artwork bounds 86 × 48. Bird must fit above ground initially. The small GameEngine coordinates only Ready/Running and the existing bird; it adds no placeholder obstacles or loss state.

Use semi-implicit Euler: `v += gravity * dt`, then `y += v * dt`. Flap sets `v = -flapSpeed` rather than adding impulse. Reject negative/nonfinite time and overflow before mutation; zero time is a no-op. Clamp Y to zero on ceiling contact and clear upward velocity so gravity can resume. Reset methods are tested domain primitives only; complete restart remains Stage 04.

Ground is deliberately not a collider yet: the bird can descend behind the ground and off-screen while Running continues. The renderer reads the real model without updating it and safely skips a below-viewport bird. Close/relaunch is the temporary way to return to Ready; there is no hidden restart behavior.

## ADR-010 — Fixed updates, bounded catch-up, and focus-aware press edges

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 02 PR #3.

Keep the scheduler in App. A 15 ms WinForms timer measures elapsed seconds through Stopwatch and feeds FixedStepLoop. Updates are fixed at 1/60 second, with at most 0.1 seconds admitted and six updates per tick. Discard excess real time and any remaining whole overdue steps; retain a fractional remainder. A 1e-12-second comparison tolerance handles floating-point step boundaries. This trades wall-clock catch-up for bounded UI work after stalls. Core receives explicit elapsed time and knows nothing about Windows timers.

FlapInput accepts a fresh Space edge, blocks repeated KeyDown until KeyUp, and handles activation separately from domain actions. Focus loss stops the timer and clears time/input accumulation. Focus return resets elapsed tracking and reads Space's held state once with GetAsyncKeyState in App: held Space is blocked until release, otherwise the next fresh press works. No global hook or background key processing is introduced. The first start press also clears Ready-time accumulation. Timer and Stopwatch are stopped, and Timer disposed, with the Form.

Automated helper tests cover the input/timing contracts; window-message probes check adapter wiring separately. Simulating the activation-time held-key result does not replace a human Alt-Tab/physical-key check, which remains explicitly pending.

## ADR-011 — Two reusable pairs and bounded ordered recycling

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 03 PR #4.

Keep numeric PipePair/PipeRectangle geometry in Core and manage two pairs in PipeSystem. The engine updates bird and pipes together only while Running. Expose a read-only collection, reuse the same pair objects, and rotate the departing leftmost pair behind the furthest remaining pair. Recycle only at Right < 0; touching X=0 alone does not recycle. Do not add scoring flags or collision rules before their stage.

Use left-edge spacing, not empty-space spacing. Defaults: width 64, spacing 260, speed 140 units/s, gap height 180, off-screen start offset 40, symmetric gap margin 40, and bird clearance 12. Initial left edges 520/780 allow about 1.69 seconds before the first pair reaches the bird's initial right edge; two pairs can be fully visible together after 2.6 seconds. Validate geometry against Playfield, including vertical/horizontal clearance and room for both pairs.

Support Core obstacle steps up to one second and require speed <= two spacings per second. Therefore a supported update crosses at most one two-pair cycle and requires at most two recycling operations, including when both pairs expire together. Reject larger Running engine updates before motion rather than silently skipping generations. The unchanged UI fixed step is 1/60 second; larger steps are diagnostic support, not a future collision-accuracy guarantee.

**Follow-up 2026-11-17:** Clamp recycled X to at least `-Width`, its mathematical full-cycle lower bound. A fractional-spacing regression showed roundoff could otherwise cause a third recycle at the exact boundary. Keep strict Right < 0 for departure; do not add a broad epsilon that delays genuine off-screen recycling.

## ADR-012 — Gap-source substitution and read-only rendering

**Date:** 2026-11-16. **Status:** Accepted through merged Stage 03 PR #4.

IGapSource requests an explicit inclusive-minimum/exclusive-maximum interval: [40, 340) with defaults. RandomGapSource owns a Random instance, optionally seeded, instead of using global shared randomness. Protect the exclusive endpoint from rounding and reject provider values outside the interval. Generate only during initialization, recycling, and domain reset. Sample required recycle/reset gaps before changing pipe positions so a faulty supplied source cannot partially reposition pipes.

Renderer reads Upper/Lower rectangles and draws original green GDI+ bodies/lips within those bounds. It does not consume randomness, move pairs, or advance the engine. Draw bird after pipes, and keep a compact Running instruction banner over scenery. Stage 03 explicitly allows passing through pipes and falling behind ground; collision, score, loss, and restart UI remain Stage 04.

## ADR-013 — Inclusive swept contact and collision-first rules

**Date:** 2026-11-17. **Status:** Accepted through merged Stage 04 PR #5.

Use Bird's nominal rectangular hitbox and both complete pipe rectangles, with edge/corner touching counted as collision. The bird rectangle conservatively includes empty artwork corners, but not the decorative shadow. Ground contact is the bird bottom at GroundTop; ceiling remains nonfatal.

GameEngine validates [0, 1] seconds in all states before mutation. Running calls are divided into at most 60 steps no longer than 1/60 second. Bird.Predict supplies the semi-implicit, ceiling-clamped endpoint without mutation. Swept AABB slabs test relative horizontal pipe travel and the linear bird segment, finding contact even through thin geometry. This combines bounded work with protection against tunneling rather than relying on endpoint overlap alone.

At earliest contact, advance only the contact fraction, clamp ground presentation, clear vertical velocity, enter GameOver, and stop. Existing ground overlap resolves at time zero. A collision step earns no point and does not recycle. Earlier valid steps' points survive later loss. Tests cover exact boundaries, diagonal time intervals, existing ground overlap, and a tiny bird/pipe crossing between disjoint endpoints.

## ADR-014 — Per-pair score lifecycle and complete UI restart

**Date:** 2026-11-17. **Status:** Accepted through merged Stage 04 PR #5.

After a collision-free movement step, award once when pair.Right < Bird.X and HasScored is false; equality does not score. Mark before recycling, then Reset clears eligibility for the new cycle. Separate PipeSystem's internal movement and recycling phases to preserve this ordering while keeping its standalone diagnostic API. Score and game-state mutation stay in Core.

On GameOver, stop the Form timer, clear time debt, show final score and one native New game button. Enter and click share a guarded restart method returning Ready, not directly Running. Reset domain score/motion/pipes/gaps/eligibility and App input/clock/accumulator. Preserve a release requirement if Space is held. Intercept Space preview from the button so native default behavior cannot restart or flap accidentally. Reuse timer, button, handlers, and pairs across runs.

Extract SceneViewport so painted content and native button use the same uniform, centered logical mapping. Accessible copy follows state; activation never resumes GameOver. Keep optional persistence/audio/progression outside this stage. Invisible STA window-message tests validate repeated restart, focused-button keys, focus return, and timing reset separately from pending human checks.

## ADR-015 — App-owned best score and safe local replacement

**Date:** 2026-11-17. **Status:** Accepted through owner-approved Stage 05 PR #6 merge.

Choose persistent high score as the only optional feature in this stage. Core continues to know only current-run Score. App uses IHighScoreStore to substitute safe test storage, FileHighScoreStore for the production LocalApplicationData path, and HighScoreService for cached best/status and error handling. This keeps file behavior testable without a database, serializer, dependency injection framework, or Windows paths in Core.

Use `%LOCALAPPDATA%\YMH219-FlappyBird\high-score.txt`, a bounded UTF-8 invariant integer, rather than checkout/build-relative data. Missing is normal zero. Reject corrupt/negative/out-of-range data; expected storage/access/security errors show brief nonmodal copy. A failed save retains the session best. The Form records once per GameOver transition; successful equal/lower runs do not write, and a failed best retries only when the next run finishes. Restart keeps the service instance.

Write/flush a unique temporary file in the same directory, close it, then replace the destination, preserving valid previous data if writing/replacement fails. Re-read the existing valid maximum before saving, but do not promise simultaneous-instance synchronization. Keep the tiny local I/O synchronous at startup/completion; it never occurs in paint or ordinary simulation updates. File tests use owned temporary directories; all Form tests and score-producing probes inject storage. See [HIGH_SCORE.md](HIGH_SCORE.md).

## ADR-016 — Player copy, focus feedback, and consistent native scaling

**Date:** 2026-11-17. **Status:** Accepted through owner-approved Stage 05 PR #6 merge.

Use distinct labeled Score and Best cards in opposite top corners, leaving the bird's central ceiling path open. Put control reminders in the ground region and remove development-stage copy. Keep Ready, Game Over, and inactive Paused cards consistent in palette and spacing. Paused is an App presentation flag, not a new Core state; activation retains the existing immediate resume and held-key release policy.

Keep one native New game button for keyboard/mouse/accessibility support, with bold text, hover/press colors, and a gold focus border. Its owned Font uses pixel units at `16 * SceneViewport.Scale`, replacing/disposal only on size change. Recompute native bounds/font after client-size changes as well as layout: the new resize regression exposed that a fixed Form can finish changing ClientSize after the layout callback. This fixes stale control placement without changing fixed-window behavior.

Leave gravity 900, flap 320, pipe speed 140, gap 180, spacing 260 and all collision/timing/input policies unchanged. Scripted checks establish readability and rule stability, not human difficulty/fairness; no human playtesting evidence supports retuning. Defer sound, progressive speed, selectable difficulty, and final submission work. Physical input/DPI transitions and owner acceptance remain pending.

## ADR-017 — Final evidence and owner-controlled submission

**Date:** 2026-11-17. **Status:** Evidence prepared; final owner acceptance pending.

Freeze application scope. Use a fresh remote clone, full check, isolated native probes and actual GameForm captures as reproducible evidence. Capture the target window with PrintWindow and disclose controlled gaps/direct stepping; this does not establish human playtesting. Commit only inspected game images, never desktop/background captures or personal score data.

The owner approved merging PR #6 and making the repository public. Applied both, reviewed tracked content/history, and verified anonymous access/profile listing. Final PR merging still requires approval; public visibility does not authorize Classroom submission or inventing identity/manual results. Keep missing student details and physical/second-computer checks open in SUBMISSION. No application code, dependencies, tuning or optional features change in Stage 06.

## ADR-018 — Owner-selected public identity fields

**Date:** 2026-11-18. **Status:** Owner-directed documentation choice.

Omit the student number from the public README as explicitly requested by the owner. Remove its placeholder and requests to supply it for publication; keep the still-unknown full name pending. No actual student number has been supplied or published by this work.

Preserve the instructor's stated identity requirement in REQUIREMENTS and identify the omission as an owner-directed exception. Do not claim the requirement was fulfilled or waived. This changes public documentation only; gameplay, tests and manual acceptance results are unaffected.
