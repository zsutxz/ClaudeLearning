using UnityEngine;
using System.Collections;
using GomokuGame.Core;
using GomokuGame.UI;
using GomokuGame.Utilities;

/// <summary>
/// Integration tests for settings persistence across game sessions
/// Tests automatic save/load functionality and cross-session settings preservation
/// </summary>
public class SettingsPersistenceIntegrationTest : MonoBehaviour
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
            StartCoroutine(RunSettingsPersistenceTests());
        }
        else
        {
            Debug.LogError("SettingsPersistenceIntegrationTest: Could not find required components");
        }
    }

    private IEnumerator RunSettingsPersistenceTests()
    {
        Debug.Log("Starting Settings Persistence Integration Tests");

        // Test 1: Automatic Settings Save/Load
        yield return StartCoroutine(TestAutomaticSettingsSaveLoad());

        // Test 2: Cross-Session Persistence
        yield return StartCoroutine(TestCrossSessionPersistence());

        // Test 3: Default Settings Application
        yield return StartCoroutine(TestDefaultSettingsApplication());

        // Test 4: Settings Integration with Game Flow
        yield return StartCoroutine(TestSettingsGameFlowIntegration());

        // Test 5: Error Recovery and Backup
        yield return StartCoroutine(TestErrorRecoveryBackup());

        Debug.Log("Settings Persistence Integration Tests Completed");
    }

    private IEnumerator TestAutomaticSettingsSaveLoad()
    {
        Debug.Log("Testing Automatic Settings Save/Load");

        bool autoSaveLoadValid = true;

        // Save initial settings
        int initialSize = 13;
        var initialCondition = GetWinConditionType();
        string initialTheme = "Dark";

        PlayerPrefsManager.SaveBoardSize(initialSize);
        NewMethod(initialCondition);
        PlayerPrefsManager.SaveBoardTheme(initialTheme);

        // Start a game and verify settings are loaded
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        int loadedSize = gameManager.GetBoardSize();
        var loadedCondition = gameManager.GetWinConditionType();

        bool settingsLoaded = loadedSize == initialSize && loadedCondition == initialCondition;
        autoSaveLoadValid &= settingsLoaded;
        Debug.Log($"Settings loaded on game start: Size={loadedSize}, Condition={loadedCondition}, Valid={settingsLoaded}");

        // Change settings through UI and verify auto-save
        int newSize = 15;
        var newCondition = GameManager.WinConditionType.FiveInRowFreeThree;

        gameManager.SetBoardSize(newSize);
        gameManager.SetWinConditionType(newCondition.ToString());

        // Simulate settings save through UI
        uiManager.SaveSettings();
        yield return new WaitForSeconds(0.1f);

        // Verify settings were saved
        int savedSize = PlayerPrefsManager.LoadBoardSize();
        var savedCondition = PlayerPrefsManager.LoadWinConditionType();

        bool settingsSaved = savedSize == newSize && savedCondition == newCondition.ToString();
        autoSaveLoadValid &= settingsSaved;
        Debug.Log($"Settings auto-saved: Size={savedSize}, Condition={savedCondition}, Valid={settingsSaved}");

        // Start new game and verify new settings are loaded
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        int newGameSize = gameManager.GetBoardSize();
        var newGameCondition = gameManager.GetWinConditionType();

        bool newSettingsLoaded = newGameSize == newSize && newGameCondition == newCondition.ToString();
        autoSaveLoadValid &= newSettingsLoaded;
        Debug.Log($"New settings loaded: Size={newGameSize}, Condition={newGameCondition}, Valid={newSettingsLoaded}");

        Debug.Log($"All automatic save/load tests: {autoSaveLoadValid}");

        static object GetWinConditionType()
        {
            return GameManager.WinConditionType.FiveInRow.ToString();
        }
    }

    private static void NewMethod(object initialCondition)
    {
        PlayerPrefsManager.SaveWinConditionType(initialCondition);
    }

    private IEnumerator TestCrossSessionPersistence()
    {
        Debug.Log("Testing Cross-Session Persistence");

        bool crossSessionValid = true;

        // Save specific settings
        int testSize = 19;
        var testCondition = GameManager.WinConditionType.FiveInRow.ToString();
        string testTheme = "Classic";

        PlayerPrefsManager.SaveBoardSize(testSize);
        PlayerPrefsManager.SaveWinConditionType(testCondition);
        PlayerPrefsManager.SaveBoardTheme(testTheme);

        // Force save to ensure persistence
        PlayerPrefs.Save();
        yield return new WaitForSeconds(0.1f);

        // Simulate application restart by clearing and reloading
        // In a real scenario, the application would restart
        // For testing, we'll verify the values persist
        
        int persistedSize = PlayerPrefsManager.LoadBoardSize();
        var persistedCondition = PlayerPrefsManager.LoadWinConditionType();
        string persistedTheme = PlayerPrefsManager.LoadBoardTheme();

        bool settingsPersisted = persistedSize == testSize && 
                                persistedCondition == testCondition && 
                                persistedTheme == testTheme;
        
        crossSessionValid &= settingsPersisted;
        Debug.Log($"Settings persisted across sessions: {settingsPersisted}");

        // Test multiple configuration changes
        for (int i = 0; i < 3; i++)
        {
            int cycleSize = 9 + (i * 2); // 9, 11, 13
            var cycleCondition = (GameManager.WinConditionType)(i % 3);
            string cycleTheme = i % 2 == 0 ? "Light" : "Dark";

            PlayerPrefsManager.SaveBoardSize(cycleSize);
            PlayerPrefsManager.SaveWinConditionType(cycleCondition);
            PlayerPrefsManager.SaveBoardTheme(cycleTheme);

            PlayerPrefs.Save();
            yield return new WaitForSeconds(0.05f);

            // Verify each configuration persists
            int loadedCycleSize = PlayerPrefsManager.LoadBoardSize();
            var loadedCycleCondition = PlayerPrefsManager.LoadWinConditionType();
            string loadedCycleTheme = PlayerPrefsManager.LoadBoardTheme();

            bool cyclePersisted = loadedCycleSize == cycleSize && 
                                 loadedCycleCondition == cycleCondition.ToString() && 
                                 loadedCycleTheme == cycleTheme;
            
            crossSessionValid &= cyclePersisted;
            Debug.Log($"Configuration cycle {i} persisted: {cyclePersisted}");
        }

        Debug.Log($"All cross-session persistence tests: {crossSessionValid}");
    }

    private IEnumerator TestDefaultSettingsApplication()
    {
        Debug.Log("Testing Default Settings Application");

        bool defaultsValid = true;

        // Clear all settings to test defaults
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        yield return new WaitForSeconds(0.1f);

        // Start a game - should use default settings
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        int defaultSize = gameManager.GetBoardSize();
        var defaultCondition = gameManager.GetWinConditionType();

        // Verify defaults are reasonable
        bool validDefaultSize = defaultSize >= 9 && defaultSize <= 19;
        bool validDefaultCondition = false;
        
        defaultsValid &= validDefaultSize && validDefaultCondition;
        Debug.Log($"Default settings applied: Size={defaultSize}, Condition={defaultCondition}, Valid={validDefaultSize && validDefaultCondition}");

        // Test that defaults are saved after first use
        int savedDefaultSize = PlayerPrefsManager.LoadBoardSize();
        var savedDefaultCondition = PlayerPrefsManager.LoadWinConditionType();
        
        bool defaultsSaved = savedDefaultSize == defaultSize && savedDefaultCondition == defaultCondition;
        defaultsValid &= defaultsSaved;
        Debug.Log($"Defaults saved after first use: {defaultsSaved}");

        Debug.Log($"All default settings tests: {defaultsValid}");
    }

    private IEnumerator TestSettingsGameFlowIntegration()
    {
        Debug.Log("Testing Settings Game Flow Integration");

        bool integrationValid = true;

        // Set specific settings
        PlayerPrefsManager.SaveBoardSize(13);
        PlayerPrefsManager.SaveWinConditionType(GameManager.WinConditionType.FiveInRowNoOverlines);

        // Complete game flow with settings
        uiManager.ShowMainMenu();
        yield return new WaitForSeconds(0.1f);

        uiManager.StartGame();
        yield return new WaitForSeconds(0.1f);

        // Verify settings are applied
        int gameSize = gameManager.GetBoardSize();
        var gameCondition = gameManager.GetWinConditionType();
        
        bool settingsApplied = gameSize == 13 && gameCondition == GameManager.WinConditionType.FiveInRowNoOverlines.ToString();
        integrationValid &= settingsApplied;
        Debug.Log($"Settings applied in game flow: {settingsApplied}");

        // Change settings during gameplay
        uiManager.ShowSettings();
        yield return new WaitForSeconds(0.1f);

        gameManager.SetBoardSize(15);
        gameManager.SetWinConditionType(GameManager.WinConditionType.FiveInRow.ToString());
        uiManager.SaveSettings();
        yield return new WaitForSeconds(0.1f);

        // Verify settings changed
        int changedSize = gameManager.GetBoardSize();
        var changedCondition = gameManager.GetWinConditionType();
        
        bool settingsChanged = changedSize == 15 && changedCondition == GameManager.WinConditionType.FiveInRow.ToString();
        integrationValid &= settingsChanged;
        Debug.Log($"Settings changed during gameplay: {settingsChanged}");

        // Restart game and verify new settings persist
        uiManager.RestartGame();
        yield return new WaitForSeconds(0.1f);

        int restartSize = gameManager.GetBoardSize();
        var restartCondition = gameManager.GetWinConditionType();
        
        bool restartSettings = restartSize == 15 && restartCondition == GameManager.WinConditionType.FiveInRow.ToString();
        integrationValid &= restartSettings;
        Debug.Log($"Settings persist after restart: {restartSettings}");

        Debug.Log($"All settings game flow integration tests: {integrationValid}");
    }

    private IEnumerator TestErrorRecoveryBackup()
    {
        Debug.Log("Testing Error Recovery and Backup");

        bool errorRecoveryValid = true;

        // Save valid settings
        PlayerPrefsManager.SaveBoardSize(15);
        PlayerPrefsManager.SaveWinConditionType(GetFiveInRow());
        PlayerPrefsManager.SaveBoardTheme("Modern");

        // Test safe save with backup
        bool safeSaveResult = PlayerPrefsManager.SafeSaveAllSettings(13, (int)(GameManager.WinConditionType.FiveInRowNoOverlines), "Dark");
        errorRecoveryValid &= safeSaveResult;
        Debug.Log($"Safe save operation: {safeSaveResult}");

        // Verify settings after safe save
        int safeSize = PlayerPrefsManager.LoadBoardSize();
        var safeCondition = PlayerPrefsManager.LoadWinConditionType();
        string safeTheme = PlayerPrefsManager.LoadBoardTheme();

        bool safeSettings = safeSize == 13 && safeTheme == "Dark";
        errorRecoveryValid &= safeSettings;
        Debug.Log($"Settings after safe save: {safeSettings}");

        // Test backup restoration capability
        bool restoreAvailable = PlayerPrefsManager.RestoreFromBackup();
        Debug.Log($"Backup restoration available: {restoreAvailable}");

        // Test error handling with invalid inputs
        bool invalidSizeHandled = (PlayerPrefsManager.LoadBoardSize() >= 9 && PlayerPrefsManager.LoadBoardSize() <= 19);
        errorRecoveryValid &= invalidSizeHandled;
        Debug.Log($"Invalid size handling: {invalidSizeHandled}");

        // Test recovery from corrupted settings
        // Simulate by setting invalid values directly
        PlayerPrefs.SetString("BoardSize", "invalid");
        PlayerPrefs.Save();
        yield return new WaitForSeconds(0.1f);

        // Should recover with default or valid value
        int recoveredSize = PlayerPrefsManager.LoadBoardSize();
        bool recovered = recoveredSize >= 9 && recoveredSize <= 19;
        errorRecoveryValid &= recovered;
        Debug.Log($"Recovery from corrupted settings: {recovered}, Size={recoveredSize}");

        Debug.Log($"All error recovery and backup tests: {errorRecoveryValid}");
    }

    private static object GetFiveInRow()
    {
        return GameManager.WinConditionType.FiveInRow;
    }
}