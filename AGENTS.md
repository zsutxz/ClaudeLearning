# Repository Guidelines

## Project Structure & Module Organization
- UnityProject/Assets/ — main source (C#, scenes, prefabs).
- UnityProject/Packages/ — package manifests and package code.
- UnityProject/ProjectSettings/ — engine/project settings.
- Tests/ or UnityProject/Tests/ — automated tests.
- docs/ — design notes and diagrams.

## Build, Test, and Development Commands
- git status — inspect changes.
- git add -A && git commit -m 'msg' — stage and commit.
- Unity headless build: Unity -batchmode -projectPath . -executeMethod BuildScript.PerformBuild -quit.
- Run Editor tests via Unity Test Runner or CLI for EditMode/PlayMode tests.

## Coding Style & Naming Conventions
- C# style: 4-space indentation, PascalCase for types/methods, camelCase for locals/params.
- One public class per file; filename matches class name (e.g., PlayerController.cs).
- Organize assets under Assets/FeatureName/.
- Use project .editorconfig or Unity defaults for formatting; run IDE auto-format.

## Testing Guidelines
- Use Unity Test Framework (EditMode/PlayMode).
- Place tests under Tests/ or UnityProject/Tests/ with suffix *Tests.cs.
- Run tests through the Editor Test Runner or via the Unity CLI.

## Commit & Pull Request Guidelines
- Commit message: short imperative summary (e.g., Add player dash ability).
- PRs should include: description, linked issue, test instructions, and screenshots/GIFs for visuals.
- Keep PRs focused and small for easier review.

## Security & Configuration Tips
- Do not commit secrets (keystores, API keys). Add them to .gitignore.
- Document required environment variables in docs/README.md.

