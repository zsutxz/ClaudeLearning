using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GomokuGame.UI
{
    /// <summary>
    /// Test script to verify main menu functionality
    /// </summary>
    public class MainMenuTest : MonoBehaviour
    {
        void Start()
        {
            // Test that all components are properly set up
            TestMainMenuSetup();
        }
        
        /// <summary>
        /// Tests that the main menu is properly set up
        /// </summary>
        private void TestMainMenuSetup()
        {
            Debug.Log("Testing main menu setup...");
            
            // Check if UIManager exists
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("UIManager found: SUCCESS");
            }
            else
            {
                Debug.LogError("UIManager not found: FAILED");
                return;
            }
            
            // Check if GameManager exists
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("GameManager found: SUCCESS");
            }
            else
            {
                Debug.LogError("GameManager not found: FAILED");
                return;
            }
            
            // Check if Canvas exists
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log("Canvas found: SUCCESS");
            }
            else
            {
                Debug.LogError("Canvas not found: FAILED");
                return;
            }
            
            // Check if main menu panel exists
            GameObject mainMenuPanel = GameObject.Find("MainMenuPanel");
            if (mainMenuPanel != null)
            {
                Debug.Log("MainMenuPanel found: SUCCESS");
            }
            else
            {
                Debug.LogError("MainMenuPanel not found: FAILED");
                return;
            }
            
            // Check if settings panel exists
            GameObject settingsPanel = GameObject.Find("SettingsPanel");
            if (settingsPanel != null)
            {
                Debug.Log("SettingsPanel found: SUCCESS");
            }
            else
            {
                Debug.LogError("SettingsPanel not found: FAILED");
                return;
            }
            
            // Check if main menu buttons exist
            Button startButton = FindButtonByName(mainMenuPanel, "StartGameButton");
            if (startButton != null)
            {
                Debug.Log("StartGameButton found: SUCCESS");
            }
            else
            {
                Debug.LogError("StartGameButton not found: FAILED");
                return;
            }
            
            Button settingsButton = FindButtonByName(mainMenuPanel, "SettingsButton");
            if (settingsButton != null)
            {
                Debug.Log("SettingsButton found: SUCCESS");
            }
            else
            {
                Debug.LogError("SettingsButton not found: FAILED");
                return;
            }
            
            Button exitButton = FindButtonByName(mainMenuPanel, "ExitGameButton");
            if (exitButton != null)
            {
                Debug.Log("ExitGameButton found: SUCCESS");
            }
            else
            {
                Debug.LogError("ExitGameButton not found: FAILED");
                return;
            }
            
            // Check if settings buttons exist
            Button saveButton = FindButtonByName(settingsPanel, "SaveButton");
            if (saveButton != null)
            {
                Debug.Log("SaveButton found: SUCCESS");
            }
            else
            {
                Debug.LogWarning("SaveButton not found: WARNING");
            }
            
            Button cancelButton = FindButtonByName(settingsPanel, "CancelButton");
            if (cancelButton != null)
            {
                Debug.Log("CancelButton found: SUCCESS");
            }
            else
            {
                Debug.LogWarning("CancelButton not found: WARNING");
            }
            
            Debug.Log("Main menu setup test completed: ALL TESTS PASSED");
            
            // Test button functionality
            TestButtonFunctionality(startButton, settingsButton, exitButton, cancelButton);
        }
        
        /// <summary>
        /// Tests the functionality of main menu buttons
        /// </summary>
        /// <param name="startButton">Start game button</param>
        /// <param name="settingsButton">Settings button</param>
        /// <param name="exitButton">Exit game button</param>
        /// <param name="cancelButton">Cancel button</param>
        private void TestButtonFunctionality(Button startButton, Button settingsButton, Button exitButton, Button cancelButton)
        {
            Debug.Log("Testing button functionality...");
            
            // Test that MainMenuController exists
            MainMenuController controller = FindObjectOfType<MainMenuController>();
            if (controller != null)
            {
                Debug.Log("MainMenuController found: SUCCESS");
                
                // Test that buttons have click listeners
                if (startButton.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("StartGameButton has click listener: SUCCESS");
                }
                else
                {
                    Debug.LogWarning("StartGameButton has no click listener: WARNING");
                }
                
                if (settingsButton.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("SettingsButton has click listener: SUCCESS");
                }
                else
                {
                    Debug.LogWarning("SettingsButton has no click listener: WARNING");
                }
                
                if (exitButton.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("ExitGameButton has click listener: SUCCESS");
                }
                else
                {
                    Debug.LogWarning("ExitGameButton has no click listener: WARNING");
                }
                
                if (cancelButton != null && cancelButton.onClick.GetPersistentEventCount() > 0)
                {
                    Debug.Log("CancelButton has click listener: SUCCESS");
                }
                else
                {
                    Debug.LogWarning("CancelButton has no click listener: WARNING");
                }
            }
            else
            {
                Debug.LogError("MainMenuController not found: FAILED");
            }
            
            Debug.Log("Button functionality test completed");
        }
        
        /// <summary>
        /// Finds a button by name in the hierarchy
        /// </summary>
        /// <param name="parent">Parent object to search in</param>
        /// <param name="name">Name of the button to find</param>
        /// <returns>Button component if found, null otherwise</returns>
        private Button FindButtonByName(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                return child.GetComponent<Button>();
            }
            return null;
        }
    }
}