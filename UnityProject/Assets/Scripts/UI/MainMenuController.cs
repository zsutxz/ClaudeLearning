using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GomokuGame.Core;

namespace GomokuGame.UI
{
    /// <summary>
    /// Controls the main menu functionality and interactions
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        #region Fields
        [Header("UI Elements")]
        [SerializeField] public Button startGameButton;
        [SerializeField] public Button settingsButton;
        [SerializeField] public Button exitGameButton;
        [SerializeField] public GameObject settingsPanel;
        [SerializeField] public GameObject mainMenuPanel;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Initialize button click listeners
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(StartGame);
                AddButtonHoverEffects(startGameButton);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ShowSettings);
                AddButtonHoverEffects(settingsButton);
            }

            if (exitGameButton != null)
            {
                exitGameButton.onClick.AddListener(ExitGame);
                AddButtonHoverEffects(exitGameButton);
            }
            
            // Ensure settings panel is hidden at start
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a new game by loading the game scene
        /// </summary>
        public void StartGame()
        {
            // Using UIManager to handle scene transition
            if (UIManager.Instance != null)
            {
                UIManager.Instance.StartGame();
            }
            
            // Also ensure we load the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Shows the settings panel
        /// </summary>
        public void ShowSettings()
        {
            GameObject menuPanel = mainMenuPanel != null ? mainMenuPanel : GameObject.Find("MainMenuPanel");
            GameObject settingsPanelObj = settingsPanel != null ? settingsPanel : GameObject.Find("SettingsPanel");
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
            
            if (settingsPanelObj != null)
            {
                settingsPanelObj.SetActive(true);
            }
            
            // Using UIManager to handle settings
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSettings();
            }
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Returns to the main menu from settings
        /// </summary>
        public void ReturnToMainMenu()
        {
            GameObject menuPanel = mainMenuPanel != null ? mainMenuPanel : GameObject.Find("MainMenuPanel");
            GameObject settingsPanelObj = settingsPanel != null ? settingsPanel : GameObject.Find("SettingsPanel");
            
            if (settingsPanelObj != null)
            {
                settingsPanelObj.SetActive(false);
            }
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
            }
            
            // Using UIManager to handle main menu
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds hover effects to a button
        /// </summary>
        /// <param name="button">Button to add effects to</param>
        private void AddButtonHoverEffects(Button button)
        {
            // Add hover color change effect
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray when hovered
            colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Darker gray when pressed
            button.colors = colors;
        }
        #endregion
    }
}