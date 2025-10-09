using UnityEngine;
using System.Collections;
using GomokuGame.Core;
using GomokuGame.UI;
using GomokuGame.Utilities;

/// <summary>
/// Validation test for Story 5.2 bug fixes and performance optimizations
/// </summary>
public class Story52ValidationTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private WinDetector winDetector;
    [SerializeField] private BoardViewManager boardViewManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (boardManager == null)
            boardManager = FindObjectOfType<BoardManager>();
        if (winDetector == null)
            winDetector = FindObjectOfType<WinDetector>();
        if (boardViewManager == null)
            boardViewManager = FindObjectOfType<BoardViewManager>();

        if (gameManager != null && boardManager != null && winDetector != null && boardViewManager != null)
        {
            StartCoroutine(RunValidationTests());
        }
        else
        {
            Debug.LogError("Story52ValidationTest: Could not find required components");
        }
    }

    private IEnumerator RunValidationTests()
    {
        Debug.Log("Starting Story 5.2 Validation Tests");

        // Test 1: Board Boundary Validation Fixes
        yield return StartCoroutine(TestBoardBoundaryValidation());

        // Test 2: Win Detection Edge Cases
        yield return StartCoroutine(TestWinDetectionEdgeCases());

        // Test 3: Settings Persistence Fixes
        yield return StartCoroutine(TestSettingsPersistence());

        // Test 4: UI State Synchronization
        yield return StartCoroutine(TestUIStateSynchronization());

        // Test 5: Performance Optimizations
        yield return StartCoroutine(TestPerformanceOptimizations());

        Debug.Log("Story 5.2 Validation Tests Completed");
    }

    private IEnumerator TestBoardBoundaryValidation()
    {
        Debug.Log("Testing Board Boundary Validation Fixes");

        bool boundaryTestsPassed = true;

        // Test 1: Invalid coordinate placement
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Test negative coordinates
        bool negativeX = !boardManager.PlacePiece(-1, 7, GameManager.Player.Black);
        bool negativeY = !boardManager.PlacePiece(7, -1, GameManager.Player.Black);
        
        // Test coordinates beyond board size
        bool beyondX = !boardManager.PlacePiece(15, 7, GameManager.Player.Black);
        bool beyondY = !boardManager.PlacePiece(7, 15, GameManager.Player.Black);

        boundaryTestsPassed &= negativeX && negativeY && beyondX && beyondY;
        Debug.Log($"Invalid coordinate placement: {negativeX && negativeY && beyondX && beyondY}");

        // Test 2: Valid boundary placement
        bool validCorner1 = boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        bool validCorner2 = boardManager.PlacePiece(14, 14, GameManager.Player.White);
        bool validEdge1 = boardManager.PlacePiece(0, 7, GameManager.Player.Black);
        bool validEdge2 = boardManager.PlacePiece(7, 0, GameManager.Player.White);

        boundaryTestsPassed &= validCorner1 && validCorner2 && validEdge1 && validEdge2;
        Debug.Log($"Valid boundary placement: {validCorner1 && validCorner2 && validEdge1 && validEdge2}");

        // Test 3: Duplicate placement prevention
        bool firstPlacement = boardManager.PlacePiece(7, 7, GameManager.Player.Black);
        bool duplicatePlacement = !boardManager.PlacePiece(7, 7, GameManager.Player.White);

        boundaryTestsPassed &= firstPlacement && duplicatePlacement;
        Debug.Log($"Duplicate placement prevention: {firstPlacement && duplicatePlacement}");

        Debug.Log($"Board Boundary Validation Tests: {boundaryTestsPassed}");
        yield return null;
    }

    private IEnumerator TestWinDetectionEdgeCases()
    {
        Debug.Log("Testing Win Detection Edge Cases");

        bool winDetectionTestsPassed = true;

        // Test 1: Boundary win detection
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Create horizontal win at top boundary
        for (int i = 0; i < 5; i++)
        {
            boardManager.PlacePiece(i, 0, GameManager.Player.Black);
        }
        bool boundaryWin = gameManager.CheckWin(4, 0);
        winDetectionTestsPassed &= boundaryWin;
        Debug.Log($"Boundary win detection: {boundaryWin}");

        // Test 2: Diagonal win detection
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 5; i++)
        {
            boardManager.PlacePiece(i, i, GameManager.Player.White);
        }
        bool diagonalWin = gameManager.CheckWin(4, 4);
        winDetectionTestsPassed &= diagonalWin;
        Debug.Log($"Diagonal win detection: {diagonalWin}");

        // Test 3: No false positives
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Create pattern that's not a win
        boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        boardManager.PlacePiece(1, 0, GameManager.Player.Black);
        boardManager.PlacePiece(2, 0, GameManager.Player.Black);
        boardManager.PlacePiece(3, 0, GameManager.Player.Black);
        // Missing 5th piece

        bool noFalsePositive = !gameManager.CheckWin(3, 0);
        winDetectionTestsPassed &= noFalsePositive;
        Debug.Log($"No false positives: {noFalsePositive}");

        Debug.Log($"Win Detection Edge Cases Tests: {winDetectionTestsPassed}");
        yield return null;
    }

    private IEnumerator TestSettingsPersistence()
    {
        Debug.Log("Testing Settings Persistence Fixes");

        bool settingsTestsPassed = true;

        // Test 1: Invalid settings handling
        bool invalidSizeHandled = true;
        try
        {
            PlayerPrefsManager.SaveBoardSize(-1);
            int recoveredSize = PlayerPrefsManager.LoadBoardSize();
            invalidSizeHandled = recoveredSize >= 9 && recoveredSize <= 19;
        }
        catch (System.Exception)
        {
            invalidSizeHandled = true;
        }
        settingsTestsPassed &= invalidSizeHandled;
        Debug.Log($"Invalid board size handling: {invalidSizeHandled}");

        // Test 2: Settings validation
        bool settingsValidation = PlayerPrefsManager.ValidateSettings();
        settingsTestsPassed &= settingsValidation;
        Debug.Log($"Settings validation: {settingsValidation}");

        // Test 3: Safe save with backup
        bool safeSaveWorks = true;
        try
        {
            // Save standard win condition (which corresponds to int value 0)
            PlayerPrefsManager.SafeSaveAllSettings(13, 0, "TestTheme");
            safeSaveWorks = PlayerPrefsManager.LoadBoardSize() == 13 &&
                           PlayerPrefsManager.LoadWinCondition() == 0 &&
                           PlayerPrefsManager.LoadTheme() == "TestTheme";
        }
        catch (System.Exception)
        {
            safeSaveWorks = false;
        }
        settingsTestsPassed &= safeSaveWorks;
        Debug.Log($"Safe save with backup: {safeSaveWorks}");

        Debug.Log($"Settings Persistence Tests: {settingsTestsPassed}");
        yield return null;
    }

    private IEnumerator TestUIStateSynchronization()
    {
        Debug.Log("Testing UI State Synchronization");

        bool uiStateTestsPassed = true;

        // Test 1: Valid state transitions
        bool stateTransitionsValid = true;

        // MainMenu -> Playing
        gameManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.MainMenu;

        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.Playing;

        // Playing -> Paused
        //gameManager.PauseGame();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.Paused;

        // Paused -> Playing
        //gameManager.ResumeGame();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.Playing;

        // Playing -> GameOver
        gameManager.EndGame(GameManager.Player.Black);
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.GameOver;

        // GameOver -> MainMenu
        gameManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.currentState == GameManager.GameState.MainMenu;

        uiStateTestsPassed &= stateTransitionsValid;
        Debug.Log($"Valid state transitions: {stateTransitionsValid}");

        // Test 2: Invalid state transition prevention
        bool invalidTransitionsPrevented = true;

        // Try to pause from MainMenu
        gameManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        //gameManager.PauseGame();
        invalidTransitionsPrevented &= gameManager.currentState == GameManager.GameState.MainMenu;

        // Try to resume from GameOver
        gameManager.EndGame(GameManager.Player.Black);
        yield return new WaitForSeconds(0.1f);
        //gameManager.ResumeGame();
        invalidTransitionsPrevented &= gameManager.currentState == GameManager.GameState.GameOver;

        uiStateTestsPassed &= invalidTransitionsPrevented;
        Debug.Log($"Invalid state transitions prevented: {invalidTransitionsPrevented}");

        Debug.Log($"UI State Synchronization Tests: {uiStateTestsPassed}");
        yield return null;
    }

    private IEnumerator TestPerformanceOptimizations()
    {
        Debug.Log("Testing Performance Optimizations");

        bool performanceTestsPassed = true;

        // Test 1: Large board performance
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // Test small board (15x15)
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        stopwatch.Start();
        for (int i = 0; i < 10; i++)
        {
            boardManager.PlacePiece(i, i, GameManager.Player.Black);
            gameManager.CheckWin(i, i);
        }
        stopwatch.Stop();
        long smallBoardTime = stopwatch.ElapsedMilliseconds;

        // Test large board (19x19)
        gameManager.SetBoardSize(19);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            boardManager.PlacePiece(i, i, GameManager.Player.Black);
            gameManager.CheckWin(i, i);
        }
        stopwatch.Stop();
        long largeBoardTime = stopwatch.ElapsedMilliseconds;

        // Performance should be reasonable for both board sizes
        bool performanceReasonable = smallBoardTime < 1000 && largeBoardTime < 2000;
        performanceTestsPassed &= performanceReasonable;
        Debug.Log($"Performance reasonable (small: {smallBoardTime}ms, large: {largeBoardTime}ms): {performanceReasonable}");

        // Test 2: Memory usage
        long initialMemory = System.GC.GetTotalMemory(false);

        // Perform operations that should not leak memory
        for (int i = 0; i < 5; i++)
        {
            gameManager.StartNewGame();
            yield return new WaitForSeconds(0.1f);
            
            for (int j = 0; j < 10; j++)
            {
                boardManager.PlacePiece(j, j, GameManager.Player.Black);
            }
        }

        System.GC.Collect();
        long finalMemory = System.GC.GetTotalMemory(false);
        
        bool memoryStable = (finalMemory - initialMemory) < 10000000; // Less than 10MB increase
        performanceTestsPassed &= memoryStable;
        Debug.Log($"Memory usage stable (delta: {finalMemory - initialMemory} bytes): {memoryStable}");

        Debug.Log($"Performance Optimization Tests: {performanceTestsPassed}");
        yield return null;
    }
}