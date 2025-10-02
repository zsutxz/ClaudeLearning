# Full-Stack Architecture: Gomoku (Five in a Row) Game

## 1. Overview

This document outlines the full-stack architecture for the Gomoku (Five in a Row) game implementation using Unity. The architecture follows a component-based design with clear separation of concerns between core game logic, user interface, and utility functions.

## 2. Technology Stack

### 2.1 Primary Technologies
- **Game Engine**: Unity 2022.3 LTS
- **Programming Language**: C#
- **UI Framework**: Unity UI Toolkit
- **Testing Framework**: Unity Test Framework
- **Persistence**: PlayerPrefs for settings storage

### 2.2 Development Environment
- Unity Hub for project management
- Visual Studio for code editing
- Git for version control

## 3. System Architecture

### 3.1 High-Level Architecture
The Gomoku game follows a component-based architecture with the following layers:

```
┌─────────────────────────────────────────────────────────────┐
│                    User Interface Layer                     │
│  ┌───────────────┐  ┌─────────────────────────────────────┐ │
│  │   UI System   │  │       UI Controllers                │ │
│  │ (Unity UI)    │  │ (UIManager, ResultsScreenController)│ │
│  └───────────────┘  └─────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                  Game Management                        │ │
│  │  (GameManager, BoardManager, WinDetector)              │ │
│  └─────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│                    Data Layer                              │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                   Data Storage                          │ │
│  │              (PlayerPrefs, In-Memory)                   │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Component Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                    GameManager (Singleton)                  │
├─────────────────────────────────────────────────────────────┤
│ - GameState                                                 │
│ - CurrentPlayer                                             │
│ - BoardSize                                                 │
│ - WinCondition                                              │
├─────────────────────────────────────────────────────────────┤
│ + StartNewGame()                                            │
│ + SwitchPlayer()                                            │
│ + EndGame()                                                 │
│ + RestartGame()                                             │
│ + ReturnToMainMenu()                                        │
│ + DeclareWinner()                                           │
│ + DeclareDraw()                                             │
│ + UpdateSettings()                                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    BoardManager                             │
├─────────────────────────────────────────────────────────────┤
│ - boardState[,]                                             │
│ - boardPieces[,]                                            │
│ - boardSize                                                 │
├─────────────────────────────────────────────────────────────┤
│ + InitializeBoard()                                         │
│ + PlacePiece()                                              │
│ + GetPieceAt()                                              │
│ + IsPositionEmpty()                                         │
│ + ClearBoard()                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    WinDetector                              │
├─────────────────────────────────────────────────────────────┤
│ - winCondition                                              │
├─────────────────────────────────────────────────────────────┤
│ + CheckWin()                                                │
│ + CheckDirection()                                          │
│ + CheckDraw()                                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    UIManager (Singleton)                    │
├─────────────────────────────────────────────────────────────┤
│ - currentPlayerText                                         │
│ - gameOverText                                              │
│ - gameUIPanel                                               │
│ - gameOverPanel                                             │
│ - mainMenuPanel                                             │
│ - settingsPanel                                             │
├─────────────────────────────────────────────────────────────┤
│ + ShowMainMenu()                                            │
│ + StartGame()                                               │
│ + RestartGame()                                             │
│ + ReturnToMainMenu()                                        │
│ + ShowSettings()                                            │
│ + SaveSettings()                                            │
│ + CancelSettings()                                          │
└─────────────────────────────────────────────────────────────┘
```

## 4. Core Components

### 4.1 GameManager (Singleton)
The central orchestrator of the game that manages the overall game flow and state.

**Responsibilities:**
- Managing game state (MainMenu, Playing, Paused, GameOver)
- Tracking current player
- Handling game lifecycle events
- Managing game settings
- Implementing singleton pattern for global access

**Key Methods:**
- `StartNewGame()` - Initializes a new game session
- `SwitchPlayer()` - Alternates between players
- `DeclareWinner()` - Ends the game with a winner
- `UpdateSettings()` - Updates board size and win condition

### 4.2 BoardManager
Manages the game board state and piece placement logic.

**Responsibilities:**
- Maintaining the board state in a 2D array
- Handling piece placement on the board
- Checking position states
- Initializing and clearing the board

**Key Methods:**
- `InitializeBoard()` - Sets up the board with specified size
- `PlacePiece()` - Places a piece at a specific position
- `GetPieceAt()` - Returns the player at a specific position
- `IsPositionEmpty()` - Checks if a position is empty

### 4.3 WinDetector
Handles win condition detection for the Gomoku game.

**Responsibilities:**
- Checking win conditions after each move
- Detecting draw conditions
- Implementing efficient win detection algorithms

**Key Methods:**
- `CheckWin()` - Determines if the last move resulted in a win
- `CheckDirection()` - Checks for win condition in a specific direction
- `CheckDraw()` - Determines if the game is a draw

### 4.4 UIManager (Singleton)
Manages the user interface elements and their interactions.

**Responsibilities:**
- Controlling UI panel visibility
- Handling user input from UI elements
- Updating UI elements with game state
- Managing settings UI
- Implementing singleton pattern for global access

**Key Methods:**
- `ShowMainMenu()` - Displays the main menu
- `StartGame()` - Initiates a new game
- `SaveSettings()` - Persists user settings
- `UpdatePlayerText()` - Updates the current player display

## 5. Data Management

### 5.1 In-Memory Data
- Game board state stored in a 2D integer array
- Game settings and state variables
- UI element references

### 5.2 Persistent Data
- Player preferences stored using PlayerPrefs
- Board size settings
- Win condition settings

## 6. User Interface Architecture

### 6.1 UI Structure
- **Main Menu**: Game access and settings
- **Game UI**: Current player display and game controls
- **Game Over**: Results display with win/draw information
- **Settings**: Configuration options for board size and win conditions

### 6.2 UI Components
- Text elements for displaying game information
- Buttons for user interactions
- Sliders for settings adjustments
- Panels for organizing UI elements

## 7. Event System

### 7.1 Game Events
- `OnGameStateChanged` - Notifies when game state changes
- `OnPlayerChanged` - Notifies when current player changes
- `OnGameStarted` - Notifies when a new game begins
- `OnGameWon` - Notifies when a player wins
- `OnGameDraw` - Notifies when the game ends in a draw

### 7.2 UI Events
- Button click handlers
- Slider value change handlers
- Scene navigation events

## 8. Testing Architecture

### 8.1 Unit Testing
- Win detection algorithm testing
- Board management functionality testing
- Game state management testing

### 8.2 Integration Testing
- Complete game flow testing
- UI interaction testing
- Settings persistence testing

## 9. Deployment Architecture

### 9.1 Build Process
- Unity build pipeline for target platforms
- Asset bundling and optimization
- Post-processing stack for visual enhancements

### 9.2 Distribution
- Standalone executable for desktop platforms
- Potential mobile deployment (future enhancement)

## 10. Performance Considerations

### 10.1 Optimization Strategies
- Object pooling for board pieces (future enhancement)
- Efficient win detection algorithms
- Memory management for large board sizes

### 10.2 Scalability
- Support for variable board sizes (9x9, 13x13, 15x15, 19x19)
- Configurable win conditions
- Modular component design for easy extension

## 11. Security Considerations

### 11.1 Data Security
- No sensitive data storage
- Local data persistence only
- No network communication

### 11.2 Code Security
- Input validation for all user interactions
- Bounds checking for array access
- Error handling for edge cases

## 12. Future Enhancements

### 12.1 Planned Features
- Board themes and visual customization
- Game statistics tracking
- Tutorial mode for new players
- Mobile platform deployment

### 12.2 Architecture Improvements
- Dependency injection for better testability
- ScriptableObjects for configuration management
- Addressable assets for better resource management

## 13. Conclusion

This architecture provides a solid foundation for the Gomoku game implementation with clear separation of concerns, maintainable code structure, and room for future enhancements. The component-based design allows for modular development and testing while maintaining good performance and user experience.