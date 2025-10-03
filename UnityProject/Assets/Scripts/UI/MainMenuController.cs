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
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitGameButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject mainMenuPanel;
        
        // Public properties for MainMenuSetup to set
        public Button startGameButton { get; set; }
        public Button settingsButton { get; set; }
        public Button exitGameButton { get; set; }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Use public properties if serialized fields are not set
            Button startBtn = startGameButton != null ? startGameButton : this.startGameButton;
            Button settingsBtn = settingsButton != null ? settingsButton : this.settingsButton;
            Button exitBtn = exitGameButton != null ? exitGameButton : this.exitGameButton;
            
            // Initialize button click listeners
            if (startBtn != null)
            {
                startBtn.onClick.AddListener(StartGame);
                AddButtonHoverEffects(startBtn);
            }
            
            if (settingsBtn != null)
            {
                settingsBtn.onClick.AddListener(ShowSettings);
                AddButtonHoverEffects(settingsBtn);
            }
            
            if (exitBtn != null)
            {
                exitBtn.onClick.AddListener(ExitGame);
                AddButtonHoverEffects(exitBtn);
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