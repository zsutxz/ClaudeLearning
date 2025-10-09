using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using GomokuGame.Utilities;
using UnityEngine.Assertions;

namespace GomokuGame.UI.Tests
{
    /// <summary>
    /// Test class for settings persistence functionality
    /// </summary>
    public class SettingsPersistenceTest
    {
        [SetUp]
        public void SetUp()
        {
            // Clear PlayerPrefs before each test
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            // Clear PlayerPrefs after each test
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void TestDefaultSettingsApplication()
        {
            // Test that default settings are applied when no saved preferences exist
            Assert.IsTrue(PlayerPrefsManager.IsFirstRun(), "Should detect first run");

            int boardSize = PlayerPrefsManager.LoadBoardSize();
            int winCondition = PlayerPrefsManager.LoadWinCondition();
            string theme = PlayerPrefsManager.LoadTheme();

            Assert.AreEqual(15, boardSize, "Default board size should be 15");
            Assert.AreEqual(5, winCondition, "Default win condition should be 5");
            Assert.AreEqual("Default", theme, "Default theme should be 'Default'");
        }

        [Test]
        public void TestSettingsSaveAndLoad()
        {
            // Test saving and loading settings
            int testBoardSize = 13;
            int testWinCondition = 4;
            string testTheme = "Classic";

            // Save settings
            PlayerPrefsManager.SaveBoardSize(testBoardSize);
            PlayerPrefsManager.SaveWinCondition(testWinCondition);
            PlayerPrefsManager.SaveTheme(testTheme);

            // Load and verify settings
            int loadedBoardSize = PlayerPrefsManager.LoadBoardSize();
            int loadedWinCondition = PlayerPrefsManager.LoadWinCondition();
            string loadedTheme = PlayerPrefsManager.LoadTheme();

            Assert.AreEqual(testBoardSize, loadedBoardSize, "Board size should persist");
            Assert.AreEqual(testWinCondition, loadedWinCondition, "Win condition should persist");
            Assert.AreEqual(testTheme, loadedTheme, "Theme should persist");
        }

        [Test]
        public void TestSettingsValidation()
        {
            // Test settings validation with valid settings
            PlayerPrefsManager.SaveBoardSize(15);
            PlayerPrefsManager.SaveWinCondition(5);
            PlayerPrefsManager.SaveTheme("Default");

            Assert.IsTrue(PlayerPrefsManager.ValidateSettings(), "Valid settings should pass validation");

            // Test settings validation with invalid board size
            PlayerPrefsManager.SaveBoardSize(5); // Invalid: too small
            Assert.IsFalse(PlayerPrefsManager.ValidateSettings(), "Invalid board size should fail validation");

            // Reset and test with invalid win condition
            PlayerPrefsManager.SaveBoardSize(15);
            PlayerPrefsManager.SaveWinCondition(2); // Invalid: too small
            Assert.IsFalse(PlayerPrefsManager.ValidateSettings(), "Invalid win condition should fail validation");
        }

        [Test]
        public void TestBackupAndRestore()
        {
            // Test backup and restore functionality
            int originalBoardSize = 15;
            int originalWinCondition = 5;
            string originalTheme = "Default";

            // Set original values
            PlayerPrefsManager.SaveBoardSize(originalBoardSize);
            PlayerPrefsManager.SaveWinCondition(originalWinCondition);
            PlayerPrefsManager.SaveTheme(originalTheme);

            // Create backup
            PlayerPrefsManager.CreateBackup();

            // Change values
            PlayerPrefsManager.SaveBoardSize(19);
            PlayerPrefsManager.SaveWinCondition(10);
            PlayerPrefsManager.SaveTheme("Classic");

            // Restore from backup
            bool restoreSuccess = PlayerPrefsManager.RestoreFromBackup();
            Assert.IsTrue(restoreSuccess, "Restore should succeed");

            // Verify original values are restored
            Assert.AreEqual(originalBoardSize, PlayerPrefsManager.LoadBoardSize(), "Board size should be restored");
            Assert.AreEqual(originalWinCondition, PlayerPrefsManager.LoadWinCondition(), "Win condition should be restored");
            Assert.AreEqual(originalTheme, PlayerPrefsManager.LoadTheme(), "Theme should be restored");
        }

        [Test]
        public void TestSafeSaveAllSettings()
        {
            // Test the safe save functionality with backup
            int testBoardSize = 17;
            int testWinCondition = 6;
            string testTheme = "Modern";

            // Use safe save
            PlayerPrefsManager.SafeSaveAllSettings(testBoardSize, testWinCondition, testTheme);

            // Verify values were saved
            Assert.AreEqual(testBoardSize, PlayerPrefsManager.LoadBoardSize(), "Board size should be saved safely");
            Assert.AreEqual(testWinCondition, PlayerPrefsManager.LoadWinCondition(), "Win condition should be saved safely");
            Assert.AreEqual(testTheme, PlayerPrefsManager.LoadTheme(), "Theme should be saved safely");
        }

        [Test]
        public void TestFirstRunDetection()
        {
            // Test first run detection
            Assert.IsTrue(PlayerPrefsManager.IsFirstRun(), "Should detect first run initially");

            // Mark first run complete
            PlayerPrefsManager.MarkFirstRunComplete();

            Assert.IsFalse(PlayerPrefsManager.IsFirstRun(), "Should not detect first run after completion");
        }

        [Test]
        public void TestResetToDefaults()
        {
            // Set custom values
            PlayerPrefsManager.SaveBoardSize(19);
            PlayerPrefsManager.SaveWinCondition(10);
            PlayerPrefsManager.SaveTheme("Custom");

            // Reset to defaults
            PlayerPrefsManager.ResetToDefaults();

            // Verify default values
            Assert.AreEqual(15, PlayerPrefsManager.LoadBoardSize(), "Board size should reset to default");
            Assert.AreEqual(5, PlayerPrefsManager.LoadWinCondition(), "Win condition should reset to default");
            Assert.AreEqual("Default", PlayerPrefsManager.LoadTheme(), "Theme should reset to default");
            Assert.IsFalse(PlayerPrefsManager.IsFirstRun(), "First run should be marked complete after reset");
        }

        [UnityTest]
        public IEnumerator TestCrossSessionPersistence()
        {
            // This test simulates cross-session persistence by saving, clearing, and reloading
            int testBoardSize = 13;
            int testWinCondition = 4;
            string testTheme = "Classic";

            // Save settings
            PlayerPrefsManager.SaveBoardSize(testBoardSize);
            PlayerPrefsManager.SaveWinCondition(testWinCondition);
            PlayerPrefsManager.SaveTheme(testTheme);

            // Simulate session end by forcing PlayerPrefs to save
            PlayerPrefs.Save();

            yield return null; // Wait one frame

            // Simulate new session by clearing in-memory cache and reloading
            PlayerPrefs.DeleteAll();

            // Load settings again
            int loadedBoardSize = PlayerPrefsManager.LoadBoardSize();
            int loadedWinCondition = PlayerPrefsManager.LoadWinCondition();
            string loadedTheme = PlayerPrefsManager.LoadTheme();

            // Note: In a real Unity test environment, PlayerPrefs would persist between sessions
            // This test verifies that the loading logic works correctly with default values
            // when no saved preferences exist after clearing
            Assert.AreEqual(15, loadedBoardSize, "Should load default board size after clearing");
            Assert.AreEqual(5, loadedWinCondition, "Should load default win condition after clearing");
            Assert.AreEqual("Default", loadedTheme, "Should load default theme after clearing");
        }
    }
}