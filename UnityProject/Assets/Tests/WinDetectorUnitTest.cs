using UnityEngine;
using GomokuGame.Core;

/// <summary>
/// Comprehensive unit tests for WinDetector functionality
/// Tests horizontal, vertical, and diagonal win detection across all board sizes
/// </summary>
public class WinDetectorUnitTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private WinDetector winDetector;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null && gameManager.winDetector != null)
        {
            winDetector = gameManager.winDetector;
            RunWinDetectorTests();
        }
        else
        {
            Debug.LogError("WinDetectorUnitTest: Could not find GameManager or WinDetector");
        }
    }

    private void RunWinDetectorTests()
    {
        Debug.Log("Starting WinDetector Unit Tests");

        // Test 1: Horizontal Win Detection
        TestHorizontalWin();

        // Test 2: Vertical Win Detection
        TestVerticalWin();

        // Test 3: Diagonal Win Detection
        TestDiagonalWins();

        // Test 4: Draw Detection
        TestDrawDetection();

        // Test 5: Edge Cases
        TestEdgeCases();

        Debug.Log("WinDetector Unit Tests Completed");
    }

    private void TestHorizontalWin()
    {
        Debug.Log("Testing Horizontal Win Detection");

        bool allTestsPassed = true;

        // Test for each board size
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();

            // Test horizontal win in middle of board
            bool horizontalWin = TestHorizontalWinForSize(size, size / 2);
            
            // Test horizontal win near top
            bool topHorizontalWin = TestHorizontalWinForSize(size, 0);
            
            // Test horizontal win near bottom
            bool bottomHorizontalWin = TestHorizontalWinForSize(size, size - 1);

            bool sizePassed = horizontalWin && topHorizontalWin && bottomHorizontalWin;
            allTestsPassed &= sizePassed;

            Debug.Log($"Horizontal Win Tests for size {size}: Middle={horizontalWin}, Top={topHorizontalWin}, Bottom={bottomHorizontalWin}");
        }

        Debug.Log($"All Horizontal Win Tests: {allTestsPassed}");
    }

    private bool TestHorizontalWinForSize(int size, int row)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 consecutive pieces horizontally
        int startX = Mathf.Max(0, (size / 2) - 2);
        
        // Ensure we have enough space
        if (startX + 5 > size)
            startX = size - 5;

        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(startX + i, row, GameManager.Player.Black);
        }

        // Check win on the last piece placed
        return gameManager.CheckWin(startX + 4, row);
    }

    private void TestVerticalWin()
    {
        Debug.Log("Testing Vertical Win Detection");

        bool allTestsPassed = true;

        // Test for each board size
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();

            // Test vertical win in middle of board
            bool verticalWin = TestVerticalWinForSize(size, size / 2);
            
            // Test vertical win near left
            bool leftVerticalWin = TestVerticalWinForSize(size, 0);
            
            // Test vertical win near right
            bool rightVerticalWin = TestVerticalWinForSize(size, size - 1);

            bool sizePassed = verticalWin && leftVerticalWin && rightVerticalWin;
            allTestsPassed &= sizePassed;

            Debug.Log($"Vertical Win Tests for size {size}: Middle={verticalWin}, Left={leftVerticalWin}, Right={rightVerticalWin}");
        }

        Debug.Log($"All Vertical Win Tests: {allTestsPassed}");
    }

    private bool TestVerticalWinForSize(int size, int col)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 consecutive pieces vertically
        int startY = Mathf.Max(0, (size / 2) - 2);
        
        // Ensure we have enough space
        if (startY + 5 > size)
            startY = size - 5;

        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(col, startY + i, GameManager.Player.White);
        }

        // Check win on the last piece placed
        return gameManager.CheckWin(col, startY + 4);
    }

    private void TestDiagonalWins()
    {
        Debug.Log("Testing Diagonal Win Detection");

        bool allTestsPassed = true;

        // Test for each board size
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();

            // Test diagonal (/) win
            bool diagonal1Win = TestDiagonal1WinForSize(size);
            
            // Test diagonal (\) win
            bool diagonal2Win = TestDiagonal2WinForSize(size);

            bool sizePassed = diagonal1Win && diagonal2Win;
            allTestsPassed &= sizePassed;

            Debug.Log($"Diagonal Win Tests for size {size}: Diagonal1={diagonal1Win}, Diagonal2={diagonal2Win}");
        }

        Debug.Log($"All Diagonal Win Tests: {allTestsPassed}");
    }

    private bool TestDiagonal1WinForSize(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 consecutive pieces diagonally (/)
        int startX = Mathf.Max(0, (size / 2) - 2);
        int startY = Mathf.Max(0, (size / 2) - 2);
        
        // Ensure we have enough space
        if (startX + 5 > size || startY + 5 > size)
        {
            startX = size - 5;
            startY = size - 5;
        }

        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(startX + i, startY + i, GameManager.Player.Black);
        }

        // Check win on the last piece placed
        return gameManager.CheckWin(startX + 4, startY + 4);
    }

    private bool TestDiagonal2WinForSize(int size)
    {
        // Clear board
        gameManager.boardManager.ClearBoard();

        // Place 5 consecutive pieces diagonally (\)
        int startX = Mathf.Max(0, (size / 2) - 2);
        int startY = Mathf.Min(size - 1, (size / 2) + 2);
        
        // Ensure we have enough space
        if (startX + 5 > size || startY - 4 < 0)
        {
            startX = size - 5;
            startY = 4;
        }

        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(startX + i, startY - i, GameManager.Player.White);
        }

        // Check win on the last piece placed
        return gameManager.CheckWin(startX + 4, startY - 4);
    }

    private void TestDrawDetection()
    {
        Debug.Log("Testing Draw Detection");

        // Test with a small board that can be filled
        gameManager.SetBoardSize(5);
        gameManager.StartNewGame();

        // Fill the board without creating a win
        bool drawDetected = false;
        
        // Fill board with alternating pieces
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                GameManager.Player player = ((x + y) % 2 == 0) ? GameManager.Player.Black : GameManager.Player.White;
                gameManager.boardManager.PlacePiece(x, y, player);
            }
        }

        // Check if draw is detected
        drawDetected = gameManager.CheckDraw();

        Debug.Log($"Draw Detection Test: {drawDetected}");
    }

    private void TestEdgeCases()
    {
        Debug.Log("Testing Edge Cases");

        // Test 4 in a row (should not win)
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();

        // Place 4 pieces horizontally
        for (int i = 0; i < 4; i++)
        {
            gameManager.boardManager.PlacePiece(i, 7, GameManager.Player.Black);
        }

        bool fourInRowNoWin = !gameManager.CheckWin(3, 7);
        Debug.Log($"4 in a row should not win: {fourInRowNoWin}");

        // Test interrupted sequence
        gameManager.boardManager.ClearBoard();
        
        // Place 3 pieces, then opponent, then 2 more
        gameManager.boardManager.PlacePiece(0, 7, GameManager.Player.Black);
        gameManager.boardManager.PlacePiece(1, 7, GameManager.Player.Black);
        gameManager.boardManager.PlacePiece(2, 7, GameManager.Player.Black);
        gameManager.boardManager.PlacePiece(3, 7, GameManager.Player.White); // Interrupt
        gameManager.boardManager.PlacePiece(4, 7, GameManager.Player.Black);
        gameManager.boardManager.PlacePiece(5, 7, GameManager.Player.Black);

        bool interruptedNoWin = !gameManager.CheckWin(5, 7);
        Debug.Log($"Interrupted sequence should not win: {interruptedNoWin}");
    }
}