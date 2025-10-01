# Gomoku Game

A Unity-based implementation of the classic Five in a Row (Gomoku) strategy board game for local two-player gameplay.

## Overview

This project delivers a fully functional digital Gomoku game with a clean, intuitive user interface focused on local multiplayer gameplay. The implementation preserves the strategic essence of the classic board game while enhancing the user experience through thoughtful design and implementation.

## Features

- **Classic Gameplay**: Standard 15x15 Gomoku board with black and white pieces
- **Local Multiplayer**: Turn-based gameplay for two players on the same device
- **Win Detection**: Accurate detection of five-in-a-row wins in all directions
- **Variable Board Sizes**: Support for 9x9, 13x13, 15x15, and 19x19 board sizes
- **Customizable Themes**: Visual customization options for the board
- **Alternative Win Conditions**: Configurable win conditions beyond standard five-in-a-row
- **Persistent Settings**: Game settings saved between sessions
- **Cross-Platform**: Compatible with Windows and macOS

## Technical Architecture

The game is built using Unity with C#, following modern game development practices:

- **Game Engine**: Unity 2022.3 LTS
- **Language**: C#
- **Architecture**: Component-based design with clear separation of concerns
- **UI System**: Unity UI Toolkit
- **Testing**: Unity Test Framework
- **Persistence**: PlayerPrefs for settings storage

For detailed technical documentation, see:
- [Architecture Document](docs/architecture.md)
- [Coding Standards](docs/architecture/coding-standards.md)
- [Technology Stack](docs/architecture/tech-stack.md)
- [Source Tree](docs/architecture/source-tree.md)

## Project Structure

```
GomokuGame/
├── Assets/                  # Unity project assets
│   ├── Scenes/             # Game scenes
│   ├── Scripts/            # C# source code
│   ├── Prefabs/            # Reusable game objects
│   └── Resources/          # Runtime assets
├── docs/                   # Project documentation
│   ├── prd/                # Product requirements
│   ├── architecture/       # Architecture documentation
│   ├── stories/            # User stories
│   └── qa/                 # Quality assurance
└── README.md               # This file
```

## Development Setup

### Prerequisites

1. Unity Hub
2. Unity 2022.3 LTS or later
3. Visual Studio 2022 or JetBrains Rider
4. Git

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd GomokuGame
   ```

2. Open the project in Unity Hub

3. Unity will automatically import assets and set up the project

### Development Workflow

1. Open the project in Unity
2. Work on features in the Unity Editor
3. Write and run tests using Unity Test Framework
4. Build the game using Unity's Build Settings

## Documentation

Project documentation follows the BMAD (Business Methodology for AI Development) framework:

- [Product Requirements Document](docs/prd.md)
- [Project Brief](docs/project-brief.md)
- [Brainstorming Results](docs/brainstorming-session-results.md)
- [Architecture Documents](docs/architecture/)
- [User Stories](docs/stories/)
- [Quality Assurance](docs/qa/)

## Testing

The project uses Unity's Test Framework with:

- Unit tests for core game logic
- Integration tests for game systems
- PlayMode tests for runtime behavior

To run tests:
1. Open the Test Runner window in Unity
2. Select the appropriate test category
3. Run tests

## Building and Deployment

To build the game:
1. Open Build Settings in Unity
2. Select target platform (Windows/macOS)
3. Configure build options
4. Click "Build" to create executable

## Contributing

This project follows the BMAD methodology for AI-driven development. All contributions should:

1. Follow the established [coding standards](docs/architecture/coding-standards.md)
2. Include appropriate tests
3. Update relevant documentation
4. Follow the Git workflow with feature branches and pull requests

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with Unity Game Engine
- Inspired by the classic Gomoku board game
- Developed using the BMAD methodology