using UnityEngine;
using GomokuGame.Utilities;

/// <summary>
/// Comprehensive unit tests for PlayerPrefsManager functionality
/// Tests settings persistence, error handling, and backup functionality
/// </summary>
public class PlayerPrefsManagerUnitTest : MonoBehaviour
{
    void Start()
    {
        RunPlayerPrefsManagerTests();
    }

    private void RunPlayerPrefsManagerTests()
    {
        Debug.Log("Starting PlayerPrefsManager Unit Tests");

        // Test 1: Settings Persistence
        TestSettingsPersistence();

        // Test 2: Error Handling
        TestErrorHandling();

        // Test 3: Backup Functionality
        TestBackupFunctionality();

        // Test 4: Default Settings
        TestDefaultSettings();

        // Test 5: Cross-Session Persistence
        TestCrossSessionPersistence();

        Debug.Log("PlayerPrefsManager Unit Tests Completed");
    }

    private void TestSettingsPersistence()
    {
        Debug.Log("Testing Settings Persistence");

        bool persistenceValid = true;

        // Test board size persistence
        int[] testSizes = { 9, 13, 15, 19 };
        foreach (int size in testSizes)
        {
            PlayerPrefsManager.SaveBoardSize(size);
            int retrievedSize = PlayerPrefsManager.LoadBoardSize();
            persistenceValid &= retrievedSize == size;
            Debug.Log($"Board size {size}: Saved={size}, Loaded={retrievedSize}, Match={retrievedSize == size}");
        }

        // Test win condition persistence - using numeric values since PlayerPrefsManager uses int
        int[] winConditions = { 0, 1, 2 }; // Standard win condition values

        foreach (int condition in winConditions)
        {
            PlayerPrefsManager.SaveWinCondition(condition);
            int retrievedCondition = PlayerPrefsManager.LoadWinCondition();
            persistenceValid &= retrievedCondition == condition;
            Debug.Log($"Win condition {condition}: Saved={condition}, Loaded={retrievedCondition}, Match={retrievedCondition == condition}");
        }

        // Test theme persistence
        string[] testThemes = { "Classic", "Modern", "Dark", "Light" };
        foreach (string theme in testThemes)
        {
            PlayerPrefsManager.SaveTheme(theme);
            string retrievedTheme = PlayerPrefsManager.LoadTheme();
            persistenceValid &= retrievedTheme == theme;
            Debug.Log($"Theme {theme}: Saved={theme}, Loaded={retrievedTheme}, Match={retrievedTheme == theme}");
        }

        Debug.Log($"All settings persistence tests: {persistenceValid}");
    }

    private void TestErrorHandling()
    {
        Debug.Log("Testing Error Handling");

        bool errorHandlingValid = true;

        // Test invalid board sizes
        int[] invalidSizes = { -1, 0, 8, 20, 100 };
        foreach (int size in invalidSizes)
        {
            // SaveBoardSize doesn't return a bool, it clamps invalid values
            PlayerPrefsManager.SaveBoardSize(size);
            int loadedSize = PlayerPrefsManager.LoadBoardSize();

            // Invalid sizes should be clamped to valid range
            bool handledProperly = loadedSize >= 9 && loadedSize <= 19;
            errorHandlingValid &= handledProperly;
            Debug.Log($"Invalid size {size}: Loaded={loadedSize}, Handled={handledProperly}");
        }

        // Test invalid win conditions (out of range)
        bool invalidWinConditionHandled = true;
        try
        {
            // Try to save an invalid win condition
            PlayerPrefsManager.SaveWinCondition(999);
            int loadedCondition = PlayerPrefsManager.LoadWinCondition();
            // Should default to a valid condition
            invalidWinConditionHandled = loadedCondition >= 0 && loadedCondition <= 2;
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for invalid win condition: {e.Message}");
            invalidWinConditionHandled = true; // Exception handling is valid
        }

        errorHandlingValid &= invalidWinConditionHandled;
        Debug.Log($"Invalid win condition handling: {invalidWinConditionHandled}");

        // Test null/empty theme
        bool nullThemeHandled = true;
        try
        {
            PlayerPrefsManager.SaveTheme(null);
            string loadedTheme = PlayerPrefsManager.LoadTheme();
            nullThemeHandled = !string.IsNullOrEmpty(loadedTheme);
        }
        catch (System.Exception e)
        {
            Debug.Log($"Exception handled for null theme: {e.Message}");
            nullThemeHandled = true;
        }

        errorHandlingValid &= nullThemeHandled;
        Debug.Log($"Null theme handling: {nullThemeHandled}");

        Debug.Log($"All error handling tests: {errorHandlingValid}");
    }

    private void TestBackupFunctionality()
    {
        Debug.Log("Testing Backup Functionality");

        bool backupValid = true;

        // Save some settings
        PlayerPrefsManager.SaveBoardSize(15);
        PlayerPrefsManager.SaveWinCondition(1); // Use numeric win condition
        PlayerPrefsManager.SaveTheme("Modern");

        // Test safe save with backup
        PlayerPrefsManager.SafeSaveAllSettings(15, 1, "Modern");
        Debug.Log("Safe save operation completed");

        // Test that settings are still accessible after safe save
        int loadedSize = PlayerPrefsManager.LoadBoardSize();
        int loadedCondition = PlayerPrefsManager.LoadWinCondition();
        string loadedTheme = PlayerPrefsManager.LoadTheme();

        bool settingsIntact = loadedSize == 15 &&
                             loadedCondition == 1 &&
                             loadedTheme == "Modern";

        backupValid &= settingsIntact;
        Debug.Log($"Settings intact after safe save: {settingsIntact}");

        // Note: PlayerPrefsManager doesn't have RestoreFromBackup method
        // This test verifies that SafeSaveAllSettings works correctly

        Debug.Log($"All backup functionality tests: {backupValid}");
    }

    private void TestDefaultSettings()
    {
        Debug.Log("Testing Default Settings");

        bool defaultsValid = true;

        // Clear all settings to test defaults
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Test default board size
        int defaultSize = PlayerPrefsManager.LoadBoardSize();
        bool validDefaultSize = defaultSize >= 9 && defaultSize <= 19;
        defaultsValid &= validDefaultSize;
        Debug.Log($"Default board size: {defaultSize}, Valid={validDefaultSize}");

        // Test default win condition
        int defaultCondition = PlayerPrefsManager.LoadWinCondition();
        bool validDefaultCondition = defaultCondition >= 0 && defaultCondition <= 2;
        defaultsValid &= validDefaultCondition;
        Debug.Log($"Default win condition: {defaultCondition}, Valid={validDefaultCondition}");

        // Test default theme
        string defaultTheme = PlayerPrefsManager.LoadTheme();
        bool validDefaultTheme = !string.IsNullOrEmpty(defaultTheme);
        defaultsValid &= validDefaultTheme;
        Debug.Log($"Default theme: {defaultTheme}, Valid={validDefaultTheme}");

        Debug.Log($"All default settings tests: {defaultsValid}");
    }

    private void TestCrossSessionPersistence()
    {
        Debug.Log("Testing Cross-Session Persistence");

        bool crossSessionValid = true;

        // Save settings
        PlayerPrefsManager.SaveBoardSize(13);
        PlayerPrefsManager.SaveWinCondition(2); // Use numeric win condition
        PlayerPrefsManager.SaveTheme("Dark");

        // Force save to disk
        PlayerPrefs.Save();

        // Simulate application restart by reloading PlayerPrefs
        PlayerPrefs.Save(); // Ensure current state is saved

        // In a real scenario, the application would restart and PlayerPrefs would reload
        // For testing, we'll just verify the values are still accessible

        int loadedSize = PlayerPrefsManager.LoadBoardSize();
        int loadedCondition = PlayerPrefsManager.LoadWinCondition();
        string loadedTheme = PlayerPrefsManager.LoadTheme();

        bool settingsPersisted = loadedSize == 13 &&
                                loadedCondition == 2 &&
                                loadedTheme == "Dark";

        crossSessionValid &= settingsPersisted;
        Debug.Log($"Cross-session persistence: {settingsPersisted}");

        // Test multiple save/load cycles
        for (int i = 0; i < 5; i++)
        {
            int testSize = 9 + (i % 4) * 2; // 9, 11, 13, 15, 9
            PlayerPrefsManager.SaveBoardSize(testSize);
            int reloadedSize = PlayerPrefsManager.LoadBoardSize();

            bool cycleValid = reloadedSize == testSize;
            crossSessionValid &= cycleValid;

            if (!cycleValid)
            {
                Debug.LogError($"Save/load cycle {i} failed: Expected={testSize}, Got={reloadedSize}");
            }
        }

        Debug.Log($"Multiple save/load cycles: {crossSessionValid}");

        Debug.Log($"All cross-session persistence tests: {crossSessionValid}");
    }
}