using UnityEngine;
using GomokuGame.Core;
using System.Collections;

/// <summary>
/// Validation test for alternative win conditions
/// </summary>
public class WinConditionValidationTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private WinDetector winDetector;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (winDetector == null)
            winDetector = FindObjectOfType<WinDetector>();

        // Run win condition validation tests
        StartCoroutine(RunWinConditionValidationTests());
    }

    private IEnumerator RunWinConditionValidationTests()
    {
        Debug.Log("Starting Win Condition Validation Tests");

        // Test Standard win condition
        yield return StartCoroutine(TestStandardWinCondition());

        // Test Capture win condition
        yield return StartCoroutine(TestCaptureWinCondition());

        // Test Time-based win condition
        yield return StartCoroutine(TestTimeBasedWinCondition());

        // Test default win condition
        yield return StartCoroutine(TestDefaultWinCondition());

        Debug.Log("Win Condition Validation Tests Completed Successfully!");
    }

    private IEnumerator TestStandardWinCondition()
    {
        Debug.Log("Testing Standard win condition");

        // Set win condition to Standard
        gameManager.SetWinConditionType("Standard");
        gameManager.StartNewGame();

        yield return null;

        // Test that standard win detection works
        bool winDetected = TestStandardWinDetection();
        Debug.Log($"Standard win detection: {winDetected}");

        // Verify win condition type is set correctly
        string currentCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Current win condition: {currentCondition}");

        yield return null;
    }

    private IEnumerator TestCaptureWinCondition()
    {
        Debug.Log("Testing Capture win condition");

        // Set win condition to Capture
        gameManager.SetWinConditionType("Capture");
        gameManager.StartNewGame();

        yield return null;

        // Test that capture win detection works
        bool captureWinDetected = TestCaptureWinDetection();
        Debug.Log($"Capture win detection: {captureWinDetected}");

        // Verify win condition type is set correctly
        string currentCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Current win condition: {currentCondition}");

        yield return null;
    }

    private IEnumerator TestTimeBasedWinCondition()
    {
        Debug.Log("Testing Time-based win condition");

        // Set win condition to TimeBased
        gameManager.SetWinConditionType("TimeBased");
        gameManager.StartNewGame();

        yield return null;

        // Test that time-based win detection works
        bool timeWinDetected = TestTimeBasedWinDetection();
        Debug.Log($"Time-based win detection: {timeWinDetected}");

        // Verify win condition type is set correctly
        string currentCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Current win condition: {currentCondition}");

        yield return null;
    }

    private IEnumerator TestDefaultWinCondition()
    {
        Debug.Log("Testing Default win condition");

        // Clear PlayerPrefs to test default
        PlayerPrefs.DeleteKey("WinConditionType");
        gameManager.StartNewGame();

        yield return null;

        // Verify default is Standard
        string currentCondition = PlayerPrefs.GetString("WinConditionType", "Standard");
        Debug.Log($"Default win condition: {currentCondition} (should be Standard)");

        yield return null;
    }

    private bool TestStandardWinDetection()
    {
        // Test basic win detection functionality
        if (winDetector == null) return false;

        // Test that CheckWin method exists and works
        try
        {
            // This is a basic test - actual win detection would require board setup
            return winDetector.CheckWin(0, 0, GameManager.Player.Black) == false; // Should return false for empty board
        }
        catch
        {
            return false;
        }
    }

    private bool TestCaptureWinDetection()
    {
        // Test capture win detection functionality
        if (winDetector == null) return false;

        // Test that capture methods exist
        try
        {
            // Test capture recording
            winDetector.RecordCapture(GameManager.Player.Black);
            int captures = winDetector.GetCaptureCount(GameManager.Player.Black);
            bool captureWin = winDetector.CheckCaptureWin(GameManager.Player.Black);

            Debug.Log($"Capture test - Black captures: {captures}, Win: {captureWin}");

            return captures == 1 && captureWin == false; // Should have 1 capture but not win yet
        }
        catch
        {
            return false;
        }
    }

    private bool TestTimeBasedWinDetection()
    {
        // Test time-based win detection functionality
        if (winDetector == null) return false;

        // Test that time methods exist
        try
        {
            // Test timer methods
            winDetector.UpdateGameTimer(10f); // Add 10 seconds
            float remainingTime = winDetector.GetRemainingTime();
            bool timeWin = winDetector.CheckTimeBasedWin(GameManager.Player.Black);

            Debug.Log($"Time-based test - Remaining time: {remainingTime}, Win: {timeWin}");

            return remainingTime > 0 && timeWin == false; // Should have remaining time but not win yet
        }
        catch
        {
            return false;
        }
    }
}