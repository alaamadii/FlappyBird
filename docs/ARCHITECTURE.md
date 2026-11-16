# Architecture

## Current implementation

The solution contains three projects:

| Project | Target | Responsibility | Dependencies |
| --- | --- | --- | --- |
| `FlappyBird.App` | `net10.0-windows` | Windows entry point and application shell | Core, Windows Forms |
| `FlappyBird.Core` | `net10.0` | Domain data and validation | .NET base libraries |
| `FlappyBird.Core.Tests` | `net10.0` | Core behavior tests | Core, xUnit, test SDK, coverage collector |

`Program.Main` is marked `[STAThread]`, initializes WinForms application configuration, and starts `GameForm`. `GameForm` creates a `Playfield`, uses its logical dimensions for the client area, configures a fixed border and double buffering, and displays a foundation status label. Controls are created in C# and owned/disposed by the Form.

`Playfield` is immutable. It rejects nonpositive dimensions and ground heights that fill or exceed the total height. `GroundTop` is computed as `Height - GroundHeight`; keeping that boundary in Core lets future collision and rendering use the same coordinate contract. Defaults are 480 × 640 with an 80-unit ground region. No bird, timer, obstacle, or scoring behavior exists yet.

Coordinates are logical playfield units. The intended convention is top-left origin, X increasing rightward, and Y increasing downward. Later rendering must account for Windows DPI scaling when mapping those coordinates to the displayed client area.

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
