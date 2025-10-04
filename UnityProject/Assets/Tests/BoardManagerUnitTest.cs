using UnityEngine;
using GomokuGame.Core;

/// <summary>
/// Comprehensive unit tests for BoardManager functionality
/// Tests initialization, piece placement, position validation, and board operations
/// </summary>
public class BoardManagerUnitTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private BoardManager boardManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null && gameManager.boardManager != null)
        {
            boardManager = gameManager.boardManager;
            RunBoardManagerTests();
        }
        else
        {
            Debug.LogError("BoardManagerUnitTest: Could not find GameManager or BoardManager");
        }
    }

    private void RunBoardManagerTests()
    {
        Debug.Log("Starting BoardManager Unit Tests");

        // Test 1: Board Initialization
        TestBoardInitialization();

        // Test 2: Piece Placement
        TestPiecePlacement();

        // Test 3: Position Validation
        TestPositionValidation();

        // Test 4: Board Operations
        TestBoardOperations();

        // Test 5: Edge Cases
        TestEdgeCases();

        Debug.Log("BoardManager Unit Tests Completed");
    }

    private void TestBoardInitialization()
    {
        Debug.Log("Testing Board Initialization");

        // Test different board sizes
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();

            // Verify board is properly initialized
            bool boardInitialized = boardManager != null;
            bool correctSize = boardManager.BoardSize == size;
            bool boardEmpty = IsBoardEmpty();

            Debug.Log($"Board size {size}: Initialized={boardInitialized}, CorrectSize={correctSize}, Empty={boardEmpty}");

            if (!boardInitialized || !correctSize || !boardEmpty)
            {
                Debug.LogError($"Board initialization failed for size {size}");
            }
        }
    }

    private void TestPiecePlacement()
    {
        Debug.Log("Testing Piece Placement");

        // Set up a standard board
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();

        // Test valid placements
        bool placement1 = boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        bool placement2 = boardManager.PlacePiece(7, 7, GameManager.Player.White);
        bool placement3 = boardManager.PlacePiece(14, 14, GameManager.Player.Black);

        // Verify placements
        bool verify1 = boardManager.GetPieceAt(0, 0) == GameManager.Player.Black;
        bool verify2 = boardManager.GetPieceAt(7, 7) == GameManager.Player.White;
        bool verify3 = boardManager.GetPieceAt(14, 14) == GameManager.Player.Black;

        Debug.Log($"Placement Tests: {placement1 && placement2 && placement3}");
        Debug.Log($"Verification Tests: {verify1 && verify2 && verify3}");

        // Test placing on occupied position
        bool occupiedPlacement = boardManager.PlacePiece(0, 0, GameManager.Player.White);
        Debug.Log($"Occupied placement should fail: {!occupiedPlacement}");
    }

    private void TestPositionValidation()
    {
        Debug.Log("Testing Position Validation");

        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();

        // Test valid positions
        bool valid1 = boardManager.IsPositionEmpty(0, 0);
        bool valid2 = boardManager.IsPositionEmpty(7, 7);
        bool valid3 = boardManager.IsPositionEmpty(14, 14);

        // Test invalid positions
        bool invalid1 = !boardManager.PlacePiece(-1, 0, GameManager.Player.Black);
        bool invalid2 = !boardManager.PlacePiece(0, -1, GameManager.Player.Black);
        bool invalid3 = !boardManager.PlacePiece(15, 0, GameManager.Player.Black);
        bool invalid4 = !boardManager.PlacePiece(0, 15, GameManager.Player.Black);

        // Test getting pieces at invalid positions
        GameManager.Player piece1 = boardManager.GetPieceAt(-1, 0);
        GameManager.Player piece2 = boardManager.GetPieceAt(0, -1);
        GameManager.Player piece3 = boardManager.GetPieceAt(15, 0);
        GameManager.Player piece4 = boardManager.GetPieceAt(0, 15);

        bool allNone = (piece1 == GameManager.Player.None) &&
                      (piece2 == GameManager.Player.None) &&
                      (piece3 == GameManager.Player.None) &&
                      (piece4 == GameManager.Player.None);

        Debug.Log($"Position Validation Tests: Valid={valid1 && valid2 && valid3}, Invalid={invalid1 && invalid2 && invalid3 && invalid4}, AllNone={allNone}");
    }

    private void TestBoardOperations()
    {
        Debug.Log("Testing Board Operations");

        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();

        // Place some pieces
        boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        boardManager.PlacePiece(7, 7, GameManager.Player.White);
        boardManager.PlacePiece(14, 14, GameManager.Player.Black);

        // Test ClearBoard
        boardManager.ClearBoard();
        bool boardCleared = IsBoardEmpty();

        Debug.Log($"Board Clear Test: {boardCleared}");

        // Test that we can place pieces again after clearing
        bool newPlacement1 = boardManager.PlacePiece(1, 1, GameManager.Player.Black);
        bool newPlacement2 = boardManager.PlacePiece(2, 2, GameManager.Player.White);

        Debug.Log($"Post-clear placements: {newPlacement1 && newPlacement2}");
    }

    private void TestEdgeCases()
    {
        Debug.Log("Testing Edge Cases");

        // Test smallest board
        gameManager.SetBoardSize(9);
        gameManager.StartNewGame();

        bool smallBoardPlacements = true;
        // Try to place pieces in all corners
        smallBoardPlacements &= boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        smallBoardPlacements &= boardManager.PlacePiece(8, 0, GameManager.Player.White);
        smallBoardPlacements &= boardManager.PlacePiece(0, 8, GameManager.Player.Black);
        smallBoardPlacements &= boardManager.PlacePiece(8, 8, GameManager.Player.White);

        Debug.Log($"Small board edge cases: {smallBoardPlacements}");

        // Test largest board
        gameManager.SetBoardSize(19);
        gameManager.StartNewGame();

        bool largeBoardPlacements = true;
        // Try to place pieces in all corners
        largeBoardPlacements &= boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        largeBoardPlacements &= boardManager.PlacePiece(18, 0, GameManager.Player.White);
        largeBoardPlacements &= boardManager.PlacePiece(0, 18, GameManager.Player.Black);
        largeBoardPlacements &= boardManager.PlacePiece(18, 18, GameManager.Player.White);

        Debug.Log($"Large board edge cases: {largeBoardPlacements}");
    }

    private bool IsBoardEmpty()
    {
        if (boardManager == null) return false;

        for (int x = 0; x < boardManager.BoardSize; x++)
        {
            for (int y = 0; y < boardManager.BoardSize; y++)
            {
                if (boardManager.GetPieceAt(x, y) != GameManager.Player.None)
                {
                    return false;
                }
            }
        }
        return true;
    }
}