# Theme Materials Documentation

This document describes the materials that need to be created for each theme in the Gomoku game.

## Material Requirements

For each theme, the following materials need to be created:

1. Black Piece Material - for black game pieces
2. White Piece Material - for white game pieces
3. Board Line Material - for the grid lines on the board
4. Board Point Material - for the intersection points on the board

## Classic Theme Materials

Path: Assets/Resources/Materials/Themes/Classic/

1. Classic_Black_Mat.mat - Pure black material
2. Classic_White_Mat.mat - Pure white material
3. Classic_Board_Mat.mat - Black material for board lines
4. Classic_Point_Mat.mat - Black material for intersection points

## Modern Theme Materials

Path: Assets/Resources/Materials/Themes/Modern/

1. Modern_Black_Mat.mat - Very dark gray (RGB: 26, 26, 26)
2. Modern_White_Mat.mat - Light gray (RGB: 230, 230, 230)
3. Modern_Board_Mat.mat - Dark gray (RGB: 51, 51, 51)
4. Modern_Point_Mat.mat - Medium gray (RGB: 77, 77, 77)

## Nature Theme Materials

Path: Assets/Resources/Materials/Themes/Nature/

1. Nature_Black_Mat.mat - Dark color (RGB: 26, 26, 26)
2. Nature_White_Mat.mat - Light brown (RGB: 204, 179, 153)
3. Nature_Board_Mat.mat - Brown (RGB: 102, 51, 26)
4. Nature_Point_Mat.mat - Green (RGB: 77, 153, 77)

## Creation Instructions

To create these materials in Unity:

1. In the Project window, navigate to the appropriate theme folder under Assets/Resources/Materials/Themes/
2. Right-click and select Create > Material
3. Name the material according to the naming convention above
4. Select the appropriate shader (Standard is recommended)
5. Set the color values as specified above
6. Save the material

These materials will be automatically loaded by the ThemeManager at runtime using Resources.Load<Material>().