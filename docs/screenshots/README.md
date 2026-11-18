# Application screenshots

These PNG files capture the actual GameForm window through the Windows `PrintWindow` API. They are not mockups, generated artwork, or composites. The images were inspected before inclusion and contain only the application window.

| Item | Evidence |
| --- | --- |
| Capture date | 2026-11-18 |
| Application source | 0a1707333fca919194faff2caaa29ec7dace33a1 on feat/retro-arcade, retaining the f231b51 flight candidate |
| Display | 120 DPI (125%); 600×800 client area, 618×847 full window |
| Ready | [ready.png](ready.png): original pixel theme, stationary Core model, animated presentation |
| Gameplay | [gameplay.png](gameplay.png): Running, score 1, best 0 |
| Loss | [game-over.png](game-over.png): ordinary engine collision, final score 1, best 1, focused New game button |
| Storage | Explicit unique path in ignored `.local/final-evidence`; owner's real score is untouched |

The temporary helper creates the real Form and uses its Space key handling. It sets the two initial gap tops to 190 for a reproducible safe path, then steps the real engine and App animation at 1/60 second while issuing scripted flaps. A complete pass earns the point through ordinary scoring. It captures Running, stops issuing flaps, lets normal collision logic end the run, then captures Game Over and checks restart.

The helper pauses/resets the production timer/clock during direct stepping and capture to avoid double updates. It freezes the supplied visual snapshot for PrintWindow. These images establish actual UI rendering and a controlled rules path, not a human random-gap playthrough or real-time smoothness. Best updates only at Game Over. No score or loss result is fabricated in the saved images.

The committed files use target-window capture only; no desktop/background content is included. The helper remains ignored diagnostic tooling. No Higgsfield-generated artwork or mockup is used as screenshot evidence. Owner acceptance remains part of [submission sign-off](../SUBMISSION.md).

SHA-256 checksums:

```text
A3044B3A9799022B1FF3F59461DD959A15E2C57296BE5C278789F41C86621CE5  ready.png
2884DEC5B54D642387C0711A19A7181AA997B15011FD1DCEBDE4E849D11DC467  gameplay.png
A7123AA0F51443968DCFA233C333718FF484FBA03F7E0BE33858CDB50B9C0318  game-over.png
```

To refresh them manually, run the application, earn a point, capture only its window while Running, then capture Game Over with final score/restart visible. Replace both PNGs, update this record/checksums, and inspect for clipping or unrelated content. Do not edit a screenshot to invent game state.
