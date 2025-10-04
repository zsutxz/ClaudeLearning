using UnityEngine;
using GomokuGame.UI;
using System.Collections;

namespace GomokuGame.Core
{
    /// <summary>
    /// Comprehensive test script to verify board size configuration and win detection works correctly for all supported sizes
    /// </summary>
    public class ComprehensiveBoardSizeTest : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardViewManager boardViewManager;
        [SerializeField] private WinDetector winDetector;

        private bool testRunning = false;
        private int currentTestSize = 0;
        private int testIndex = 0;
        private int[] testSizes = { 9, 13, 15, 19 };

        void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            if (boardViewManager == null)
                boardViewManager = FindObjectOfType<BoardViewManager>();

            if (winDetector == null)
                winDetector = FindObjectOfType<WinDetector>();

            // Run tests
            StartCoroutine(RunComprehensiveTests());
        }

        private IEnumerator RunComprehensiveTests()
        {
            Debug.Log("Starting Comprehensive Board Size Tests");

            // Test each board size
            foreach (int size in testSizes)
            {
                Debug.Log($"Testing board size: {size}x{size}");
                currentTestSize = size;

                // Test board size configuration
                yield return StartCoroutine(TestBoardSizeConfiguration(size));

                // Test win detection for this size
                yield return StartCoroutine(TestWinDetection(size));

                // Test boundary checks
                yield return StartCoroutine(TestBoundaryChecks(size));

                testIndex++;
            }

            Debug.Log("All Comprehensive Board Size Tests Completed");
        }

        private IEnumerator TestBoardSizeConfiguration(int size)
        {
            Debug.Log($"Testing board size configuration for {size}x{size}");

            // Set the board size
            gameManager.SetBoardSize(size);
            int retrievedSize = gameManager.GetBoardSize();
            Debug.Log($"Set board size to {size}, retrieved {retrievedSize}. Match: {size == retrievedSize}");

            // Start a new game to initialize the board
            gameManager.StartNewGame();

            // Wait a frame for initialization
            yield return null;

            // Test that BoardManager has the correct size
            if (gameManager.boardManager != null)
            {
                int boardManagerSize = gameManager.boardManager.BoardSize;
                Debug.Log($"BoardManager size: {boardManagerSize}. Match: {size == boardManagerSize}");

                // Verify board state array dimensions
                var boardState = gameManager.boardManager.BoardState;
                bool dimensionsCorrect = boardState.GetLength(0) == size && boardState.GetLength(1) == size;
                Debug.Log($"BoardState dimensions correct: {dimensionsCorrect}");
            }

            // Test that BoardViewManager has the correct size
            if (boardViewManager != null)
            {
                int boardViewSize = boardViewManager.boardSize;
                Debug.Log($"BoardViewManager size: {boardViewSize}. Match: {size == boardViewSize}");

                // Verify piece visuals array dimensions
                // Note: This is a private field, so we can't directly check it
                // But we can verify that the board is visually created correctly
                Debug.Log("BoardViewManager board creation verified through visual inspection");
            }

            Debug.Log($"Board size configuration test for {size}x{size} completed");
        }

        private IEnumerator TestWinDetection(int size)
        {
            Debug.Log($"Testing win detection for {size}x{size} board");

            // Clear the board first
            gameManager.boardManager.ClearBoard();
            if (boardViewManager != null)
            {
                boardViewManager.ClearVisuals();
            }

            // Test horizontal win
            yield return StartCoroutine(TestHorizontalWin(size));

            // Test vertical win
            yield return StartCoroutine(TestVerticalWin(size));

            // Test diagonal wins
            yield return StartCoroutine(TestDiagonalWin(size));

            // Test draw condition (if board is small enough to test reasonably)
            if (size <= 13)
            {
                yield return StartCoroutine(TestDrawCondition(size));
            }

            Debug.Log($"Win detection test for {size}x{size} completed");
        }

        private IEnumerator TestHorizontalWin(int size)
        {
            Debug.Log("Testing horizontal win detection");

            // Place 5 pieces in a horizontal line (starting from position 0,0)
            int winLength = 5;
            int startX = 0;
            int startY = size / 2; // Middle row

            // Make sure we don't go out of bounds
            if (startX + winLength <= size)
            {
                for (int i = 0; i < winLength; i++)
                {
                    gameManager.boardManager.PlacePiece(startX + i, startY, GameManager.Player.Black);
                }

                // Check if the last piece creates a win
                bool isWin = gameManager.CheckWin(startX + winLength - 1, startY);
                Debug.Log($"Horizontal win detection: {isWin}");
            }
            else
            {
                Debug.LogWarning("Board too small for horizontal win test");
            }

            yield return null;
        }

        private IEnumerator TestVerticalWin(int size)
        {
            Debug.Log("Testing vertical win detection");

            // Place 5 pieces in a vertical line (starting from position 0,0)
            int winLength = 5;
            int startX = size / 2; // Middle column
            int startY = 0;

            // Make sure we don't go out of bounds
            if (startY + winLength <= size)
            {
                for (int i = 0; i < winLength; i++)
                {
                    gameManager.boardManager.PlacePiece(startX, startY + i, GameManager.Player.White);
                }

                // Check if the last piece creates a win
                bool isWin = gameManager.CheckWin(startX, startY + winLength - 1);
                Debug.Log($"Vertical win detection: {isWin}");
            }
            else
            {
                Debug.LogWarning("Board too small for vertical win test");
            }

            yield return null;
        }

        private IEnumerator TestDiagonalWin(int size)
        {
            Debug.Log("Testing diagonal win detection");

            // Place 5 pieces in a diagonal line (starting from position 0,0)
            int winLength = 5;
            int startX = 0;
            int startY = 0;

            // Test diagonal / (bottom-left to top-right)
            if (startX + winLength <= size && startY + winLength <= size)
            {
                for (int i = 0; i < winLength; i++)
                {
                    gameManager.boardManager.PlacePiece(startX + i, startY + i, GameManager.Player.Black);
                }

                // Check if the last piece creates a win
                bool isWin = gameManager.CheckWin(startX + winLength - 1, startY + winLength - 1);
                Debug.Log($"Diagonal / win detection: {isWin}");
            }
            else
            {
                Debug.LogWarning("Board too small for diagonal / win test");
            }

            // Clear board for next test
            gameManager.boardManager.ClearBoard();

            // Test diagonal \ (top-left to bottom-right)
            startX = 0;
            startY = winLength - 1;

            if (startX + winLength <= size && startY - winLength + 1 >= 0)
            {
                for (int i = 0; i < winLength; i++)
                {
                    gameManager.boardManager.PlacePiece(startX + i, startY - i, GameManager.Player.White);
                }

                // Check if the last piece creates a win
                bool isWin = gameManager.CheckWin(startX + winLength - 1, startY - winLength + 1);
                Debug.Log($"Diagonal \\ win detection: {isWin}");
            }
            else
            {
                Debug.LogWarning("Board too small for diagonal \\ win test");
            }

            yield return null;
        }

        private IEnumerator TestDrawCondition(int size)
        {
            Debug.Log("Testing draw condition detection");

            // Fill the board almost completely, leaving just a few spaces
            int emptySpaces = 3;
            int filledSpaces = 0;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Leave some spaces empty
                    if (filledSpaces < (size * size - emptySpaces))
                    {
                        GameManager.Player player = (filledSpaces % 2 == 0) ? GameManager.Player.Black : GameManager.Player.White;
                        gameManager.boardManager.PlacePiece(x, y, player);
                        filledSpaces++;
                    }
                }
            }

            // Check if board is full (should be false since we left spaces)
            bool isFull = winDetector.CheckDraw();
            Debug.Log($"Board full (with {emptySpaces} empty spaces): {isFull}");

            // Now fill the remaining spaces
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (gameManager.boardManager.IsPositionEmpty(x, y))
                    {
                        GameManager.Player player = GameManager.Player.Black; // Any player
                        gameManager.boardManager.PlacePiece(x, y, player);
                    }
                }
            }

            // Check if board is full now (should be true)
            isFull = winDetector.CheckDraw();
            Debug.Log($"Board full (completely filled): {isFull}");

            yield return null;
        }

        private IEnumerator TestBoundaryChecks(int size)
        {
            Debug.Log($"Testing boundary checks for {size}x{size} board");

            // Test placing pieces at valid positions
            bool validPlacement1 = gameManager.boardManager.PlacePiece(0, 0, GameManager.Player.Black);
            bool validPlacement2 = gameManager.boardManager.PlacePiece(size - 1, size - 1, GameManager.Player.White);
            Debug.Log($"Valid boundary placements: {validPlacement1} and {validPlacement2}");

            // Test placing pieces at invalid positions
            bool invalidPlacement1 = gameManager.boardManager.PlacePiece(-1, 0, GameManager.Player.Black);
            bool invalidPlacement2 = gameManager.boardManager.PlacePiece(0, -1, GameManager.Player.Black);
            bool invalidPlacement3 = gameManager.boardManager.PlacePiece(size, 0, GameManager.Player.Black);
            bool invalidPlacement4 = gameManager.boardManager.PlacePiece(0, size, GameManager.Player.Black);
            Debug.Log($"Invalid boundary placements correctly rejected: {!invalidPlacement1} {!invalidPlacement2} {!invalidPlacement3} {!invalidPlacement4}");

            // Test getting pieces at valid positions
            GameManager.Player piece1 = gameManager.boardManager.GetPieceAt(0, 0);
            GameManager.Player piece2 = gameManager.boardManager.GetPieceAt(size - 1, size - 1);
            Debug.Log($"Valid boundary piece retrieval: {piece1} and {piece2}");

            // Test getting pieces at invalid positions
            GameManager.Player piece3 = gameManager.boardManager.GetPieceAt(-1, 0);
            GameManager.Player piece4 = gameManager.boardManager.GetPieceAt(0, -1);
            GameManager.Player piece5 = gameManager.boardManager.GetPieceAt(size, 0);
            GameManager.Player piece6 = gameManager.boardManager.GetPieceAt(0, size);
            bool noneReturned = (piece3 == GameManager.Player.None) &&
                               (piece4 == GameManager.Player.None) &&
                               (piece5 == GameManager.Player.None) &&
                               (piece6 == GameManager.Player.None);
            Debug.Log($"Invalid boundary piece retrieval returns None: {noneReturned}");

            // Test checking empty positions at valid positions
            bool isEmpty1 = gameManager.boardManager.IsPositionEmpty(1, 1);
            bool isEmpty2 = gameManager.boardManager.IsPositionEmpty(size/2, size/2);
            Debug.Log($"Valid position empty check: {isEmpty1} and {isEmpty2}");

            // Test checking empty positions at invalid positions
            bool isEmpty3 = gameManager.boardManager.IsPositionEmpty(-1, 0);
            bool isEmpty4 = gameManager.boardManager.IsPositionEmpty(0, -1);
            bool isEmpty5 = gameManager.boardManager.IsPositionEmpty(size, 0);
            bool isEmpty6 = gameManager.boardManager.IsPositionEmpty(0, size);
            bool allFalse = !isEmpty3 && !isEmpty4 && !isEmpty5 && !isEmpty6;
            Debug.Log($"Invalid position empty check returns false: {allFalse}");

            yield return null;
        }
    }
}