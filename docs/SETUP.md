# Development environment

## Prerequisites

| Tool | Requirement | Purpose |
| --- | --- | --- |
| Windows | Windows 10/11 | Run and debug Windows Forms |
| .NET SDK | 10.0.402; newer patches in 10.0.4xx allowed | Build App/Core/Tests |
| Git | Available on PATH | Branches and version history |
| VS Code | Optional editor | Shared tasks and debugger |
| C# extensions | `ms-dotnettools.csharp`, optionally C# Dev Kit | C# language and debug support |
| GitHub CLI | Optional for local work | Authentication and PR management |

Install the SDK rather than only the runtime. The Windows SDK installation supplies the Windows Desktop targeting pack. There is no separate workload installation step for this solution. Package restore uses NuGet; no project secrets or `.env` file are required.

## Initial verification

Open PowerShell at the root of the existing checkout:

```powershell
./scripts/dev.ps1 doctor
./scripts/dev.ps1 check
./scripts/dev.ps1 run
```

`doctor` reports the SDK executable, SDK/runtime information, Git, and optional GitHub CLI availability. `check` runs restore, Release build, tests, and a formatting check, stopping at the first failure. `run` builds and launches the foundation window. Close that window to return to the terminal.

The repository pins the SDK through `global.json`. Updating the feature band requires an explicit project decision and matching validation. A .NET 9 SDK cannot build `net10.0` projects.

## Environment correction

During inspection on November 16, 2026, `dotnet` resolved to `C:\Program Files\dotnet\dotnet.exe`, but the inherited `DOTNET_ROOT` pointed to `D:\universty\NTP lab\.tools\dotnet`. `DOTNET_CLI_HOME` also pointed to a parent-directory tool folder. The original build still succeeded, but these overrides make runtime discovery and CLI state less predictable.

`scripts/dev.ps1` sets `DOTNET_ROOT` to the directory containing the selected executable and `DOTNET_CLI_HOME` to `<repository>/.local/dotnet` for its process only. Its `finally` block restores both previous values, including after failures. It does not edit user or machine environment variables or install software. This makes project commands independent of those inherited overrides.

For raw commands or VS Code debugging, use a terminal with the intended SDK on PATH. If a stale environment override affects direct execution, remove or correct it in your own Windows environment settings and restart VS Code. The shared debug configuration launches the built DLL through the C# debugger.

## VS Code

Open the repository root (the folder containing `FlappyBird.slnx`), accept the recommended extensions, and wait for C# project loading.

- **Ctrl+Shift+B** runs the Release build task.
- **Tasks: Run Task → test** builds and runs the test suite.
- **F5 → Flappy Bird (Windows)** builds and launches the application under the debugger.

The tasks call Windows PowerShell using a process task, so paths containing spaces are passed as separate arguments. A VS Code debug session needs the C# debugger installed; the command-line workflow does not require VS Code.

## Equivalent CLI commands

```powershell
dotnet restore FlappyBird.slnx
dotnet build FlappyBird.slnx -c Release --no-restore
dotnet test FlappyBird.slnx -c Release --no-build --logger trx --results-directory TestResults
dotnet format FlappyBird.slnx --verify-no-changes --no-restore
dotnet run --project src/FlappyBird.App -c Release
```

The raw commands inherit your shell environment; prefer the project launcher when using this checkout's original environment.

## Troubleshooting

| Symptom | Action |
| --- | --- |
| `dotnet` not found | Install the Windows .NET SDK, add its directory to PATH, and reopen the terminal. |
| Compatible SDK not found | Run `dotnet --list-sdks`; install the SDK feature band selected by `global.json`. |
| NuGet restore fails | Check network/proxy access to NuGet and the actual error; retry `./scripts/dev.ps1 restore`. Do not treat failed restore as a passing build. |
| PowerShell blocks scripts | Follow the institution's execution policy. If permitted, invoke `powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./scripts/dev.ps1 check` for that process only. |
| Build cannot overwrite App output | Close the running application or debugger and build again. |
| Formatting check fails | Run `dotnet format FlappyBird.slnx`, inspect the diff, then rerun `check`. |
| Application cannot locate .NET | Use `./scripts/dev.ps1 run`; inspect the SDK path and stale `DOTNET_ROOT` values. |
| No tests discovered | Use the solution or test project path, check restore/build output, and confirm the xUnit packages remain referenced. |
| GitHub authentication fails | Run `gh auth status` locally and authenticate as the repository owner; never paste tokens into the repository. |

`bin`, `obj`, `TestResults`, and `.local` are generated and ignored. They are not deliverable source files. No cleanup command is required to work on the project.
