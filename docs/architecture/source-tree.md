# Gomoku Game Source Tree

## Overview

This document provides a detailed overview of the project's source tree structure, explaining the purpose and contents of each directory and file in the repository.

## Root Directory

```
GomokuGame/
├── .bmad-core/              # BMAD framework core files
├── .claude/                 # Claude Code configuration
├── .git/                    # Git version control directory
├── Assets/                  # Unity project assets
├── docs/                    # Project documentation
├── Packages/                # Unity package dependencies
├── ProjectSettings/         # Unity project configuration
├── web-bundles/             # Web agent bundles
├── .gitignore               # Git ignore rules
├── CLAUDE.md                # Claude Code project instructions
├── INITIAL_Gomoku.md        # Initial project description
├── package.json             # NPM package configuration
└── README.md                # Project README (to be created)
```

## BMAD Core Directory

```
.bmad-core/
├── agents/                  # BMAD agent definitions
├── data/                    # Shared data files
├── tasks/                   # Executable workflow tasks
└── templates/               # Document templates
```

## Unity Assets Directory

```
Assets/
├── Scenes/                  # Unity scenes
│   ├── MainMenu.unity       # Main menu scene
│   └── GameScene.unity      # Primary gameplay scene
├── Scripts/                 # C# scripts
│   ├── Core/                # Core game logic
│   │   ├── GameManager.cs   # Main game state manager
│   │   ├── BoardManager.cs  # Board management logic
│   │   ├── PieceManager.cs  # Piece placement logic
│   │   └── WinDetector.cs   # Win condition detection
│   ├── UI/                  # User interface components
│   │   ├── UIManager.cs     # Main UI controller
│   │   ├── MenuController.cs # Menu system controller
│   │   ├── GameUI.cs        # In-game UI elements
│   │   └── SettingsUI.cs    # Settings interface
│   ├── Utilities/           # Helper classes
│   │   ├── EventManager.cs  # Event system management
│   │   └── PlayerPrefsManager.cs # Settings persistence
│   └── Tests/               # Game tests
│       ├── UnitTests/       # Unit tests
│       └── IntegrationTests/ # Integration tests
├── Prefabs/                 # Reusable game objects
│   ├── Board.prefab         # Game board prefab
│   ├── Piece.prefab         # Game piece prefab
│   └── UI/                  # UI element prefabs
├── Materials/               # Visual materials
├── Textures/                # Image assets
├── Fonts/                   # Font assets
├── Audio/                   # Sound assets
└── Resources/               # Runtime loaded assets
```

## Documentation Directory

```
docs/
├── architecture/            # Architecture documentation
│   ├── coding-standards.md  # Coding standards
│   ├── source-tree.md       # Source tree documentation
│   └── tech-stack.md        # Technology stack
├── prd/                     # Product requirements (sharded)
│   ├── epic-1*.md           # Foundation & Core Gameplay
│   ├── epic-2*.md           # User Interface & Local Multiplayer
│   ├── epic-3*.md           # Customization & Variable Gameplay
│   └── epic-4*.md           # Game Settings & Persistence
├── stories/                 # User stories (sharded)
│   ├── story-1.1*.md        # Basic Gomoku board
│   ├── story-1.2*.md        # Piece placement logic
│   ├── story-1.3*.md        # Win detection
│   ├── story-2.1*.md        # Game state display
│   ├── story-2.2*.md        # Game controls
│   ├── story-2.3*.md        # Main menu
│   ├── story-3.1*.md        # Board size options
│   ├── story-3.2*.md        # Board themes
│   ├── story-3.3*.md        # Alternative win conditions
│   ├── story-4.1*.md        # Game settings menu
│   └── story-4.2*.md        # Settings persistence
├── qa/                      # Quality assurance artifacts
│   └── test-design/         # Test design documents
├── prd.md                   # Main product requirements document
├── architecture.md          # Main architecture document
├── project-brief.md         # Project brief
└── brainstorming-session-results.md # Brainstorming results
```

## Web Bundles Directory

```
web-bundles/
├── agents/                  # Agent-specific bundles
│   ├── dev/                 # Developer agent bundle
│   ├── qa/                  # QA agent bundle
│   ├── architect/           # Architect agent bundle
│   └── pm/                  # Product manager agent bundle
└── roles/                   # Role-specific bundles
    ├── developer/           # Developer role bundle
    ├── tester/              # Tester role bundle
    └── designer/            # Designer role bundle
```

## Configuration Files

### package.json

Contains NPM package dependencies:
- commander: For CLI functionality

### .gitignore

Specifies files and directories that should not be tracked by Git:
- Unity build artifacts
- Temporary files
- IDE-specific files
- OS-specific files

### CLAUDE.md

Provides guidance to Claude Code when working with the repository:
- Project awareness and context
- Code structure and modularity guidelines
- Testing and reliability requirements
- Task completion criteria
- Style and conventions
- Development commands and workflow

### INITIAL_Gomoku.md

Contains the initial project description:
- Unity-based Gomoku game for local two-player gameplay
- No networking or AI requirements

## Unity-Specific Directories

### Packages Directory

Contains Unity package dependencies managed by the Unity Package Manager:
- Core Unity packages
- Third-party packages

### ProjectSettings Directory

Contains Unity project configuration files:
- Editor settings
- Build settings
- Physics settings
- Graphics settings
- Input settings

## Development Workflow Directories

### .claude Directory

Contains Claude Code configuration and settings:
- Hook configurations
- Custom commands
- AI assistant settings

### .bmad-core Directory

Contains the BMAD framework core files:
- Agents for different roles (developer, QA, architect, product manager)
- Tasks for common workflows
- Templates for documentation
- Data files for configuration

## File Naming Conventions

### Documentation Files

- Use lowercase with hyphens for separation
- Use .md extension for Markdown files
- Include version numbers in filenames when appropriate

### Script Files

- Use PascalCase matching class names
- One class per file
- Include .cs extension

### Asset Files

- Use descriptive names
- Follow Unity naming conventions
- Group related assets in subdirectories

## Directory Organization Principles

### Separation of Concerns

Directories are organized to separate different concerns:
- Source code vs. documentation
- Core logic vs. UI components
- Development vs. runtime assets

### Scalability

The structure supports growth:
- Easy addition of new features
- Clear organization as project expands
- Logical grouping of related functionality

### Maintainability

The structure promotes maintainability:
- Clear file and directory purposes
- Consistent naming conventions
- Logical grouping of related items

## Access and Permissions

### Read Access

All team members should have read access to:
- Documentation
- Source code
- Asset files
- Configuration files

### Write Access

Write access should be controlled through:
- Git branching strategy
- Pull request review process
- Code ownership policies

## Updates and Maintenance

This source tree documentation should be updated when:
- New directories are added
- Directory purposes change
- File organization is refactored
- New team members join the project

Regular reviews should ensure:
- Accuracy of directory descriptions
- Consistency with actual file structure
- Clarity of organizational principles