# Architecture

## Current implementation

The solution contains four projects:

| Project | Target | Responsibility | Dependencies |
| --- | --- | --- | --- |
| `FlappyBird.App` | `net10.0-windows` | Window, input/focus adapter, timing, and rendering | Core, Windows Forms |
| `FlappyBird.Core` | `net10.0` | Geometry, motion, gaps, swept contact, scoring, three-state game/reset | .NET base libraries |
| `FlappyBird.Core.Tests` | `net10.0` | Core behavior tests | Core, xUnit, test SDK, coverage collector |
| `FlappyBird.App.Tests` | `net10.0-windows` | Rendering, input-edge, and timing tests | App, xUnit, test SDK, Windows drawing APIs |

Program initializes WinForms on an STA thread. GameForm owns window/input/focus, FixedStepLoop, Stopwatch, one timer and one restart Button. GameEngine owns Bird, PipeSystem, Score, and Ready/Running/GameOver. Renderer receives read-only presentation inputs; it owns no game rules. The existing fixed-window/DPI/double-buffering behavior remains.

Playfield remains 480 × 640 with an 80-unit ground region. GroundTop=560 is shared by geometry, collision, and rendering. Mandatory contact, score, loss, and restart behavior are now implemented; optional persistence/audio/progression are not.

## Bird, settings, and state

GameSettings validates finite positive gravity (900 units/s²), flap-speed magnitude (320 units/s), and nominal bird dimensions (86 × 48). Bird validates that its initial rectangle fits above ground. X/Y are top-left coordinates; initial X centers the bird and initial Y preserves the Stage 01 center at `GroundTop * 0.51`, clamped to the initial playable region.

Bird uses private setters and deliberate Flap/Update/Reset methods. Flap sets velocity to `-FlapSpeed`. Semi-implicit Euler computes `vNext = v + gravity * dt`, then `yNext = y + vNext * dt`. Zero time changes nothing; negative/nonfinite elapsed time and overflowing results are rejected before mutation. At or above the ceiling, Y becomes zero and any upward velocity is cleared; gravity resumes downward movement on later steps.

Bird.Predict computes an update without mutation so contact can be checked first; ordinary Bird.Update retains the standalone physics contract. On loss, the engine places the bird at contact and clears its vertical velocity. Independent Bird arithmetic may still go below ground; GameEngine is responsible for gameplay collision.

## Rule update order and contact

Engine Update validates finite elapsed time in [0, 1] before any state change, including Ready/GameOver. Zero is a no-op. Only Running advances. Larger calls are divided into at most 60 equal steps, each no longer than 1/60 second; the App already sends fixed 1/60 steps.

For each rule step: predict Bird endpoint; find earliest swept pipe/ground contact; if hit, move only to that fraction, enter GameOver, and stop. Otherwise commit bird/pipe movement, award eligible passes, then recycle departing pairs. A collision step earns no new point; points earned in earlier steps remain. No later steps run after loss.

The hitbox is Bird's nominal X/Y/Width/Height rectangle (86 × 48 by default), including conservative empty corners around the drawn body/tail/beak, excluding shadow. Pipe hitboxes use their full model rectangles, including lips. Exact edge/corner contact counts. Ground is checked at the bird's bottom; ceiling is still a nonfatal clamp.

CollisionDetector uses inclusive swept AABB slabs: expand the obstacle by bird dimensions, then intersect X/Y time intervals for relative motion. Pipe travel becomes positive relative X; Bird movement is the line between its current position and the semi-implicit, ceiling-clamped endpoint. The earliest fraction in [0, 1] is contact. This is a discrete motion contract with linear sweeps, not a claim of exact continuous parabolic integration. Ground crossing is solved along that same segment. Existing touch/overlap resolves immediately at fraction zero, without negative elapsed time.

## Score and clean-run reset

Score is controlled by GameEngine. A pair with HasScored=false earns one point only when its moved Right is strictly less than Bird.X in a collision-free step. Mark before recycling, then PipePair.Reset clears eligibility for a new cycle. No points are awarded at equality, after loss, or by rendering.

Engine.Reset obtains fresh pipe gaps/starting positions, resets Bird, sets Score=0, and returns Ready. Form restart invokes it only on GameOver, then clears the loop/clock, preserves a release requirement for held Space, hides the button, returns focus, and reuses the original timer. A separate fresh Space starts the new run. Enter repeats in Ready do nothing. State changes update accessible descriptions; loss stops the timer, and activation cannot resume a GameOver run.

## Obstacles, generation, and recycling

PipePair stores X, width, gap top/height and private-set HasScored. Upper geometry is `(X, 0, width, gapTop)`; lower is `(X, gapTop + gapHeight, width, GroundTop - gapTop - gapHeight)`. PipeRectangle remains a numeric immutable value type with no drawing dependency.

PipeSystem owns exactly two reusable pairs, exposed through a read-only collection in left-to-right order. Defaults are speed 140 units/s, width 64, gap 180, left-edge spacing 260, initial off-screen offset 40, symmetric gap margin 40, and bird-clearance tuning 12. Initial X is `playfield.Width + offset + index * spacing`, yielding 520 and 780. Empty space is spacing minus width (196), not 260.

GameSettings construction validates each scalar as finite/positive. ValidateFor checks bird fit, gap height greater than bird height plus twice clearance, gap plus two margins strictly below GroundTop, empty horizontal space greater than bird width plus twice clearance, and room for two full pairs in the viewport. It also rejects unrepresentable initial positions and speed above two spacings per second.

Gap bounds are `[GapMargin, GroundTop - GapMargin - PipeGapHeight)`, or [40, 340) by default. IGapSource supplies the gap top in those explicit bounds. RandomGapSource owns its Random instance (optionally seeded); deterministic substitutes drive tests. The exclusive endpoint is protected against floating-point rounding. PipeSystem rejects invalid provider results. Rendering receives only pair geometry and never accesses the source.

Update accepts [0, 1] seconds, moves both left by `speed * elapsed`, then recycles a pair only when Right < 0. Right == 0 is retained until the next movement. Each departing leftmost pair is placed after the current rightmost at one spacing and rotated to the end of the collection. Cyclic object identity changes are expected; spatial order remains left-to-right. The one-second/speed limits bound travel to two spacings and recycling to at most two operations per call. The UI uses 1/60-second updates, not these larger diagnostic steps.

At a maximum full-cycle update, the mathematically recycled X cannot be less than `-Width`. The recycled result is clamped to that lower bound to prevent fractional-spacing roundoff from creating a tiny negative Right and a spurious third recycle. This does not soften the strict departure test on the original moving pair.

Standalone PipeSystem.Update retains its pre-sampled, move-and-recycle contract for geometry diagnostics/tests. Engine uses internal movement and recycling phases separately so collision/scoring occur before repositioning. Invalid provider results fail fast; elapsed-input validation is transactional, but provider faults during engine recycling are not a general transaction guarantee. Production RandomGapSource supplies valid values. Reset samples both gaps before changing positions; a fresh sample may naturally repeat. Pause remains a Form concern and freezes the entire engine.

## Input, timing, and lifecycle

App handles KeyDown/KeyUp. FlapInput accepts one fresh press until release, suppressing OS key repeats. The first accepted Space clears Ready-time clock/accumulator, enters Running, and flaps; Space default processing is suppressed.

The UI-thread timer has a 15 ms scheduling interval. Stopwatch measures elapsed time; FixedStepLoop advances Core by 1/60 second at a time. Each tick adds at most 0.1 seconds, performs at most six updates, drops any whole overdue steps, and retains only a fractional remainder. The comparison uses a 1e-12-second rounding tolerance. Repaint is requested after updates/input, and never drives simulation.

Deactivation stops the timer and clears timing/input accumulation. Activation samples held Space once through GetAsyncKeyState and resets timing; a live run resumes, GameOver stays stopped. Loss/restart also preserve a held-Space release requirement. ProcessKeyPreview consumes child-button Space messages so Space cannot click New game; Enter is routed to the same guarded restart method through command/key handling. There is no background hook. Timer/click handlers are registered once in the constructor and the timer is disposed with the Form.

## Renderer and coordinate mapping

Coordinates use top-left origin, X rightward, Y downward. Ground is anchored at `GroundTop` (560). Artwork is centered in the model's bird rectangle and scaled from nominal 86 × 48 bounds. The shadow is decorative. Birds below the viewport are culled before converting coordinates to floats.

Renderer now also receives Score. SceneViewport computes `scale = min(clientWidth/logicalWidth, clientHeight/logicalHeight)` and centered offsets for both graphics and native button bounds. It maps the logical restart rectangle (centered, Y=390, 220 × 48) inside the Game Over panel. Graphics transforms/clipping remain unchanged; empty clients are ignored.

Renderer paints scenery/pipes/bird, live score and guidance, Ready panel if applicable, ground, and a dimmed Game Over/final-score card when finished. The native Button owns input and paints above the card. Lips remain within model bounds. Pixel-unit fonts scale with geometry; no paint advances rules or randomness. A 125% desktop yields a 600 × 800 client area.

Each render saves and restores the caller's graphics state, including transforms and clipping. Fonts, brushes, pens, paths, and string formats are locally owned with `using` disposal. The renderer retains only the immutable Playfield; it owns no persistent native drawing resources and therefore needs no disposal lifecycle. Painting reads dimensions and does not create or advance gameplay state.

The Windows-only App test project renders into off-screen bitmaps; it does not introduce Windows dependencies into Core or make the Core tests require a UI.

## Remaining work

| Type | Planned responsibility |
| --- | --- |
| Stage 05 | Usability/tuning and selected optional enhancements |
| Stage 06 | Final manual QA, screenshots, second-computer and submission checks |

The implemented flow is key event → input edge → Core engine, then timer signal → fixed updates → engine → repaint request. Future game rules must preserve this separation.

The state flow is Ready → Running → GameOver → Ready. Stage 04 is implemented but owner acceptance remains pending.

## Design constraints

- Keep the dependency direction toward Core. Never add a Core reference to App.
- Keep Windows-specific types, `System.Drawing`, keyboard events, and timers out of Core.
- Use private mutation and deliberate methods for gameplay objects; avoid exposing writable state merely to make tests convenient.
- Use simple composition and narrowly scoped classes. Add interfaces when there is an actual substitution need, such as controlled randomness or score storage.
- Keep timer updates on the WinForms UI thread. Stop/dispose timers with the Form lifecycle once they exist.
- Make restart reset all gameplay state and input latches. Future persistence must not block normal play if storage fails.

The project avoids external game frameworks and unnecessary abstractions so the student can explain its own GameEngine class, OOP relationships, and event flow.
