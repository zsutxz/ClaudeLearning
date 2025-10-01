# Data Models

## Game State

**Purpose:** Represents the current state of the game including board state, player turns, and game status.

**Key Attributes:**
- BoardState: int[,] - 2D array representing the game board
- CurrentPlayer: int - Identifier for the current player (1 or 2)
- GameStatus: enum - Current game status (Playing, Won, Draw)
- BoardSize: int - Current board dimensions

## Player

**Purpose:** Represents a player in the game with their properties.

**Key Attributes:**
- PlayerId: int - Unique identifier for the player
- PlayerName: string - Display name for the player
- PieceType: int - Type of piece the player uses (1 for black, 2 for white)
- Score: int - Player's current score

## Settings

**Purpose:** Stores user preferences and game settings.

**Key Attributes:**
- BoardSize: int - Preferred board size (9, 13, 15, 19)
- Theme: string - Selected visual theme
- WinCondition: int - Number of pieces in a row needed to win
- LastPlayed: DateTime - Timestamp of last game
