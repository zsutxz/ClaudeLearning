# Epic 1 Foundation & Core Gameplay

The goal of this epic is to establish the foundational elements of the Gomoku game, including the Unity project setup, basic board implementation, and core game logic for detecting wins.

## Story 1.1 Implement basic Gomoku board
As a player,
I want to see a Gomoku board on my screen,
so that I can play the game.

Acceptance Criteria:
1. The board shall be a 15x15 grid
2. The board shall visually distinguish intersection points where pieces can be placed
3. The board shall be centered on the screen
4. The board shall have a clean, visually appealing design

## Story 1.2 Implement piece placement logic
As a player,
I want to place pieces on the board by clicking,
so that I can make my moves.

Acceptance Criteria:
1. Players shall be able to place pieces only on valid intersection points
2. Players shall alternate turns between black and white pieces
3. Players shall not be able to place pieces on occupied intersections
4. Invalid clicks shall be ignored without error messages

## Story 1.3 Implement win detection
As a player,
I want the game to detect when I get five in a row,
so that I know when I've won.

Acceptance Criteria:
1. The system shall detect five consecutive pieces in horizontal direction
2. The system shall detect five consecutive pieces in vertical direction
3. The system shall detect five consecutive pieces in diagonal directions
4. The system shall declare a winner when five in a row is detected
