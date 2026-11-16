# Decision log

Record the date, context, decision, and consequences when changing an important design choice. Entries are chronological: initialization choices precede the Stage 01 rendering decisions. Gameplay remains planned.

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

**Date:** 2026-11-16. **Status:** Implemented in Stage 01; owner review pending.

Keep Playfield dimensions as the logical coordinate system. GameForm delegates paint events to an App-only GameRenderer, passing its actual client size. The renderer scales uniformly, centers the scene, and uses borders when aspect ratios differ. WinForms autoscaling starts at 96 DPI; fonts use pixel units and the same transform as shapes, avoiding duplicate DPI scaling. The window remains fixed with maximize disabled.

Use original GDI+ primitives for the scenery and bird preview. No gameplay Bird model or external assets are needed in this stage. Save/restore graphics state and dispose drawing resources per paint; the renderer holds no disposable native resources between calls. This favors explicit ownership and straightforward repaint behavior over premature caching. The composition is designed for the default 480 × 640 playfield; arbitrary game-layout customization remains future work.

## ADR-008 — Windows renderer regression tests

**Date:** 2026-11-16. **Status:** Implemented in Stage 01; owner review pending.

Add a separate `net10.0-windows` App.Tests project rather than coupling Core tests to System.Drawing. Bitmap tests cover common scale factors, centered borders, ground/bird placement, graphics-state restoration, empty client areas, and deterministic repaint. Keep visual review separate: these assertions do not prove live cover/uncover behavior, smoothness, or per-monitor DPI transitions. The existing Windows CI runs both suites through the solution without changing the Core target.
