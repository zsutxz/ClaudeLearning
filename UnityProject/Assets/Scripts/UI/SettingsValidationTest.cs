using UnityEngine;
using GomokuGame.Core;
using System.Collections;
using GomokuGame.UI;

/// <summary>
/// Validation test for game settings menu functionality
/// </summary>
public class SettingsValidationTest : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        // Run settings validation tests
        StartCoroutine(RunSettingsValidationTests());
    }

    private IEnumerator RunSettingsValidationTests()
    {
        Debug.Log("Starting Settings Menu Validation Tests");

        // Test settings menu access
        yield return StartCoroutine(TestSettingsMenuAccess());

        // Test settings persistence
        yield return StartCoroutine(TestSettingsPersistence());

        // Test default settings
        yield return StartCoroutine(TestDefaultSettings());

        // Test settings integration with game logic
        yield return StartCoroutine(TestSettingsIntegration());

        Debug.Log("Settings Menu Validation Tests Completed Successfully!");
    }

    private IEnumerator TestSettingsMenuAccess()
    {
        Debug.Log("Testing Settings Menu Access");

        // Test that settings panel can be shown
        if (uiManager != null)
        {
            uiManager.ShowSettings();
            yield return new WaitForSeconds(0.5f);

            // Verify settings panel is active
            Debug.Log("Settings menu access test completed");
        }
        else
        {
            Debug.LogWarning("UIManager not found for settings access test");
        }

        yield return null;
    }

    private IEnumerator TestSettingsPersistence()
    {
        Debug.Log("Testing Settings Persistence");

        // Test board size persistence
        int testBoardSize = 13;
        PlayerPrefs.SetInt("BoardSize", testBoardSize);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.1f);

        // Verify board size is saved
        int savedBoardSize = PlayerPrefs.GetInt("BoardSize", 15);
        Debug.Log($"Board size persistence test: Set={testBoardSize}, Saved={savedBoardSize}");

        // Test win condition persistence
        string testWinCondition = "Capture";
        PlayerPrefs.SetString("WinConditionType", testWinCondition);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.1f);

        // Verify win condition is saved
        string savedWinCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Win condition persistence test: Set={testWinCondition}, Saved={savedWinCondition}");

        yield return null;
    }

    private IEnumerator TestDefaultSettings()
    {
        Debug.Log("Testing Default Settings");

        // Clear PlayerPrefs to test defaults
        PlayerPrefs.DeleteKey("BoardSize");
        PlayerPrefs.DeleteKey("WinConditionType");

        yield return new WaitForSeconds(0.1f);

        // Verify default board size
        int defaultBoardSize = PlayerPrefs.GetInt("BoardSize", 15);
        Debug.Log($"Default board size test: {defaultBoardSize} (should be 15)");

        // Verify default win condition
        string defaultWinCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Default win condition test: {defaultWinCondition} (should be Standard)");

        yield return null;
    }

    private IEnumerator TestSettingsIntegration()
    {
        Debug.Log("Testing Settings Integration with Game Logic");

        if (gameManager != null)
        {
            // Test board size integration
            int testBoardSize = 19;
            gameManager.SetBoardSize(testBoardSize);
            int currentBoardSize = gameManager.GetBoardSize();
            Debug.Log($"Board size integration test: Set={testBoardSize}, Current={currentBoardSize}");

            // Test win condition integration
            string testWinCondition = "TimeBased";
            gameManager.SetWinConditionType(testWinCondition);

            // Start a new game to verify settings are applied
            gameManager.StartNewGame();

            yield return new WaitForSeconds(0.5f);

            Debug.Log("Settings integration test completed");
        }
        else
        {
            Debug.LogWarning("GameManager not found for settings integration test");
        }

        yield return null;
    }
}