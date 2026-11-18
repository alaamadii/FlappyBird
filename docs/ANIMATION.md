# Retro artwork and animation

The owner requested an Atari-like arcade appearance and improved animation. The implementation uses original pixel-art shapes and a small fixed palette rather than copied game assets. The existing smaller-hop/capped-fall physics remain unchanged by this presentation work.

## Ownership and assets

| Component | Responsibility |
| --- | --- |
| [SceneAnimation](../src/FlappyBird.App/Animation/SceneAnimation.cs) | Advance App-only presentation time and expose an immutable SceneAnimationFrame |
| [GameForm](../src/FlappyBird.App/Forms/GameForm.cs) | Drive presentation from the existing bounded loop; apply fresh-key, focus, loss and reset events |
| [GameRenderer](../src/FlappyBird.App/Rendering/GameRenderer.cs) | Read the snapshot and model, draw original block artwork, and restore caller graphics state |
| [PixelFont](../src/FlappyBird.App/Rendering/PixelFont.cs) | Draw original five-by-seven glyphs for large headings/numbers; no font file |

Bird poses, pipe highlights, stars, moon, stepped landscape, ground tiles, panels and heading glyphs are authored directly in C#. Small instructions/native controls use the installed Windows monospace font; no font files are redistributed. No images, video, GIF, downloaded sprite sheet, paid service or network connection is required at runtime.

The connected Higgsfield generation attempt did not produce an asset. No Higgsfield-generated media is included or claimed as provenance. The delivered artwork is code-authored; the screenshots capture that actual application.

## Timing and state behavior

- Wings follow **up → middle → down → middle**, 100 ms per pose, from a bounded 0.4-second phase.
- Ready uses a 3-unit bob over 1.8 seconds and wings while Core remains stationary.
- A fresh accepted Space press restarts the up-wing pose and clears idle bob. Held-key repeat cannot retrigger it.
- Running tilt reads vertical velocity and is limited to −8° through +12°. The artwork is inset inside the unchanged nominal bird rectangle; tilt is a visual effect, not a hitbox or velocity change.
- Ground scroll uses the shared GameSettings.PipeSpeed (140 by default), wrapping every 24 units. Backdrop phase runs at 10 units/s with a 480-unit loop and a slower landscape layer. Cloud copies and repeated terrain periods keep the wrap seamless.
- The same fixed 1/60-second callback updates gameplay and presentation. The existing 100 ms/six-update catch-up limit still bounds work. Ready runs visual updates only.
- Deactivation stops the timer and clears timing debt, preserving the exact presentation frame. Activation resumes without advancing through inactive wall time.
- Game Over freezes presentation. New game resets it to the same initial snapshot while retaining best score.

OnPaint never reads a clock or advances animation. Repainting a supplied snapshot gives identical output, including after different frames have been rendered. The renderer uses local disposable graphics resources and saves/restores transform, clipping and drawing settings. No frame bitmaps or external media are allocated/loaded during painting.

## Scaling, evidence and limits

The existing 480×640 logical coordinate system and centered uniform SceneViewport are retained. Block edges use non-antialiased GDI+ drawing; integer glyph cells fit available heading bounds. This is a retro style, not an emulation of a particular console's hardware. Native button bounds/font retain the same DPI mapping and accessibility behavior.

Tests cover idle/flight phases, wrap, custom scroll speed, fresh flap, invalid input, GameOver/reset and native focus freezing. Renderer tests compare explicit wing/tilt/scroll snapshots at 100%/125%/150% equivalents, require exact repeated output and verify unchanged Core state. Existing native-window isolation remains in place.

[Actual window screenshots](screenshots/README.md) show Ready, a valid scored run and Game Over using isolated score storage. Direct renderer previews are diagnostic images only. Physical monitor transitions, perceived smoothness, owner visual acceptance and other manual submission checks remain separate in [SUBMISSION.md](SUBMISSION.md).
