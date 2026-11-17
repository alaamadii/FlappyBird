# Architecture

## Current implementation

The solution contains four projects:

| Project | Target | Responsibility | Dependencies |
| --- | --- | --- | --- |
| `FlappyBird.App` | `net10.0-windows` | Window, input/focus adapter, timing, and rendering | Core, Windows Forms |
| `FlappyBird.Core` | `net10.0` | Bird/pipe geometry, motion, gap generation, settings, Ready/Running state | .NET base libraries |
| `FlappyBird.Core.Tests` | `net10.0` | Core behavior tests | Core, xUnit, test SDK, coverage collector |
| `FlappyBird.App.Tests` | `net10.0-windows` | Rendering, input-edge, and timing tests | App, xUnit, test SDK, Windows drawing APIs |

`Program.Main` initializes WinForms on an STA thread and starts GameForm. The Form constructs shared Playfield, GameEngine with GameSettings, and GameRenderer. Engine owns Bird, PipeSystem, and Ready/Running state. Form owns FlapInput, FixedStepLoop, Stopwatch, and the WinForms timer. The fixed border, 96-DPI baseline, double buffering, and repaint-on-resize remain. OnPaint passes Bird/state/pipe pairs to the renderer without updating them.

Playfield is immutable with defaults 480 × 640 and ground height 80. `GroundTop = Height - GroundHeight` is shared with pipe geometry, rendering, and future collision. Pipe movement exists; ground/pipe contact, score, Game Over, and restart controls do not.

## Bird, settings, and state

GameSettings validates finite positive gravity (900 units/s²), flap-speed magnitude (320 units/s), and nominal bird dimensions (86 × 48). Bird validates that its initial rectangle fits above ground. X/Y are top-left coordinates; initial X centers the bird and initial Y preserves the Stage 01 center at `GroundTop * 0.51`, clamped to the initial playable region.

Bird uses private setters and deliberate Flap/Update/Reset methods. Flap sets velocity to `-FlapSpeed`. Semi-implicit Euler computes `vNext = v + gravity * dt`, then `yNext = y + vNext * dt`. Zero time changes nothing; negative/nonfinite elapsed time and overflowing results are rejected before mutation. At or above the ceiling, Y becomes zero and any upward velocity is cleared; gravity resumes downward movement on later steps.

GameEngine ignores valid updates while Ready; Flap enters Running and flaps, then Running advances Bird and pipes with the same elapsed time. Running rejects unsupported pipe time steps before moving Bird. Reset refreshes pipes and restores bird/Ready, without a UI/input binding. Ground/pipe contact does not end flight.

## Obstacles, generation, and recycling

PipePair stores shared X, width, gap top, and gap height behind movement/reset methods. Upper geometry is `(X, 0, width, gapTop)`; lower geometry is `(X, gapTop + gapHeight, width, GroundTop - gapTop - gapHeight)`. PipeRectangle is a small immutable numeric value type, with Right/Bottom derived properties and no System.Drawing dependency. There are no score flags.

PipeSystem owns exactly two reusable pairs, exposed through a read-only collection in left-to-right order. Defaults are speed 140 units/s, width 64, gap 180, left-edge spacing 260, initial off-screen offset 40, symmetric gap margin 40, and bird-clearance tuning 12. Initial X is `playfield.Width + offset + index * spacing`, yielding 520 and 780. Empty space is spacing minus width (196), not 260.

GameSettings construction validates each scalar as finite/positive. ValidateFor checks bird fit, gap height greater than bird height plus twice clearance, gap plus two margins strictly below GroundTop, empty horizontal space greater than bird width plus twice clearance, and room for two full pairs in the viewport. It also rejects unrepresentable initial positions and speed above two spacings per second.

Gap bounds are `[GapMargin, GroundTop - GapMargin - PipeGapHeight)`, or [40, 340) by default. IGapSource supplies the gap top in those explicit bounds. RandomGapSource owns its Random instance (optionally seeded); deterministic substitutes drive tests. The exclusive endpoint is protected against floating-point rounding. PipeSystem rejects invalid provider results. Rendering receives only pair geometry and never accesses the source.

Update accepts [0, 1] seconds, moves both left by `speed * elapsed`, then recycles a pair only when Right < 0. Right == 0 is retained until the next movement. Each departing leftmost pair is placed after the current rightmost at one spacing and rotated to the end of the collection. Cyclic object identity changes are expected; spatial order remains left-to-right. The one-second/speed limits bound travel to two spacings and recycling to at most two operations per call. The UI uses 1/60-second updates, not these larger diagnostic steps.

At a maximum full-cycle update, the mathematically recycled X cannot be less than `-Width`. The recycled result is clamped to that lower bound to prevent fractional-spacing roundoff from creating a tiny negative Right and a spurious third recycle. This does not soften the strict departure test on the original moving pair.

Gap samples needed by recycling are checked before pipe movement; an invalid source cannot leave partially moved pipe positions. Reset obtains two fresh samples before restoring initial positions, reusing existing pairs. A fresh sample may naturally equal an earlier value. No pause logic is duplicated in PipeSystem: the existing Form stops engine updates while inactive, freezing bird and pipes together.

## Input, timing, and lifecycle

App handles KeyDown/KeyUp. FlapInput accepts one fresh press until release, suppressing OS key repeats. The first accepted Space clears Ready-time clock/accumulator, enters Running, and flaps; Space default processing is suppressed.

The UI-thread timer has a 15 ms scheduling interval. Stopwatch measures elapsed time; FixedStepLoop advances Core by 1/60 second at a time. Each tick adds at most 0.1 seconds, performs at most six updates, drops any whole overdue steps, and retains only a fractional remainder. The comparison uses a 1e-12-second rounding tolerance. Repaint is requested after updates/input, and never drives simulation.

Deactivation stops the timer, resets clock/accumulator, and suspends input. Activation resets timing and uses one Windows `GetAsyncKeyState` call to check whether Space is already held. Held Space must be released before another flap. There is no hook or background key processing. Bird state is preserved while inactive, and inactive time is not replayed. Form disposal stops/disposes the timer and stops Stopwatch.

## Renderer and coordinate mapping

Coordinates use top-left origin, X rightward, Y downward. Ground is anchored at `GroundTop` (560). Artwork is centered in the model's bird rectangle and scaled from nominal 86 × 48 bounds. The shadow is decorative. Birds below the viewport are culled before converting coordinates to floats.

`GameRenderer.Render(Graphics, Size, Bird, GameState, IReadOnlyList<PipePair>)` computes `scale = min(clientWidth / logicalWidth, clientHeight / logicalHeight)`. It centers the scaled playfield, clips to the logical rectangle, and fills unused client area with a dark border. Scene X/Y are never stretched independently. Empty client areas are ignored.

The renderer paints scenery, visible model-derived pipes, bird, heading/banner, Ready panel if applicable, then ground. Pipe lips remain inside the outer model rectangles and do not shrink the opening. Off-screen pipe pairs are skipped; drawing never generates gaps or changes models. A compact Running banner keeps instructions readable over pipes. Pixel-unit fonts follow the scene transform without duplicate DPI multiplication. All visuals are original GDI+ shapes; a 125% desktop produced a 600 × 800 client area.

Each render saves and restores the caller's graphics state, including transforms and clipping. Fonts, brushes, pens, paths, and string formats are locally owned with `using` disposal. The renderer retains only the immutable Playfield; it owns no persistent native drawing resources and therefore needs no disposal lifecycle. Painting reads dimensions and does not create or advance gameplay state.

The Windows-only App test project renders into off-screen bitmaps; it does not introduce Windows dependencies into Core or make the Core tests require a UI.

## Remaining gameplay design — not yet implemented

| Type | Planned responsibility |
| --- | --- |
| `PipePair` extension | One-time score eligibility when scoring is implemented |
| `GameEngine` extensions | Collision, score, Game Over, and complete restart |

The implemented flow is key event → input edge → Core engine, then timer signal → fixed updates → engine → repaint request. Future game rules must preserve this separation.

Current states are Ready and Running. GameOver, complete restart, collision, and score ordering remain Stage 04.

## Design constraints

- Keep the dependency direction toward Core. Never add a Core reference to App.
- Keep Windows-specific types, `System.Drawing`, keyboard events, and timers out of Core.
- Use private mutation and deliberate methods for gameplay objects; avoid exposing writable state merely to make tests convenient.
- Use simple composition and narrowly scoped classes. Add interfaces when there is an actual substitution need, such as controlled randomness or score storage.
- Keep timer updates on the WinForms UI thread. Stop/dispose timers with the Form lifecycle once they exist.
- Make restart reset all gameplay state and input latches. Future persistence must not block normal play if storage fails.

The project intentionally avoids a game engine and a large framework architecture so that the student can explain the actual OOP relationships and event flow.
