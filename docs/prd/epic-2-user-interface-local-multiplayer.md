# Epic 2 User Interface & Local Multiplayer

The goal of this epic is to create an intuitive user interface for local two-player gameplay, including turn management and game state display.

## Story 2.1 Implement game state display
As a player,
I want to see whose turn it is,
so that I know when it's my turn to play.

Acceptance Criteria:
1. The UI shall clearly indicate which player's turn it is (black or white)
2. The turn indicator shall update immediately after each move
3. The turn indicator shall be visually distinct and easy to understand

## Story 2.2 Implement game controls
As a player,
I want to be able to restart or quit the game,
so that I can start a new game or exit when finished.

Acceptance Criteria:
1. The UI shall provide a "Restart Game" button that resets the board
2. The UI shall provide a "Quit Game" button that exits to the main menu
3. Restarting shall preserve current game settings
4. Quitting shall return the player to the main menu

## Story 2.3 Create main menu
As a player,
I want a main menu to start the game,
so that I can begin playing.

Acceptance Criteria:
1. The main menu shall provide a "Start Game" button
2. The main menu shall provide access to game settings
3. The main menu shall have a visually appealing design consistent with the game
4. Clicking "Start Game" shall transition to the game board screen
