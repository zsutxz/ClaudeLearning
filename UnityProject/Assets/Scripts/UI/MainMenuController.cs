using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GomokuGame.Core;
using GomokuGame.Constants;

namespace GomokuGame.UI
{
    /// <summary>
    /// Controls the main menu functionality and interactions
    /// Supports both mouse and keyboard navigation for accessibility
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
        
        // Array of all interactive buttons for keyboard navigation
        private Button[] navigationButtons;
        private int currentFocusIndex = 0;
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
            
            // Initialize keyboard navigation
            InitializeKeyboardNavigation();
        }
        
        /// <summary>
        /// Sets up keyboard navigation for menu buttons
        /// </summary>
        private void InitializeKeyboardNavigation()
        {
            // Create array of all interactive buttons
            navigationButtons = new Button[] { startGameButton, settingsButton, exitGameButton };
            
            // Set up navigation for each button
            for (int i = 0; i < navigationButtons.Length; i++)
            {
                if (navigationButtons[i] != null)
                {
                    // Configure navigation to support keyboard tabbing
                    Navigation nav = navigationButtons[i].navigation;
                    nav.mode = Navigation.Mode.Explicit;
                    
                    // Set up next/previous navigation
                    int nextIndex = (i + 1) % navigationButtons.Length;
                    int prevIndex = (i - 1 + navigationButtons.Length) % navigationButtons.Length;
                    
                    nav.selectOnRight = navigationButtons[nextIndex];
                    nav.selectOnLeft = navigationButtons[prevIndex];
                    nav.selectOnDown = navigationButtons[nextIndex];
                    nav.selectOnUp = navigationButtons[prevIndex];
                    
                    navigationButtons[i].navigation = nav;
                }
            }
            
            // Set initial focus
            if (navigationButtons.Length > 0 && navigationButtons[0] != null)
            {
                navigationButtons[0].Select();
                currentFocusIndex = 0;
            }
        }
        #endregion
        
        #region Unity Lifecycle
        private void Update()
        {
            // Handle keyboard input for accessibility
            HandleKeyboardInput();
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Handles keyboard input for menu navigation and selection
        /// </summary>
        private void HandleKeyboardInput()
        {
            // Check for Enter or Space key to activate current button
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (navigationButtons != null && navigationButtons.Length > 0 && 
                    currentFocusIndex >= 0 && currentFocusIndex < navigationButtons.Length &&
                    navigationButtons[currentFocusIndex] != null)
                {
                    // Simulate button click
                    navigationButtons[currentFocusIndex].onClick.Invoke();
                }
            }
            
            // Check for Escape key to return from settings or exit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If in settings panel, return to main menu
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    ReturnToMainMenu();
                }
                // If in main menu, exit game (optional - could also just do nothing)
                else
                {
                    // Optionally exit game when pressing Escape on main menu
                    // ExitGame();
                }
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
            GameObject menuPanel = mainMenuPanel;
            GameObject settingsPanelObj = settingsPanel;
            
            // Validate required references
            if (menuPanel == null)
            {
                Debug.LogWarning("MainMenuPanel reference is not set in MainMenuController");
            }
            
            if (settingsPanelObj == null)
            {
                Debug.LogWarning("SettingsPanel reference is not set in MainMenuController");
            }
            
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
            
            // Reset focus when showing settings
            if (navigationButtons != null && navigationButtons.Length > 0 && navigationButtons[0] != null)
            {
                navigationButtons[0].Select();
                currentFocusIndex = 0;
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
            GameObject menuPanel = mainMenuPanel;
            GameObject settingsPanelObj = settingsPanel;
            
            // Validate required references
            if (menuPanel == null)
            {
                Debug.LogWarning("MainMenuPanel reference is not set in MainMenuController");
            }
            
            if (settingsPanelObj == null)
            {
                Debug.LogWarning("SettingsPanel reference is not set in MainMenuController");
            }
            
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
            
            // Restore focus when returning to main menu
            if (navigationButtons != null && navigationButtons.Length > 0 && navigationButtons[0] != null)
            {
                navigationButtons[0].Select();
                currentFocusIndex = 0;
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