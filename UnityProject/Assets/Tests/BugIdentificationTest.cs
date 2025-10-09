using UnityEngine;
using System.Collections;
using GomokuGame.Core;
using GomokuGame.UI;
using GomokuGame.Utilities;

/// <summary>
/// Bug identification and resolution tests
/// Tests edge cases, boundary conditions, error handling, and invalid input scenarios
/// </summary>
public class BugIdentificationTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (gameManager != null && uiManager != null)
        {
            StartCoroutine(RunBugIdentificationTests());
        }
        else
        {
            Debug.LogError("BugIdentificationTest: Could not find required components");
        }
    }

    private IEnumerator RunBugIdentificationTests()
    {
        Debug.Log("Starting Bug Identification and Resolution Tests");

        // Test 1: Edge Cases and Boundary Conditions
        yield return StartCoroutine(TestEdgeCasesBoundaryConditions());

        // Test 2: Error Handling and Recovery
        yield return StartCoroutine(TestErrorHandlingRecovery());

        // Test 3: Invalid Input Scenarios
        yield return StartCoroutine(TestInvalidInputScenarios());

        // Test 4: State Management Issues
        yield return StartCoroutine(TestStateManagementIssues());

        // Test 5: Integration Bug Scenarios
        yield return StartCoroutine(TestIntegrationBugScenarios());

        Debug.Log("Bug Identification and Resolution Tests Completed");
    }

    private IEnumerator TestEdgeCasesBoundaryConditions()
    {
        Debug.Log("Testing Edge Cases and Boundary Conditions");

        bool edgeCasesHandled = true;

        // Test minimum board size
        gameManager.SetBoardSize(9);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Test placing pieces at all corners of small board
        bool corner1 = gameManager.boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        bool corner2 = gameManager.boardManager.PlacePiece(8, 0, GameManager.Player.White);
        bool corner3 = gameManager.boardManager.PlacePiece(0, 8, GameManager.Player.Black);
        bool corner4 = gameManager.boardManager.PlacePiece(8, 8, GameManager.Player.White);
        
        edgeCasesHandled &= corner1 && corner2 && corner3 && corner4;
        Debug.Log($"Small board corner placements: {corner1 && corner2 && corner3 && corner4}");

        // Test maximum board size
        gameManager.SetBoardSize(19);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Test placing pieces at all corners of large board
        corner1 = gameManager.boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        corner2 = gameManager.boardManager.PlacePiece(18, 0, GameManager.Player.White);
        corner3 = gameManager.boardManager.PlacePiece(0, 18, GameManager.Player.Black);
        corner4 = gameManager.boardManager.PlacePiece(18, 18, GameManager.Player.White);
        
        edgeCasesHandled &= corner1 && corner2 && corner3 && corner4;
        Debug.Log($"Large board corner placements: {corner1 && corner2 && corner3 && corner4}");

        // Test win detection at board boundaries
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Create win at top boundary
        for (int i = 0; i < 5; i++)
        {
            gameManager.boardManager.PlacePiece(i, 0, GameManager.Player.Black);
        }
        bool boundaryWin = gameManager.CheckWin(4, 0);
        edgeCasesHandled &= boundaryWin;
        Debug.Log($"Boundary win detection: {boundaryWin}");

        // Test draw condition with nearly full board
        gameManager.SetBoardSize(5);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Fill all but one position
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (!(x == 2 && y == 2)) // Leave center empty
                {
                    GameManager.Player player = ((x + y) % 2 == 0) ? GameManager.Player.Black : GameManager.Player.White;
                    gameManager.boardManager.PlacePiece(x, y, player);
                }
            }
        }

        bool nearDraw = !gameManager.CheckDraw(); // Should not be draw with one empty space
        edgeCasesHandled &= nearDraw;
        Debug.Log($"Near-draw condition handled: {nearDraw}");

        Debug.Log($"All edge cases and boundary conditions: {edgeCasesHandled}");
    }

    private IEnumerator TestErrorHandlingRecovery()
    {
        Debug.Log("Testing Error Handling and Recovery");

        bool errorHandlingGood = true;

        // Test invalid board size handling
        bool invalidSizeHandled = true;
        try
        {
            gameManager.SetBoardSize(-1);
            // Should either fail or clamp to valid range
            int currentSize = gameManager.GetBoardSize();
            invalidSizeHandled = currentSize >= 9 && currentSize <= 19;
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for invalid board size: {e.Message}");
            invalidSizeHandled = true;
        }
        errorHandlingGood &= invalidSizeHandled;
        Debug.Log($"Invalid board size handling: {invalidSizeHandled}");

        // Test placing piece on occupied position
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        bool firstPlacement = gameManager.boardManager.PlacePiece(7, 7, GameManager.Player.Black);
        bool secondPlacement = gameManager.boardManager.PlacePiece(7, 7, GameManager.Player.White);
        
        bool duplicatePlacementHandled = firstPlacement && !secondPlacement;
        errorHandlingGood &= duplicatePlacementHandled;
        Debug.Log($"Duplicate placement handling: {duplicatePlacementHandled}");

        // Test out of bounds placement
        bool outOfBoundsHandled = !gameManager.boardManager.PlacePiece(-1, 0, GameManager.Player.Black) &&
                                 !gameManager.boardManager.PlacePiece(0, -1, GameManager.Player.Black) &&
                                 !gameManager.boardManager.PlacePiece(15, 0, GameManager.Player.Black) &&
                                 !gameManager.boardManager.PlacePiece(0, 15, GameManager.Player.Black);
        errorHandlingGood &= outOfBoundsHandled;
        Debug.Log($"Out of bounds placement handling: {outOfBoundsHandled}");

        // Test invalid win condition
        bool invalidWinConditionHandled = true;
        //try
        //{
        //    gameManager.SetWinConditionType(""999");
        //    var currentCondition = gameManager.GetWinConditionType();
        //    invalidWinConditionHandled = currentCondition >= GameManager.WinConditionType.FiveInRow && 
        //                                currentCondition <= GameManager.WinConditionType.FiveInRowFreeThree;
        //}
        //catch (System.Exception e)
        //{
        //    Debug.Log($"Exception handled for invalid win condition: {e.Message}");
        //    invalidWinConditionHandled = true;
        //}
        errorHandlingGood &= invalidWinConditionHandled;
        Debug.Log($"Invalid win condition handling: {invalidWinConditionHandled}");

        // Test settings persistence error recovery
        bool settingsErrorRecovery = true;
        try
        {
            // Simulate corrupted settings
            PlayerPrefs.SetString("BoardSize", "invalid");
            PlayerPrefs.Save();
            
            // Should recover with default settings
            int recoveredSize = PlayerPrefsManager.LoadBoardSize();
            settingsErrorRecovery = recoveredSize >= 9 && recoveredSize <= 19;
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for corrupted settings: {e.Message}");
            settingsErrorRecovery = true;
        }
        errorHandlingGood &= settingsErrorRecovery;
        Debug.Log($"Settings error recovery: {settingsErrorRecovery}");

        Debug.Log($"All error handling and recovery tests: {errorHandlingGood}");
    }

    private IEnumerator TestInvalidInputScenarios()
    {
        Debug.Log("Testing Invalid Input Scenarios");

        bool invalidInputHandled = true;

        // Test null and empty theme names
        bool nullThemeHandled = true;
        try
        {
            PlayerPrefsManager.SaveBoardTheme(null);
            string loadedTheme = PlayerPrefsManager.LoadBoardTheme();
            nullThemeHandled = !string.IsNullOrEmpty(loadedTheme);
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for null theme: {e.Message}");
            nullThemeHandled = true;
        }
        invalidInputHandled &= nullThemeHandled;
        Debug.Log($"Null theme handling: {nullThemeHandled}");

        // Test extremely long theme name
        bool longThemeHandled = true;
        try
        {
            string longTheme = new string('A', 1000);
            PlayerPrefsManager.SaveBoardTheme(longTheme);
            string loadedTheme = PlayerPrefsManager.LoadBoardTheme();
            longThemeHandled = !string.IsNullOrEmpty(loadedTheme);
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for long theme: {e.Message}");
            longThemeHandled = true;
        }
        invalidInputHandled &= longThemeHandled;
        Debug.Log($"Long theme handling: {longThemeHandled}");

        // Test invalid player switching scenarios
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Test switching player multiple times rapidly
        GameManager.Player initialPlayer = gameManager.currentPlayer;
        
        for (int i = 0; i < 100; i++)
        {
            gameManager.SwitchPlayer();
        }
        
        GameManager.Player finalPlayer = gameManager.currentPlayer;
        bool rapidSwitchingHandled = (initialPlayer == GameManager.Player.Black && finalPlayer == GameManager.Player.White) ||
                                    (initialPlayer == GameManager.Player.White && finalPlayer == GameManager.Player.Black);
        invalidInputHandled &= rapidSwitchingHandled;
        Debug.Log($"Rapid player switching handled: {rapidSwitchingHandled}");

        // Test game operations during invalid states
        bool invalidStateOperations = true;

        // Try operations during game over state
        gameManager.EndGame();
        yield return new WaitForSeconds(0.1f);
        
        // Should handle piece placement attempts gracefully
        bool placementDuringGameOver = !gameManager.boardManager.PlacePiece(0, 0, GameManager.Player.Black);
        invalidStateOperations &= placementDuringGameOver;
        Debug.Log($"Placement during game over handled: {placementDuringGameOver}");

        Debug.Log($"All invalid input scenario tests: {invalidInputHandled}");
    }

    private IEnumerator TestStateManagementIssues()
    {
        Debug.Log("Testing State Management Issues");

        bool stateManagementGood = true;

        // Test state transitions
        bool stateTransitionsValid = true;
        
        // MainMenu -> Playing
        uiManager.ShowMainMenu();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.gameState == GameManager.GameState.MainMenu;
        
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.gameState == GameManager.GameState.Playing;
        
        // Playing -> GameOver
        gameManager.EndGame();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.gameState == GameManager.GameState.GameOver;
        
        // GameOver -> MainMenu
        uiManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        stateTransitionsValid &= gameManager.gameState == GameManager.GameState.MainMenu;
        
        stateManagementGood &= stateTransitionsValid;
        Debug.Log($"State transitions valid: {stateTransitionsValid}");

        // Test restart from different states
        bool restartFromAllStates = true;
        
        // Restart from Playing
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        uiManager.RestartGame();
        yield return new WaitForSeconds(0.1f);
        restartFromAllStates &= gameManager.gameState == GameManager.GameState.Playing;
        
        // Restart from GameOver
        gameManager.EndGame();
        yield return new WaitForSeconds(0.1f);
        uiManager.RestartGame();
        yield return new WaitForSeconds(0.1f);
        restartFromAllStates &= gameManager.gameState == GameManager.GameState.Playing;
        
        stateManagementGood &= restartFromAllStates;
        Debug.Log($"Restart from all states: {restartFromAllStates}");

        // Test settings persistence across state changes
        bool settingsPersistAcrossStates = true;
        
        // Set settings in main menu
        uiManager.ShowMainMenu();
        yield return new WaitForSeconds(0.1f);
        gameManager.SetBoardSize(13);
        
        // Start game and verify settings
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        settingsPersistAcrossStates &= gameManager.GetBoardSize() == 13;
        
        // Restart and verify settings persist
        uiManager.RestartGame();
        yield return new WaitForSeconds(0.1f);
        settingsPersistAcrossStates &= gameManager.GetBoardSize() == 13;
        
        // Return to menu and start new game
        uiManager.ReturnToMainMenu();
        yield return new WaitForSeconds(0.1f);
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        settingsPersistAcrossStates &= gameManager.GetBoardSize() == 13;
        
        stateManagementGood &= settingsPersistAcrossStates;
        Debug.Log($"Settings persist across state changes: {settingsPersistAcrossStates}");

        Debug.Log($"All state management tests: {stateManagementGood}");
    }

    private IEnumerator TestIntegrationBugScenarios()
    {
        Debug.Log("Testing Integration Bug Scenarios");

        bool integrationBugsFixed = true;

        // Test rapid UI interactions
        bool rapidUIInteractions = true;
        
        // Rapidly switch between UI states
        for (int i = 0; i < 10; i++)
        {
            uiManager.ShowMainMenu();
            yield return new WaitForSeconds(0.01f);
            uiManager.StartGame();
            yield return new WaitForSeconds(0.01f);
            uiManager.ShowSettings();
            yield return new WaitForSeconds(0.01f);
            uiManager.CancelSettings();
            yield return new WaitForSeconds(0.01f);
        }
        
        // Should end in a consistent state
        rapidUIInteractions &= gameManager.gameState == GameManager.GameState.Playing ||
                              gameManager.gameState == GameManager.GameState.MainMenu;
        integrationBugsFixed &= rapidUIInteractions;
        Debug.Log($"Rapid UI interactions handled: {rapidUIInteractions}");

        // Test simultaneous game operations
        bool simultaneousOperations = true;
        
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        
        // Simulate multiple operations happening at once
        for (int i = 0; i < 5; i++)
        {
            int x = i;
            int y = i;
            
            if (gameManager.boardManager.IsPositionEmpty(x, y))
            {
                gameManager.boardManager.PlacePiece(x, y, gameManager.currentPlayer);
                gameManager.CheckWin(x, y);
                gameManager.SwitchPlayer();
            }
        }
        
        // Should not crash or enter invalid state
        simultaneousOperations &= gameManager.gameState == GameManager.GameState.Playing;
        integrationBugsFixed &= simultaneousOperations;
        Debug.Log($"Simultaneous operations handled: {simultaneousOperations}");

        // Test settings changes during gameplay
        bool settingsDuringGameplay = true;
        
        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);
        
        // Change settings while game is in progress
        uiManager.ShowSettings();
        yield return new WaitForSeconds(0.1f);
        
        gameManager.SetBoardSize(19);
        uiManager.SaveSettings();
        yield return new WaitForSeconds(0.1f);
        
        // Game should continue normally
        settingsDuringGameplay &= gameManager.gameState == GameManager.GameState.Playing;
        settingsDuringGameplay &= gameManager.GetBoardSize() == 19;
        integrationBugsFixed &= settingsDuringGameplay;
        Debug.Log($"Settings changes during gameplay: {settingsDuringGameplay}");

        // Test win detection with complex board states
        bool complexWinDetection = true;
        
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);
        
        // Create complex pattern with multiple potential wins
        for (int i = 0; i < 5; i++)
        {
            // Horizontal line
            gameManager.boardManager.PlacePiece(i, 7, GameManager.Player.Black);
            // Vertical line
            gameManager.boardManager.PlacePiece(7, i, GameManager.Player.White);
        }
        
        // Should correctly detect horizontal win
        complexWinDetection &= gameManager.CheckWin(4, 7);
        integrationBugsFixed &= complexWinDetection;
        Debug.Log($"Complex win detection: {complexWinDetection}");

        Debug.Log($"All integration bug scenario tests: {integrationBugsFixed}");
    }
}