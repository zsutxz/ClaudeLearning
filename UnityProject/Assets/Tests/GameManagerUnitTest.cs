using UnityEngine;
using GomokuGame.Core;

/// <summary>
/// Comprehensive unit tests for GameManager functionality
/// Tests state transitions, player switching, game lifecycle, and settings management
/// </summary>
public class GameManagerUnitTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            RunGameManagerTests();
        }
        else
        {
            Debug.LogError("GameManagerUnitTest: Could not find GameManager");
        }
    }

    private void RunGameManagerTests()
    {
        Debug.Log("Starting GameManager Unit Tests");

        // Test 1: Game State Management
        TestGameStateManagement();

        // Test 2: Player Switching
        TestPlayerSwitching();

        // Test 3: Game Lifecycle
        TestGameLifecycle();

        // Test 4: Settings Management
        TestSettingsManagement();

        // Test 5: Win/Draw Declarations
        TestWinDrawDeclarations();

        Debug.Log("GameManager Unit Tests Completed");
    }

    private void TestGameStateManagement()
    {
        Debug.Log("Testing Game State Management");

        // Test initial state
        bool initialState = gameManager.gameState == GameManager.GameState.MainMenu;
        Debug.Log($"Initial state is MainMenu: {initialState}");

        // Test state transitions
        gameManager.StartNewGame();
        bool playingState = gameManager.gameState == GameManager.GameState.Playing;
        Debug.Log($"After StartNewGame, state is Playing: {playingState}");

        gameManager.EndGame();
        bool gameOverState = gameManager.gameState == GameManager.GameState.GameOver;
        Debug.Log($"After EndGame, state is GameOver: {gameOverState}");

        gameManager.ReturnToMainMenu();
        bool backToMainMenu = gameManager.gameState == GameManager.GameState.MainMenu;
        Debug.Log($"After ReturnToMainMenu, state is MainMenu: {backToMainMenu}");

        bool allStatesValid = initialState && playingState && gameOverState && backToMainMenu;
        Debug.Log($"All state transitions: {allStatesValid}");
    }

    private void TestPlayerSwitching()
    {
        Debug.Log("Testing Player Switching");

        gameManager.StartNewGame();

        // Test initial player
        bool initialPlayer = gameManager.currentPlayer == GameManager.Player.Black;
        Debug.Log($"Initial player is Black: {initialPlayer}");

        // Test player switching
        gameManager.SwitchPlayer();
        bool switchedToWhite = gameManager.currentPlayer == GameManager.Player.White;
        Debug.Log($"After first switch, player is White: {switchedToWhite}");

        gameManager.SwitchPlayer();
        bool switchedBackToBlack = gameManager.currentPlayer == GameManager.Player.Black;
        Debug.Log($"After second switch, player is Black: {switchedBackToBlack}");

        // Test multiple switches
        for (int i = 0; i < 10; i++)
        {
            GameManager.Player previousPlayer = gameManager.currentPlayer;
            gameManager.SwitchPlayer();
            GameManager.Player currentPlayer = gameManager.currentPlayer;
            
            bool validSwitch = (previousPlayer == GameManager.Player.Black && currentPlayer == GameManager.Player.White) ||
                              (previousPlayer == GameManager.Player.White && currentPlayer == GameManager.Player.Black);
            
            if (!validSwitch)
            {
                Debug.LogError($"Invalid player switch at iteration {i}: {previousPlayer} -> {currentPlayer}");
            }
        }

        Debug.Log($"Multiple player switches completed successfully");
    }

    private void TestGameLifecycle()
    {
        Debug.Log("Testing Game Lifecycle");

        // Test complete game lifecycle
        bool lifecycleValid = true;

        // Start from main menu
        gameManager.ReturnToMainMenu();
        lifecycleValid &= gameManager.gameState == GameManager.GameState.MainMenu;

        // Start new game
        gameManager.StartNewGame();
        lifecycleValid &= gameManager.gameState == GameManager.GameState.Playing;
        lifecycleValid &= gameManager.currentPlayer == GameManager.Player.Black;

        // Make some moves
        gameManager.boardManager.PlacePiece(0, 0, gameManager.currentPlayer);
        gameManager.SwitchPlayer();
        gameManager.boardManager.PlacePiece(1, 1, gameManager.currentPlayer);

        // End game
        gameManager.EndGame();
        lifecycleValid &= gameManager.gameState == GameManager.GameState.GameOver;
        NewMethod();
        lifecycleValid &= gameManager.gameState == GameManager.GameState.Playing;
        lifecycleValid &= gameManager.currentPlayer == GameManager.Player.Black;

        // Return to main menu
        gameManager.ReturnToMainMenu();
        lifecycleValid &= gameManager.gameState == GameManager.GameState.MainMenu;

        Debug.Log($"Complete game lifecycle: {lifecycleValid}");
    }

    private void NewMethod()
    {

        // Restart game
        gameManager.RestartGame();
    }

    private void TestSettingsManagement()
    {
        Debug.Log("Testing Settings Management");

        bool settingsValid = true;

        // Test board size settings
        int[] testSizes = { 9, 13, 15, 19 };
        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            int retrievedSize = gameManager.GetBoardSize();
            settingsValid &= retrievedSize == size;
            Debug.Log($"Board size {size}: Set={size}, Retrieved={retrievedSize}, Match={retrievedSize == size}");
        }

        // Test win condition settings
        GameManager.WinConditionType[] winConditions = {
            GameManager.WinConditionType.FiveInRow,
            GameManager.WinConditionType.FiveInRowNoOverlines,
            GameManager.WinConditionType.FiveInRowFreeThree
        };

        foreach (var condition in winConditions)
        {
            gameManager.SetWinConditionType(condition.ToString());
            var retrievedCondition = gameManager.GetWinConditionType();
            settingsValid &= retrievedCondition == condition.ToString();
            Debug.Log($"Win condition {condition}: Set={condition}, Retrieved={retrievedCondition}, Match={retrievedCondition == condition.ToString()}");
        }

        // Test that settings persist across game restarts
        gameManager.SetBoardSize(13);
        gameManager.SetWinConditionType(GameManager.WinConditionType.FiveInRowNoOverlines.ToString());
        
        gameManager.StartNewGame();
        
        bool settingsPersist = gameManager.GetBoardSize() == 13 &&
                              gameManager.GetWinConditionType() == GameManager.WinConditionType.FiveInRowNoOverlines.ToString();
        
        Debug.Log($"Settings persist across game restarts: {settingsPersist}");
        settingsValid &= settingsPersist;

        Debug.Log($"All settings management tests: {settingsValid}");
    }

    private void TestWinDrawDeclarations()
    {
        Debug.Log("Testing Win/Draw Declarations");

        // Test win declaration
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();

        // Create a win condition
        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(i, 7, GameManager.Player.Black);
        }

        // Check win
        bool winDetected = gameManager.CheckWin(4, 7);
        Debug.Log($"Win detected: {winDetected}");

        // Test draw declaration
        gameManager.SetBoardSize(5);
        gameManager.StartNewGame();

        // Fill board without win
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                GameManager.Player player = ((x + y) % 2 == 0) ? GameManager.Player.Black : GameManager.Player.White;
                gameManager.boardManager.PlacePiece(x, y, player);
            }
        }

        bool drawDetected = gameManager.CheckDraw();
        Debug.Log($"Draw detected: {drawDetected}");

        // Test that win takes precedence over draw
        gameManager.SetBoardSize(5);
        gameManager.StartNewGame();

        // Create win condition
        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(i, 0, GameManager.Player.Black);
        }

        bool winPrecedence = gameManager.CheckWin(4, 0) && !gameManager.CheckDraw();
        Debug.Log($"Win takes precedence over draw: {winPrecedence}");
    }
}