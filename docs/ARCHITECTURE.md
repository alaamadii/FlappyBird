# Architecture

## Current implementation

The solution contains four projects:

| Project | Target | Responsibility | Dependencies |
| --- | --- | --- | --- |
| `FlappyBird.App` | `net10.0-windows` | Windows entry point, window lifecycle, and static GDI+ renderer | Core, Windows Forms |
| `FlappyBird.Core` | `net10.0` | Domain data and validation | .NET base libraries |
| `FlappyBird.Core.Tests` | `net10.0` | Core behavior tests | Core, xUnit, test SDK, coverage collector |
| `FlappyBird.App.Tests` | `net10.0-windows` | Bitmap rendering and graphics-state regression tests | App, xUnit, test SDK, Windows drawing APIs |

`Program.Main` is marked `[STAThread]`, initializes WinForms application configuration, and starts `GameForm`. `GameForm` owns one immutable `Playfield` and a `GameRenderer` using that same object. It configures a fixed border, a 96-DPI autoscale baseline, double buffering, and repaint-on-resize. Its `OnPaint` delegates to the renderer using the current client size. The initial status Label has been removed; the Ready copy is painted with the scene and exposed through the Form's accessible description.

`Playfield` is immutable. It rejects nonpositive dimensions and ground heights that fill or exceed the total height. `GroundTop` is computed as `Height - GroundHeight`; keeping that boundary in Core lets future collision and rendering use the same coordinate contract. Defaults are 480 × 640 with an 80-unit ground region. A stationary bird is drawn in App; no Bird model, timer, obstacle, or scoring behavior exists yet.

## Renderer and coordinate mapping

Coordinates use a top-left origin, X increasing rightward, and Y increasing downward. The static composition targets the default 480 × 640 playfield. Ground geometry is anchored to `Playfield.GroundTop` (560), and the bird preview is centered at `(Width / 2, GroundTop * 0.51)`.

`GameRenderer.Render(Graphics, Size)` computes `scale = min(clientWidth / logicalWidth, clientHeight / logicalHeight)`. It centers the scaled playfield using half of the remaining width/height, clips to the logical rectangle, and paints a dark border in any unused client area. It never stretches X and Y independently. Zero-size client areas are ignored.

The renderer draws sky/clouds/hills, a heading, the stationary bird, the Ready panel, and ground. All visuals are original GDI+ primitives. It uses pixel-unit fonts so fonts follow the same transform as geometry; it does not multiply by DPI a second time. WinForms handles DPI-related window sizing from the 96-DPI baseline. A 125% desktop was observed to produce a 600 × 800 client area.

Each render saves and restores the caller's graphics state, including transforms and clipping. Fonts, brushes, pens, paths, and string formats are locally owned with `using` disposal. The renderer retains only the immutable Playfield; it owns no persistent native drawing resources and therefore needs no disposal lifecycle. Painting reads dimensions and does not create or advance gameplay state.

The Windows-only App test project renders into off-screen bitmaps; it does not introduce Windows dependencies into Core or make the Core tests require a UI.

## Intended gameplay design — not yet implemented

| Type | Planned responsibility |
| --- | --- |
| `Bird` | Position, vertical velocity, gravity integration, flap action |
| `PipePair` | Upper/lower obstacle geometry, gap, horizontal movement, score eligibility |
| `GameEngine` | Update entities, recycle pipes, detect collisions, calculate score, manage state and reset |
| `GameForm` | Keyboard events, timer lifecycle, rendering requests, and presentation |
| Renderer | Draw a snapshot of Core state using GDI+; dispose drawing resources |

The intended update flow is keyboard event → input command → Core engine, then timer tick → engine update → repaint request. Painting reads state without advancing it. Core receives elapsed time explicitly instead of consulting WinForms timers, allowing deterministic tests.

The planned state flow is Ready → Running → GameOver, with restart returning to a clean initial state. The precise input, timing, collision-boundary, and score-ordering contracts will be decided and tested in the relevant stages rather than implied by this document.

## Design constraints

- Keep the dependency direction toward Core. Never add a Core reference to App.
- Keep Windows-specific types, `System.Drawing`, keyboard events, and timers out of Core.
- Use private mutation and deliberate methods for gameplay objects; avoid exposing writable state merely to make tests convenient.
- Use simple composition and narrowly scoped classes. Add interfaces when there is an actual substitution need, such as controlled randomness or score storage.
- Keep timer updates on the WinForms UI thread. Stop/dispose timers with the Form lifecycle once they exist.
- Make restart reset all gameplay state and input latches. Future persistence must not block normal play if storage fails.

The project intentionally avoids a game engine and a large framework architecture so that the student can explain the actual OOP relationships and event flow.
