# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### 🔄 Project Awareness & Context
- This repository uses the BMAD (Business Methodology for AI Development) framework
- The project is focused on developing a Gomoku (Five in a Row) game in Unity for local two-player gameplay
- Follow the structured planning and development workflow defined in the BMAD methodology
- All development work should be based on sharded stories in the `docs/stories/` directory

### 🧱 Code Structure & Modularity
- The project follows a structured architecture with separation of concerns
- Code should be organized according to the architecture documents in `docs/architecture/`
- Follow the coding standards defined in `docs/architecture/coding-standards.md`
- Use the tech stack specified in `docs/architecture/tech-stack.md`

### 🧪 Testing & Reliability
- Follow the test architecture principles defined by the QA agent in BMAD
- Implement tests at appropriate levels (unit, integration, E2E) as guided by the test design
- Ensure all acceptance criteria have corresponding test coverage
- Run tests regularly to ensure no regressions are introduced

### ✅ Task Completion
- Mark completed tasks in the story files immediately after finishing them
- Update the Dev Agent Record sections in story files as required
- Add any new sub-tasks or TODOs discovered during development to the story file
- Ensure all validation checks pass before marking a story as complete

### 📎 Style & Conventions
- Follow the coding standards defined in the architecture documentation
- Write clean, maintainable code with clear variable and function names
- Add comments for complex logic explaining the why, not just the what
- Follow the established patterns in the codebase

### 📚 Documentation & Explainability
- Document complex algorithms and business logic with inline comments
- Update relevant documentation files when making significant changes
- Ensure code is self-documenting through clear naming conventions
- Follow the documentation guidelines in the architecture documents

### 🧠 AI Behavior Rules
- Never assume missing context. Ask questions if uncertain.
- Never hallucinate libraries or functions – only use known, verified packages.
- Always confirm file paths and module names exist before referencing them.
- Never delete or overwrite existing code unless explicitly instructed to.

### ⚙️ Development Commands
- Use the BMAD framework agents for development tasks:
  - `@dev` for implementation tasks
  - `@qa` for quality assurance and testing
  - `@architect` for architecture-related decisions
- Since this is primarily a Unity project, development will focus on Unity-specific workflows
- For any JavaScript/TypeScript components, standard npm commands would apply:
  - `npm install` - Install dependencies
  - `npm test` - Run tests (when test files exist)
  - `npm run build` - Build the project (when build scripts exist)

### 🏗️ Project Architecture
This repository follows the BMAD methodology with a structured approach to planning and development:

**Core Directories:**
- `.bmad-core/` - Contains the BMAD framework core files
- `.claude/` - Claude Code configuration and settings
- `docs/` - Documentation including PRD, architecture, stories, and QA artifacts
- `UnityProject/` - Unity game project files
- `web-bundles/` - Web agent bundles for different roles

**BMAD Workflow:**
1. Planning phase with PM, Architect, UX Expert agents
2. Document sharding into epics and stories
3. Development cycle with SM, Dev, and QA agents
4. Quality gates and continuous validation

**Project Structure:**
The repository follows the BMAD structured approach:
```
.
├── .bmad-core/              # BMAD framework core files
├── .claude/                 # Claude Code configuration
├── docs/                    # Documentation (PRD, architecture, stories)
│   ├── prd/                 # Product requirements documents
│   ├── architecture/        # Architecture documentation
│   ├── stories/             # Sharded user stories
│   └── qa/                  # Quality assurance artifacts
├── UnityProject/            # Unity game project files
│   ├── Assets/              # Unity assets
│   │   ├── Scripts/         # C# source code
│   │   │   ├── Core/        # Core game logic (BoardManager, GameManager, WinDetector)
│   │   │   ├── UI/          # UI components and controllers
│   │   │   └── Utilities/   # Utility functions
│   │   ├── Tests/           # Test files
│   │   ├── Scenes/          # Unity scenes
│   │   ├── Prefabs/         # Reusable game objects
│   │   └── Resources/       # Runtime assets
│   └── ProjectSettings/     # Unity project settings
├── web-bundles/             # Web agent bundles
├── package.json             # Project dependencies and scripts
└── README.md                # Project overview and setup instructions
```

### 🔧 Common Development Tasks
1. **Implementing stories**: Use the `@dev` agent with the `develop-story` command
2. **Testing**: Use the `@qa` agent for test design and validation
3. **Code quality**: Follow the coding standards and run linting tools
4. **Documentation**: Update story files and technical documentation as needed

### 📦 Project Dependencies and Structure
- Primary dependency: commander library (for CLI functionality)
- Focus: Unity game development for Gomoku (Five in a Row)
- Framework: BMAD methodology for AI-driven development
- Project structure follows BMAD conventions with sharded documentation

### 🧪 Testing Approach
- Tests should be designed using the `@qa *design` command for each story
- Follow the test levels framework defined in `.bmad-core/data/test-levels-framework.md`
- For Unity development, testing would involve:
  - Unit tests for game logic (if using Unity Test Framework)
  - Play mode tests for integration testing
  - Manual testing for UI and gameplay experience
- For any web-based components, use appropriate JavaScript/TypeScript testing frameworks

### 🛠️ Development Environment Setup
1. For Unity development:
   - Install Unity Hub
   - Install Unity 2022.3 LTS or later
   - Ensure Unity Test Framework is available
2. For web-based components:
   - Ensure Node.js (version 14 or higher) is installed
   - Run `npm install` to install dependencies
3. Familiarize yourself with the BMAD agents and workflow

### 🏃 Common Development Commands
Since this is primarily a Unity project, the development workflow will be Unity-centric:
- Unity-specific workflows for building and testing the Gomoku game
- For any web/JavaScript components:
  - `npm install` - Install dependencies
  - `npm test` - Run tests (when test files exist)
  - `npm run build` - Build the project (when build scripts exist)
- Refer to the BMAD documentation for agent-specific commands:
  - `@dev implement story` - Implement a user story
  - `@qa *review story` - Perform quality assurance review
  - `@architect review design` - Get architecture feedback

To work with the Unity project:
1. Open the project in Unity Hub: `UnityProject/UnityProject.sln`
2. Work on features in the Unity Editor
3. Write and run tests using Unity Test Framework
4. Build the game using Unity's Build Settings

### 🎮 Current Project Focus
The current project is implementing a Gomoku (Five in a Row) game in Unity:
- Two-player local gameplay
- No network functionality required
- No AI opponent implementation needed
- Focus on core game mechanics and user experience

### 📖 Documentation Guidelines
- Follow the BMAD documentation standards
- Update story files with implementation details
- Maintain clear and concise technical documentation
- Ensure all architecture decisions are properly documented

### 🧪 Testing Guidelines
- Use the QA agent to design comprehensive test strategies
- Implement tests at appropriate levels as determined by risk assessment
- Ensure test coverage for all acceptance criteria
- Follow the test quality principles defined in the BMAD framework
- For Unity development, utilize Unity's testing frameworks appropriately

### 🧱 Core Game Components
The Gomoku game consists of several core components:

1. **BoardManager** (`UnityProject/Assets/Scripts/Core/BoardManager.cs`):
   - Manages the game board state and piece placement
   - Handles board initialization and piece placement validation

2. **GameManager** (`UnityProject/Assets/Scripts/Core/GameManager.cs`):
   - Controls overall game state (MainMenu, Playing, Paused, GameOver)
   - Manages player turns and game flow
   - Handles game start, pause, resume, and end conditions

3. **WinDetector** (`UnityProject/Assets/Scripts/Core/WinDetector.cs`):
   - Detects win conditions in all directions (horizontal, vertical, diagonal)
   - Checks for draw conditions when the board is full

4. **BoardViewManager** (`BoardViewManager.cs`):
   - Manages the visual representation of the game board
   - Handles piece visualization and board rendering
   - Supports GPU instancing for performance optimization

### 🧪 Running Tests
To run tests in the Unity project:
1. Open the Test Runner window in Unity (Window → General → Test Runner)
2. Select the appropriate test category (EditMode or PlayMode)
3. Click "Run All" to execute tests

Note: The Unity Test Framework may need to be configured properly for tests to run.