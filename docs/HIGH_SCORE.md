# High-score storage

Persistent best is an optional enhancement selected for Stage 05. It is separate from the instructor's mandatory current-run scoring requirement.

## Location and format

Production resolves `Environment.SpecialFolder.LocalApplicationData` and uses:

```text
%LOCALAPPDATA%\YMH219-FlappyBird\high-score.txt
```

The path is independent of the current directory, checkout, and executable location. The folder is created only when a save is needed. No application data is committed; `.gitignore` also excludes accidentally copied `high-score.txt` and its temporary files.

The file contains an invariant-culture unsigned decimal integer in UTF-8 without a BOM, followed by a newline when written by the app. Example:

```text
12
```

Scores range from 0 to `int.MaxValue` (2,147,483,647). The reader trims surrounding whitespace and accepts at most 64 characters, rejecting empty content, signs, decimals, multiple values, overflow, excessive content, or invalid UTF-8. There is no JSON schema, database, migration, or third-party dependency. This is a convenience record, not a tamper-resistant leaderboard.

## Responsibilities and lifecycle

| Type | Responsibility |
| --- | --- |
| Core GameEngine | Current run's score, reset, and GameOver transition; no file or Windows dependency |
| App IHighScoreStore | Load a best and save/return the retained maximum |
| App FileHighScoreStore | Resolve the production path, validate bounded input, and replace the file safely |
| App HighScoreService | Load once, retain session/persisted best, handle storage errors, expose cached status |
| App GameForm | Create/inject storage, record each completed run once, preserve the service across restart |
| App GameRenderer | Draw supplied values/status; never read or write storage |

The Form's default constructor selects real storage. Tests/probes use the injected constructor. HighScoreService loads during startup. Best on Ready/Running represents the best completed run loaded or achieved in this session, while Score describes the current run. A new completed best updates both memory and storage on entering GameOver. A reset clears Score but preserves Best.

A successful persisted best suppresses further writes for equal/lower completed runs. If saving fails, the session best remains pending; the next finished run makes one retry even if its own score is lower. No repaint, focus event, ordinary timer tick, or reset retries it. Relaunch can recover only the last successful saved value.

## Failure behavior

| Situation | Result |
| --- | --- |
| Missing file/directory | Best 0, no warning and no eager file creation |
| Invalid, corrupt, or unreadable data | Best 0 and “Saved best score unavailable.”; gameplay remains usable |
| Unavailable LocalApplicationData or denied read | Same nonmodal load warning; no technical path/exception shown |
| New best cannot be saved | Keep the new best in memory and show “Best score couldn't be saved.” |
| Later save succeeds | Update the persisted maximum and clear the warning |

Warnings appear in the ground footer and Game Over panel and in the accessible description. They are cached plain text, with no dialogs or repeated logging in the game loop. A load warning remains until a successful later save or relaunch. Corrupt data is not edited just by loading; a later positive best can replace it.

Save re-reads valid data to avoid lowering a newer existing value. It writes and flushes a unique temporary file beside the destination, closes it, then calls `File.Move(..., overwrite: true)` to replace the old file. A write/replace failure leaves the previous valid destination intact; temporary cleanup is attempted in `finally`. The local locked-file test verifies this behavior. Expected I/O, invalid-data, access, and security exceptions are handled by the service without changing gameplay state.

This design has no lock spanning multiple application instances, cloud sync, filesystem crash-recovery guarantee, or live external-edit detection. Use one instance per Windows user. Writes are tiny synchronous operations on the UI thread at run completion, rather than per-frame work.

## Tests and diagnostics

FileHighScoreStoreTests uses a unique owned directory under the OS temporary folder and removes it after each case. HighScoreServiceTests and GameFormRulesTests inject memory stores; they never use the production default constructor. Native-window tests retain their nonparallel collection to isolate process-wide WinForms initialization from bitmap assertions.

File tests cover missing data, valid values including zero/maximum, fresh-instance reload, malformed/oversized/invalid UTF-8 data, maximum retention, recovery (including directly saving zero over corrupt content), locked reads, denied replacement, unchanged valid data, temporary-file cleanup, and negative input. Service/Form tests verify one save per completed new-best run, retention across resets, failure feedback/retry, and gameplay usability. Tests do not change system ACLs or the owner's score file.

Ignored diagnostic probes under `.local/scene-preview`, `.local/flight-probe`, `.local/rules-probe`, and `.local/polish-probe` also use explicit isolated score paths. The Release executable smoke check stays in Ready and closes without completing a run, so it cannot write a personal score. The normal product path is intentionally left untouched by automation. See [TESTING.md](TESTING.md) for executed checks and remaining owner validation.
