# Contributing to NoteAI Task Terminal

Thank you for your interest in contributing. Please read this document
before submitting any pull request.

## 1. Legal Notice (Read First)

This repository is **source-available, proprietary software** — not open
source. By submitting a pull request, patch, or any other contribution,
you agree to the terms of [`LICENSE §4 (Contributions)`](./LICENSE):

- You irrevocably assign to the Author all right, title, and interest in
  your contribution, or grant the Author a perpetual, worldwide,
  royalty-free, irrevocable license to use, modify, and relicense it —
  at the Author's election.
- You confirm the contribution is your own original work and that you
  have the right to grant this assignment/license.
- Contributions may be reviewed, modified, rejected, or removed at the
  Author's sole discretion.
- Submitting a contribution grants you no ownership, royalty, or license
  rights over the Software as a whole.

If you do not agree to these terms, do not submit a pull request.

## 2. Development Setup

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- [Ollama](https://ollama.ai/) running locally, with a model pulled
  (e.g. `ollama pull qwen2.5-coder:7b`)
- An IDE with C#/Avalonia support (optional, any works via `dotnet` CLI):
  - [Visual Studio](https://visualstudio.microsoft.com/) (Windows only)
  - [Visual Studio Code](https://code.visualstudio.com/) with the C# Dev Kit
    extension (cross-platform)
  - [JetBrains Rider](https://www.jetbrains.com/rider/) (cross-platform)
  
### Clone & Build
```bash
git clone https://github.com/Rinwikun/NoteAITask.git
cd NoteAITask
dotnet restore
dotnet build
```

> Local compilation for evaluation/development is permitted under
> [`LICENSE §2`](./LICENSE). You may **not** publish, redistribute, or
> commercially use your local build.

## 3. Project Conventions

- **Pattern:** MVVM via CommunityToolkit.Mvvm (`ObservableProperty`
  source generators). Do not introduce manual `INotifyPropertyChanged`
  boilerplate.
- **Settings persistence:** Each settings card owns an isolated
  persistence method — do not add shared "save all" methods that touch
  fields outside a card's own scope (see `SettingsViewModel.cs` for the
  established pattern).
- **No mid-save disk reads:** `SettingsViewModel`'s in-memory
  `ObservableProperty` fields are the single source of truth during a
  session. Do not re-read from disk between building a snapshot and
  writing it.
- **Error handling:** No empty `catch {}` blocks. Surface real failures
  to the UI; never silently report success on a failed write.
- **Cross-platform safety:** Code must run correctly on Windows, Linux,
  and (best-effort) macOS. Avoid OS-specific path assumptions outside
  the shell executor abstraction.

## 4. Submitting a Pull Request

1. Open an Issue first for non-trivial changes, to confirm scope before
   you invest time.
2. Keep PRs focused — one feature or fix per PR.
3. Include a clear description of what changed and why.
4. Ensure `dotnet build` succeeds with no new warnings before submitting.
5. Do not include compiled binaries, `bin/`, `obj/`, or local secrets
   (API keys, `.env` files) in your PR.

## 5. Reporting Bugs / Requesting Features

- **Bugs:** Open an [Issue](https://github.com/Rinwikun/NoteAITask/issues)
  with reproduction steps, OS, and logs if available.
- **Feature requests / feedback:** Use the
  [Discussions](https://github.com/Rinwikun/NoteAITask/discussions) tab.

## 6. Code of Conduct

Be respectful. No harassment, discrimination, or abusive language in
Issues, Discussions, or PR reviews.
