# Decision log

Record the date, context, decision, and consequences when changing an important design choice. These entries describe the current initialization foundation, not completed gameplay.

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
