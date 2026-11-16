# Architecture

## Current implementation

The solution contains four projects:

| Project | Target | Responsibility | Dependencies |
| --- | --- | --- | --- |
| `FlappyBird.App` | `net10.0-windows` | Window, input/focus adapter, timing, and rendering | Core, Windows Forms |
| `FlappyBird.Core` | `net10.0` | Bird motion, settings, Ready/Running state, validation | .NET base libraries |
| `FlappyBird.Core.Tests` | `net10.0` | Core behavior tests | Core, xUnit, test SDK, coverage collector |
| `FlappyBird.App.Tests` | `net10.0-windows` | Rendering, input-edge, and timing tests | App, xUnit, test SDK, Windows drawing APIs |

`Program.Main` initializes WinForms on an STA thread and starts GameForm. The Form constructs a shared Playfield, GameEngine with GameSettings, and GameRenderer. Engine owns Bird and Ready/Running state. Form owns FlapInput, FixedStepLoop, Stopwatch, and the WinForms timer. The fixed border, 96-DPI baseline, double buffering, and repaint-on-resize remain. OnPaint passes current Bird/state to the renderer without updating motion.

Playfield is immutable with defaults 480 × 640 and ground height 80. `GroundTop = Height - GroundHeight` is shared with rendering and future collision. Ground is currently decorative; there are no pipes, score, Game Over, or restart controls.

## Bird, settings, and state

GameSettings validates finite positive gravity (900 units/s²), flap-speed magnitude (320 units/s), and nominal bird dimensions (86 × 48). Bird validates that its initial rectangle fits above ground. X/Y are top-left coordinates; initial X centers the bird and initial Y preserves the Stage 01 center at `GroundTop * 0.51`, clamped to the initial playable region.

Bird uses private setters and deliberate Flap/Update/Reset methods. Flap sets velocity to `-FlapSpeed`. Semi-implicit Euler computes `vNext = v + gravity * dt`, then `yNext = y + vNext * dt`. Zero time changes nothing; negative/nonfinite elapsed time and overflowing results are rejected before mutation. At or above the ceiling, Y becomes zero and any upward velocity is cleared; gravity resumes downward movement on later steps.

GameEngine ignores valid updates while Ready; Flap enters Running and flaps, then Running advances Bird. Reset primitives restore initial motion and Ready but have no UI/input binding yet. There is no ground stop: the bird can descend behind ground and off-screen while Running continues.

## Input, timing, and lifecycle

App handles KeyDown/KeyUp. FlapInput accepts one fresh press until release, suppressing OS key repeats. The first accepted Space clears Ready-time clock/accumulator, enters Running, and flaps; Space default processing is suppressed.

The UI-thread timer has a 15 ms scheduling interval. Stopwatch measures elapsed time; FixedStepLoop advances Core by 1/60 second at a time. Each tick adds at most 0.1 seconds, performs at most six updates, drops any whole overdue steps, and retains only a fractional remainder. The comparison uses a 1e-12-second rounding tolerance. Repaint is requested after updates/input, and never drives simulation.

Deactivation stops the timer, resets clock/accumulator, and suspends input. Activation resets timing and uses one Windows `GetAsyncKeyState` call to check whether Space is already held. Held Space must be released before another flap. There is no hook or background key processing. Bird state is preserved while inactive, and inactive time is not replayed. Form disposal stops/disposes the timer and stops Stopwatch.

## Renderer and coordinate mapping

Coordinates use top-left origin, X rightward, Y downward. Ground is anchored at `GroundTop` (560). Artwork is centered in the model's bird rectangle and scaled from nominal 86 × 48 bounds. The shadow is decorative. Birds below the viewport are culled before converting coordinates to floats.

`GameRenderer.Render(Graphics, Size, Bird, GameState)` computes `scale = min(clientWidth / logicalWidth, clientHeight / logicalHeight)`. It centers the scaled playfield, clips to the logical rectangle, and fills unused client area with a dark border. Scene X/Y are never stretched independently. Empty client areas are ignored.

The renderer draws scenery, heading, current bird, a panel only while Ready, and ground. Pixel-unit fonts follow the scene transform without duplicate DPI multiplication. All visuals are original GDI+ primitives. WinForms scales the window from the 96-DPI baseline; a 125% desktop produced a 600 × 800 client area.

Each render saves and restores the caller's graphics state, including transforms and clipping. Fonts, brushes, pens, paths, and string formats are locally owned with `using` disposal. The renderer retains only the immutable Playfield; it owns no persistent native drawing resources and therefore needs no disposal lifecycle. Painting reads dimensions and does not create or advance gameplay state.

The Windows-only App test project renders into off-screen bitmaps; it does not introduce Windows dependencies into Core or make the Core tests require a UI.

## Remaining gameplay design — not yet implemented

| Type | Planned responsibility |
| --- | --- |
| `PipePair` | Upper/lower obstacle geometry, gap, horizontal movement, score eligibility |
| `GameEngine` extensions | Coordinate obstacles, collision, score, Game Over, and complete restart |

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
