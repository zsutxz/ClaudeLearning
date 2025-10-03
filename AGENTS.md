# Repository Guidelines

## Project Structure & Module Organization
- `UnityProject/Assets/` — main source (C#, scenes, prefabs).
- `UnityProject/Packages/` — package manifests and package code.
- `UnityProject/ProjectSettings/` — engine/project settings.
- `Tests/` or `UnityProject/Tests/` — automated tests where present.
- `docs/` — design notes, architecture diagrams.

## Build, Test, and Development Commands
- `git status` — inspect changes.
- `git add -A && git commit -m "msg"` — stage and commit all changes.
- Unity Editor CLI example: `Unity -batchmode -projectPath . -executeMethod BuildScript.PerformBuild -quit`.
- Use the Editor for iterative development; CI will run headless builds where configured.

## Coding Style & Naming Conventions
- C# style: use 4-space indentation, PascalCase for types/methods, camelCase for locals/params.
- Files: one public class per file, filename matches class name (`MyController.cs`).
- Assets: organize under `Assets/FeatureName/...`.
- Formatting: use the project .editorconfig or Unity default; run auto-format in your IDE.

## Testing Guidelines
- Use Unity Test Framework (EditMode/PlayMode). Place tests under `Tests/` with suffix `*Tests.cs`.
- Run tests via Unity CLI or Editor Test Runner.
- Aim for meaningful unit coverage on gameplay logic and integration tests for scenes.

## Commit & Pull Request Guidelines
- Commit message pattern: short summary (imperative), optional body. Example: `Add player dash ability`.
- PRs should include: description, linked issue, test instructions, and screenshots/GIFs for visual changes.
- Keep PRs focused; prefer small, reviewable commits.

## Security & Configuration Tips
- Don’t commit secrets or user-specific files (`Secrets.json`, local keystores).
- Add sensitive items to `.gitignore` and document required env vars in `docs/README.md`.

If you want, I can run a quick `git status` or open this file for edits.
