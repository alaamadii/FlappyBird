# Project working agreement

## Scope and workflow

- Build the YMH219 C#/.NET 10 Windows Forms assignment incrementally.
- Work on a dedicated branch, never directly on `main`.
- Complete and review one development stage before starting the next.
- Do not merge pull requests automatically. The project owner approves merging.
- Do not publish, push, or create remote resources unless the task authorizes it.
- Preserve existing user changes. Never fabricate commits or development dates.

## Design

- `FlappyBird.Core` must stay independent of WinForms, drawing APIs, timers, and file UI.
- `FlappyBird.App` owns Windows input, presentation, and timer lifecycle.
- Keep classes small, responsibilities clear, and behavior explainable by a student.
- Do not implement future-stage gameplay while doing initialization.
- Prefer programmatic WinForms UI; this project does not require the Designer.

## Validation and documentation

- Run `./scripts/dev.ps1 check` from PowerShell after code/configuration changes.
- Add meaningful assertions for changed Core behavior; no empty passing tests.
- Report only checks actually executed. Distinguish local results, CI, and manual UI review.
- Record important design decisions in `docs/DECISIONS.md`.
- Update `docs/DEVELOPMENT.md` with the date and implemented work for every stage.
- Keep English Markdown documentation consistent with code and current limitations.
- Keep `bin`, `obj`, `.local`, `TestResults`, secrets, and personal IDE settings out of Git.
