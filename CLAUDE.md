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
- **BMAD Framework Agents**:
  - `@dev` - Implementation tasks and story development
  - `@qa` - Quality assurance, testing, and validation
  - `@architect` - Architecture decisions and technical guidance
- **Unity Development**:
  - Open project: `UnityProject/UnityProject.sln` in Unity Hub
  - Build game: Unity Build Settings → Select platform → Build
  - Run tests: Unity Test Runner (Window → General → Test Runner)
- **Node.js Components** (limited use):
  - `npm install` - Install commander dependency
  - Note: No npm test or build scripts currently configured
- **Git Operations**:
  - Follow BMAD workflow with feature branches
  - Update story files in `docs/stories/` when tasks are completed

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

### 🏃 Common Development Tasks
1. **Story Implementation**: 
   - Use `@dev` agent with story files from `docs/stories/`
   - Update story files with implementation progress
   - Follow BMAD development cycle

2. **Testing Workflow**:
   - Use `@qa` agent for test design and validation
   - Run Unity tests via Test Runner window
   - Tests located in `UnityProject/Assets/Tests/`

3. **Unity Development**:
   - Core scripts in `UnityProject/Assets/Scripts/Core/`
   - UI components in `UnityProject/Assets/Scripts/UI/`
   - Tests in `UnityProject/Assets/Tests/`
   - Scenes in `UnityProject/Assets/Scenes/`

4. **Documentation Updates**:
   - Update story files immediately after task completion
   - Follow BMAD documentation standards
   - Maintain Dev Agent Record sections

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

### 🧱 Core Game Architecture
**Game Logic Layer** (`UnityProject/Assets/Scripts/Core/`):
- `BoardManager.cs` - Board state, piece placement, validation
- `GameManager.cs` - Game state (MainMenu, Playing, GameOver), turn management
- `WinDetector.cs` - Win condition detection in all directions
- `InputManager.cs` - Mouse input to board coordinate conversion

**UI Layer** (`UnityProject/Assets/Scripts/UI/`):
- `BoardViewManager.cs` - Visual board representation, piece rendering
- `UIManager.cs` - UI state management, player interactions

**Supporting Systems**:
- `ThemeManager.cs` (`Themes/`) - Visual theme management
- `PlayerPrefsManager.cs` (`Utilities/`) - Settings persistence

**Key Dependencies**:
- Core game logic is independent of Unity-specific dependencies
- UI components depend on Unity UI system
- Settings use Unity's PlayerPrefs for persistence

### 🧪 Testing
**Running Tests in Unity:**
1. Open Test Runner: Window → General → Test Runner
2. Select test category (EditMode or PlayMode)
3. Run individual tests or "Run All"

**Test Structure:**
- **Unit Tests**: `BoardManagerUnitTest.cs`, `WinDetectorUnitTest.cs`, `GameManagerUnitTest.cs`
- **Integration Tests**: `GameFlowIntegrationTest.cs`, `SettingsPersistenceIntegrationTest.cs`
- **Validation Tests**: `WinConditionValidationTest.cs`, `BoardSizeTest.cs`
- **Performance Tests**: `PerformanceBenchmarkTest.cs`

**Test Locations:**
- All tests in `UnityProject/Assets/Tests/`
- Test files follow naming convention: `*UnitTest.cs`, `*IntegrationTest.cs`

### 🎨 UI Components
The UI is organized with the following key components:
- **Main Menu**: `MainMenuController.cs`, `MainMenuSetup.cs`
- **Game UI**: `UIManager.cs`, `GameCanvas.prefab`
- **Results Screen**: `ResultsScreenController.cs`
- **Settings Panel**: Various settings-related UI components

### 🎨 Theme System
The game supports multiple themes:
- **Classic**: Traditional black and white pieces
- **Modern**: Contemporary color scheme
- **Nature**: Earth-toned color scheme
Themes are managed through the `ThemeManager` and stored in `Resources/Materials/Themes/`