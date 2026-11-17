# Application screenshots

These PNG files capture the actual GameForm window through the Windows `PrintWindow` API. They are not mockups, generated artwork, or composites. The images were inspected before inclusion and contain only the application window.

| Item | Evidence |
| --- | --- |
| Capture date | 2026-11-17 |
| Application source | 50431896fee7b618a70094083ae62146f7880ebb, merged through PR #6 into main at 97416ab |
| Display | 120 DPI (125%); 600×800 client area, 618×847 full window |
| Gameplay | [gameplay.png](gameplay.png): Running, score 1, best 0 |
| Loss | [game-over.png](game-over.png): natural ground contact, final score 1, best 1, focused New game button |
| Storage | Explicit unique path in ignored `.local/final-evidence`; owner's real score is untouched |

The temporary capture helper creates the real Form and uses its Space key message handling. It sets the two initial gap tops to 190 for a reproducible safe path, then steps the real engine at 1/60 second while issuing scripted flaps. A complete pass earns the point through ordinary engine scoring. It captures Running, stops issuing flaps, lets the bird fall naturally to ground contact, then captures Game Over and checks restart.

The helper pauses the production timer during directly stepped simulation to avoid double updates. The images therefore establish actual UI rendering and a controlled rules path, not a human random-gap playthrough or real-time smoothness. Best is updated only at Game Over. No position, point, or loss result is fabricated in the saved images.

The initial desktop-area capture did not capture the target window and was discarded. The committed files use target-window capture only. No desktop/background content is included. The helper remains ignored; it is diagnostic tooling, not an application runtime dependency. Owner screenshot acceptance remains part of [submission sign-off](../SUBMISSION.md).

SHA-256 checksums:

```text
F7DDCB6D3A391E879CB35A8ED8039E57D9C3529FD1E40348FF025BF96E98850D  gameplay.png
9894027084BE4BB70096DEDFBEB3821A13298FE09C8C23603F8663A8425996FF  game-over.png
```

To refresh them manually, run the application, earn a point, capture only its window while Running, then capture Game Over with final score/restart visible. Replace both PNGs, update this record/checksums, and inspect for clipping or unrelated content. Do not edit a screenshot to invent game state.
