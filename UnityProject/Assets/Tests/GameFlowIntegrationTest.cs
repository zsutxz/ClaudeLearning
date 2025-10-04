using UnityEngine;
using System.Collections;
using GomokuGame.Core;
using GomokuGame.UI;

/// <summary>
/// Integration tests for complete game flow from main menu to game over
/// Tests UI interactions, state synchronization, and complete game lifecycle
/// </summary>
public class GameFlowIntegrationTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private ResultsScreenController resultsScreenController;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        if (mainMenuController == null)
            mainMenuController = FindObjectOfType<MainMenuController>();
        if (resultsScreenController == null)
            resultsScreenController = FindObjectOfType<ResultsScreenController>();

        if (gameManager != null && uiManager != null)
        {
            StartCoroutine(RunGameFlowTests());
        }
        else
        {
            Debug.LogError("GameFlowIntegrationTest: Could not find required components");
        }
    }

    private IEnumerator RunGameFlowTests()
    {
        Debug.Log("Starting Game Flow Integration Tests");

        // Test 1: Complete Game Lifecycle
        yield return StartCoroutine(TestCompleteGameLifecycle());

        // Test 2: UI State Synchronization
        yield return StartCoroutine(TestUIStateSynchronization());

        // Test 3: Settings Integration
        yield return StartCoroutine(TestSettingsIntegration());

        // Test 4: Win/Draw Scenarios
        yield return StartCoroutine(TestWinDrawScenarios());

        // Test 5: Game Restart and Reset
        yield return StartCoroutine(TestGameRestartReset());

        Debug.Log("Game Flow Integration Tests Completed");
    }

    private IEnumerator TestCompleteGameLifecycle()
    {
        Debug.Log("Testing Complete Game Lifecycle");

        bool lifecycleValid = true;

        // Start from main menu
        uiManager.ShowMainMenu();
        yield return new WaitForSeconds(0.1f);
        lifecycleValid &= gameManager.gameState == GameManager.GameState.MainMenu;
        Debug.Log($"Main menu state: {lifecycleValid}");

        // Start a new game
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        lifecycleValid &= gameManager.gameState == GameManager.GameState.Playing;
        Debug.Log($"Game started state: {lifecycleValid}");

        // Make some moves
        for (int i = 0; i < 3; i++)
        {
            gameManager.boardManager.PlacePiece(i, i, gameManager.currentPlayer);
            gameManager.SwitchPlayer();
            yield return new WaitForSeconds(0.05f);
        }

        // End game
        gameManager.EndGame(GameManager.Player.Black);
        yield return new WaitForSeconds(0.1f);
        lifecycleValid &= gameManager.gameState == GameManager.GameState.GameOver;
        Debug.Log($"Game over state: {lifecycleValid}");

        // Return to main menu
        uiManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        lifecycleValid &= gameManager.gameState == GameManager.GameState.MainMenu;
        Debug.Log($"Back to main menu: {lifecycleValid}");

        Debug.Log($"Complete game lifecycle: {lifecycleValid}");
    }

    private IEnumerator TestUIStateSynchronization()
    {
        Debug.Log("Testing UI State Synchronization");

        bool uiSyncValid = true;

        // Test main menu to game transition
        uiManager.ShowMainMenu();
        yield return new WaitForSeconds(0.1f);
        bool mainMenuVisible = GetMainMenuPanel() != null && uiManager.mainMenuPanel.activeInHierarchy;
        uiSyncValid &= mainMenuVisible;
        Debug.Log($"Main menu visible: {mainMenuVisible}");

        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        bool gameUIVisible = uiManager.gameUIPanel != null && uiManager.gameUIPanel.activeInHierarchy;
        uiSyncValid &= gameUIVisible;
        Debug.Log($"Game UI visible: {gameUIVisible}");

        // Test player text updates
        string initialPlayerText = uiManager.currentPlayerText != null ? uiManager.currentPlayerText.text : "";
        bool initialTextValid = !string.IsNullOrEmpty(initialPlayerText);
        uiSyncValid &= initialTextValid;
        Debug.Log($"Initial player text valid: {initialTextValid}");

        // Switch player and check text update
        gameManager.SwitchPlayer();
        yield return new WaitForSeconds(0.1f);
        string updatedPlayerText = GetCurrentPlayerText() != null ? uiManager.currentPlayerText.text : "";
        bool textUpdated = !string.IsNullOrEmpty(updatedPlayerText) && updatedPlayerText != initialPlayerText;
        uiSyncValid &= textUpdated;
        Debug.Log($"Player text updated: {textUpdated}");

        // Test game over UI
        gameManager.EndGame(GameManager.Player.White);
        yield return new WaitForSeconds(0.1f);
        bool gameOverVisible = uiManager.gameOverPanel != null && uiManager.gameOverPanel.activeInHierarchy;
        uiSyncValid &= gameOverVisible;
        Debug.Log($"Game over UI visible: {gameOverVisible}");

        Debug.Log($"All UI state synchronization: {uiSyncValid}");
    }

    private UnityEngine.UI.Text GetCurrentPlayerText()
    {
        return uiManager.currentPlayerText;
    }

    private GameObject GetMainMenuPanel()
    {
        return uiManager.mainMenuPanel;
    }

    private IEnumerator TestSettingsIntegration()
    {
        Debug.Log("Testing Settings Integration");

        bool settingsIntegrationValid = true;

        // Test settings menu
        uiManager.ShowSettings();
        yield return new WaitForSeconds(0.1f);
        bool settingsVisible = uiManager.settingsPanel != null && uiManager.settingsPanel.activeInHierarchy;
        settingsIntegrationValid &= settingsVisible;
        Debug.Log($"Settings menu visible: {settingsVisible}");

        // Test settings save
        int originalSize = gameManager.GetBoardSize();
        int testSize = originalSize == 15 ? 13 : 15; // Use different size
        
        gameManager.SetBoardSize(testSize);
        uiManager.SaveSettings();
        yield return new WaitForSeconds(0.1f);

        // Verify settings were saved
        int savedSize = gameManager.GetBoardSize();
        settingsIntegrationValid &= savedSize == testSize;
        Debug.Log($"Settings saved correctly: {savedSize == testSize}");

        // Test settings cancel
        uiManager.ShowSettings();
        yield return new WaitForSeconds(0.1f);
        
        gameManager.SetBoardSize(originalSize == 15 ? 13 : 15); // Change to different size
        uiManager.CancelSettings();
        yield return new WaitForSeconds(0.1f);

        // Verify settings were not saved
        int currentSize = gameManager.GetBoardSize();
        settingsIntegrationValid &= currentSize == testSize; // Should still be the previously saved size
        Debug.Log($"Settings cancel worked: {currentSize == testSize}");

        // Test that settings persist across game sessions
        uiManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        
        int newGameSize = gameManager.GetBoardSize();
        settingsIntegrationValid &= newGameSize == testSize;
        Debug.Log($"Settings persist across sessions: {newGameSize == testSize}");

        Debug.Log($"All settings integration tests: {settingsIntegrationValid}");
    }

    private IEnumerator TestWinDrawScenarios()
    {
        Debug.Log("Testing Win/Draw Scenarios");

        bool winDrawValid = true;

        // Test win scenario
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Create a win
        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(i, 7, GameManager.Player.Black);
        }

        bool winDetected = gameManager.CheckWin(4, 7);
        winDrawValid &= winDetected;
        Debug.Log($"Win detected: {winDetected}");

        // Verify game over state and UI
        yield return new WaitForSeconds(0.1f);
        bool gameOverAfterWin = gameManager.gameState == GameManager.GameState.GameOver;
        winDrawValid &= gameOverAfterWin;
        Debug.Log($"Game over after win: {gameOverAfterWin}");

        bool winUIVisible = uiManager.gameOverPanel != null && uiManager.gameOverPanel.activeInHierarchy;
        winDrawValid &= winUIVisible;
        Debug.Log($"Win UI visible: {winUIVisible}");

        // Test draw scenario
        gameManager.SetBoardSize(5);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Fill board without win
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                GameManager.Player player = ((x + y) % 2 == 0) ? GameManager.Player.Black : GameManager.Player.White;
                gameManager.boardManager.PlacePiece(x, y, player);
            }
        }

        // For draw detection, we'll manually check if the board is full
        bool boardFull = true;
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (gameManager.boardManager.GetPieceAt(x, y) == GameManager.Player.None)
                {
                    boardFull = false;
                    break;
                }
            }
            if (!boardFull) break;
        }

        bool drawDetected = boardFull && !gameManager.CheckWin(4, 4); // Board full but no win
        winDrawValid &= drawDetected;
        Debug.Log($"Draw detected: {drawDetected}");

        // Verify game over state and UI
        yield return new WaitForSeconds(0.1f);
        bool gameOverAfterDraw = gameManager.gameState == GameManager.GameState.GameOver;
        winDrawValid &= gameOverAfterDraw;
        Debug.Log($"Game over after draw: {gameOverAfterDraw}");

        bool drawUIVisible = uiManager.gameOverPanel != null && uiManager.gameOverPanel.activeInHierarchy;
        winDrawValid &= drawUIVisible;
        Debug.Log($"Draw UI visible: {drawUIVisible}");

        Debug.Log($"All win/draw scenario tests: {winDrawValid}");
    }

    private IEnumerator TestGameRestartReset()
    {
        Debug.Log("Testing Game Restart and Reset");

        bool restartValid = true;

        // Start a game and make some moves
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Make some moves
        for (int i = 0; i < 3; i++)
        {
            gameManager.boardManager.PlacePiece(i, i, gameManager.currentPlayer);
            gameManager.SwitchPlayer();
        }

        // Test restart game
        uiManager.RestartGame();
        yield return new WaitForSeconds(0.1f);

        bool restarted = gameManager.gameState == GameManager.GameState.Playing;
        restartValid &= restarted;
        Debug.Log($"Game restarted: {restarted}");

        // Verify board is cleared
        bool boardCleared = true;
        for (int x = 0; x < 15; x++)
        {
            for (int y = 0; y < 15; y++)
            {
                if (gameManager.boardManager.GetPieceAt(x, y) != GameManager.Player.None)
                {
                    boardCleared = false;
                    break;
                }
            }
        }
        restartValid &= boardCleared;
        Debug.Log($"Board cleared on restart: {boardCleared}");

        // Verify player reset to Black
        bool playerReset = gameManager.currentPlayer == GameManager.Player.Black;
        restartValid &= playerReset;
        Debug.Log($"Player reset to Black: {playerReset}");

        // Test return to main menu and new game
        uiManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);

        bool newGameStarted = gameManager.gameState == GameManager.GameState.Playing;
        restartValid &= newGameStarted;
        Debug.Log($"New game started from menu: {newGameStarted}");

        Debug.Log($"All game restart/reset tests: {restartValid}");
    }
}