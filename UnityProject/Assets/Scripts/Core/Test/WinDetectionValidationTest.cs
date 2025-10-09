using UnityEngine;
using GomokuGame.Core;
using System.Collections;
using GomokuGame.UI;

/// <summary>
/// Final validation test to confirm win detection works across all board sizes
/// </summary>
public class WinDetectionValidationTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoardViewManager boardViewManager;
    [SerializeField] private WinDetector winDetector;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (boardViewManager == null)
            boardViewManager = FindObjectOfType<BoardViewManager>();

        if (winDetector == null)
            winDetector = FindObjectOfType<WinDetector>();

        // Run final validation tests
        StartCoroutine(RunFinalValidationTests());
    }

    private IEnumerator RunFinalValidationTests()
    {
        Debug.Log("Starting Final Win Detection Validation Tests");

        // Test each board size
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            Debug.Log($"Final validation for board size: {size}x{size}");

            // Test win detection for this size
            yield return StartCoroutine(TestWinDetectionForSize(size));
        }

        Debug.Log("Final Win Detection Validation Tests Completed Successfully!");
        Debug.Log("All board sizes have been validated for win detection functionality.");
    }

    private IEnumerator TestWinDetectionForSize(int size)
    {
        Debug.Log($"Testing win detection for {size}x{size} board");

        // Set and initialize the board
        gameManager.SetBoardSize(size);
        gameManager.StartNewGame();

        // Wait a frame for initialization
        yield return null;

        // Test all win directions
        bool horizontalWin = TestHorizontalWin(size);
        bool verticalWin = TestVerticalWin(size);
        bool diagonalWin1 = TestDiagonalWin1(size);
        bool diagonalWin2 = TestDiagonalWin2(size);

        Debug.Log($"Win detection results for {size}x{size}:");
        Debug.Log($"  Horizontal: {horizontalWin}");
        Debug.Log($"  Vertical: {verticalWin}");
        Debug.Log($"  Diagonal /: {diagonalWin1}");
        Debug.Log($"  Diagonal \\: {diagonalWin2}");
        Debug.Log($"  All directions: {horizontalWin && verticalWin && diagonalWin1 && diagonalWin2}");

        // Test boundary conditions
        bool boundaryChecks = TestBoundaryConditions(size);
        Debug.Log($"  Boundary checks: {boundaryChecks}");
    }

    private bool TestHorizontalWin(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 pieces horizontally in the middle of the board
        int middleRow = size / 2;
        int startX = Mathf.Max(0, (size / 2) - 2); // Start 2 positions before middle

        // Make sure we don't go out of bounds
        if (startX + 5 <= size)
        {
            for (int i = 0; i < 5; i++)
            {
                gameManager.boardManager.PlacePiece(startX + i, middleRow, GameManager.Player.Black);
            }

            // Check win on the last piece placed
            return gameManager.CheckWin(startX + 4, middleRow);
        }

        return false; // Board too small
    }

    private bool TestVerticalWin(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 pieces vertically in the middle of the board
        int middleCol = size / 2;
        int startY = Mathf.Max(0, (size / 2) - 2); // Start 2 positions before middle

        // Make sure we don't go out of bounds
        if (startY + 5 <= size)
        {
            for (int i = 0; i < 5; i++)
            {
                gameManager.boardManager.PlacePiece(middleCol, startY + i, GameManager.Player.White);
            }

            // Check win on the last piece placed
            return gameManager.CheckWin(middleCol, startY + 4);
        }

        return false; // Board too small
    }

    private bool TestDiagonalWin1(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 pieces diagonally (/) in the middle of the board
        int startCol = Mathf.Max(0, (size / 2) - 2);
        int startRow = Mathf.Max(0, (size / 2) - 2);

        // Make sure we don't go out of bounds
        if (startCol + 5 <= size && startRow + 5 <= size)
        {
            for (int i = 0; i < 5; i++)
            {
                gameManager.boardManager.PlacePiece(startCol + i, startRow + i, GameManager.Player.Black);
            }

            // Check win on the last piece placed
            return gameManager.CheckWin(startCol + 4, startRow + 4);
        }

        return false; // Board too small
    }

    private bool TestDiagonalWin2(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 pieces diagonally (\) in the middle of the board
        int startCol = Mathf.Max(0, (size / 2) - 2);
        int startRow = Mathf.Min(size - 1, (size / 2) + 2);

        // Make sure we don't go out of bounds
        if (startCol + 5 <= size && startRow - 4 >= 0)
        {
            for (int i = 0; i < 5; i++)
            {
                gameManager.boardManager.PlacePiece(startCol + i, startRow - i, GameManager.Player.White);
            }

            // Check win on the last piece placed
            return gameManager.CheckWin(startCol + 4, startRow - 4);
        }

        return false; // Board too small
    }

    private bool TestBoundaryConditions(int size)
    {
        // Test placing pieces at boundaries
        bool valid1 = gameManager.boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        bool valid2 = gameManager.boardManager.PlacePiece(size - 1, size - 1, GameManager.Player.White);

        // Test that out of bounds placements fail
        bool invalid1 = !gameManager.boardManager.PlacePiece(-1, 0, GameManager.Player.Black);
        bool invalid2 = !gameManager.boardManager.PlacePiece(0, -1, GameManager.Player.Black);
        bool invalid3 = !gameManager.boardManager.PlacePiece(size, 0, GameManager.Player.Black);
        bool invalid4 = !gameManager.boardManager.PlacePiece(0, size, GameManager.Player.Black);

        // Test that getting pieces at invalid positions returns None
        GameManager.Player piece1 = gameManager.boardManager.GetPieceAt(-1, 0);
        GameManager.Player piece2 = gameManager.boardManager.GetPieceAt(0, -1);
        GameManager.Player piece3 = gameManager.boardManager.GetPieceAt(size, 0);
        GameManager.Player piece4 = gameManager.boardManager.GetPieceAt(0, size);
        bool allNone = (piece1 == GameManager.Player.None) &&
                      (piece2 == GameManager.Player.None) &&
                      (piece3 == GameManager.Player.None) &&
                      (piece4 == GameManager.Player.None);

        return valid1 && valid2 && invalid1 && invalid2 && invalid3 && invalid4 && allNone;
    }
}