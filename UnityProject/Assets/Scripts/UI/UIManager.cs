using UnityEngine;
using UnityEngine.UI;
using GomokuGame.Core;
using System.Collections;

namespace GomokuGame.UI
{
    /// <summary>
    /// Manages the user interface elements and their interactions
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        public static UIManager Instance { get; private set; }
        #endregion

        #region Fields
        [Header("Game UI Elements")]
        [SerializeField] private Text currentPlayerText;
        [SerializeField] private Text gameStateText;
        [SerializeField] private Text gameOverText;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject resultsScreenPanel;
        [SerializeField] private ResultsScreenController resultsScreenController;
        
        [Header("Game Controls")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        
        [Header("Main Menu Elements")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Header("Settings UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider boardSizeSlider;
        [SerializeField] private Slider winConditionSlider;
        [SerializeField] private Text boardSizeValueText;
        [SerializeField] private Text winConditionValueText;
        [SerializeField] private Button[] boardSizeButtons;
        [SerializeField] private Button boardSize9x9Button;
        [SerializeField] private Button boardSize13x13Button;
        [SerializeField] private Button boardSize15x15Button;
        [SerializeField] private Button boardSize19x19Button;
        [SerializeField] private Dropdown themeDropdown;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton pattern implementation
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
                
                // Add MainMenuSetup component if we're in the main menu scene
                if (gameObject.scene.name == "MainMenu" || gameObject.scene.name == "MainMenu.unity")
                {
                    if (GetComponent<MainMenuSetup>() == null)
                    {
                        gameObject.AddComponent<MainMenuSetup>();
                    }
                    
                    // Add MainMenuTest component for testing
                    if (GetComponent<MainMenuTest>() == null)
                    {
                        gameObject.AddComponent<MainMenuTest>();
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to game events
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
                gameManager.OnPlayerChanged += OnPlayerChanged;
                gameManager.OnGameWon += OnGameWon;
                gameManager.OnGameDraw += OnGameDraw;
            }
            
            // Subscribe to button click events
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
                AddButtonHoverEffects(restartButton);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
                AddButtonHoverEffects(mainMenuButton);
            }
            
            // Initialize UI state
            UpdatePlayerText();
            UpdateGameStateText();
            HideAllPanels();
            ShowPanel(mainMenuPanel);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows the main menu
        /// </summary>
        public void ShowMainMenu()
        {
            HideAllPanels();
            ShowPanel(mainMenuPanel);
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartGame()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // Load board size from PlayerPrefs
                int boardSize = PlayerPrefs.GetInt("BoardSize", 15); // Default to 15 if not set
                gameManager.SetBoardSize(boardSize);

                // Ensure theme is applied by initializing ThemeManager if it exists
                GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
                if (themeManager != null)
                {
                    // ThemeManager automatically loads the saved theme on Awake,
                    // so we just ensure it exists before starting the game
                }

                gameManager.StartNewGame();
            }
        }

        /// <summary>
        /// Restarts the current game
        /// </summary>
        public void RestartGame()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // Reset the board visuals
                var boardView = gameManager.GetBoardView();
                if (boardView != null)
                {
                    boardView.ClearVisuals();
                }

                // Reset win detector
                if (gameManager.winDetector != null)
                {
                    // Win detector will be reset with new game
                }

                // Ensure theme is applied by initializing ThemeManager if it exists
                GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
                if (themeManager != null)
                {
                    // ThemeManager automatically loads the saved theme on Awake,
                    // so we just ensure it exists before starting the game
                }

                // Start a new game with the same board size
                gameManager.StartNewGame();
            }
        }

        /// <summary>
        /// Returns to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // Clear the board visuals before returning to main menu
                var boardView = gameManager.GetBoardView();
                if (boardView != null)
                {
                    boardView.ClearVisuals();
                }

                gameManager.ReturnToMainMenu();
            }
        }

        /// <summary>
        /// Shows the settings panel
        /// </summary>
        public void ShowSettings()
        {
            HideAllPanels();
            ShowPanel(settingsPanel);
            LoadSettingsValues();
        }

        /// <summary>
        /// Saves the current settings
        /// </summary>
        public void SaveSettings()
        {
            if (boardSizeSlider != null && winConditionSlider != null)
            {
                int boardSize = (int)boardSizeSlider.value;
                int winCondition = (int)winConditionSlider.value;

                // Save to PlayerPrefs
                PlayerPrefs.SetInt("BoardSize", boardSize);
                PlayerPrefs.SetInt("WinCondition", winCondition);
                
                // Save theme selection
                if (themeDropdown != null)
                {
                    PlayerPrefs.SetString("BoardTheme", themeDropdown.options[themeDropdown.value].text);
                }
                
                PlayerPrefs.Save();

                // Update GameManager if it exists
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.SetBoardSize(boardSize);
                }
            }

            ShowMainMenu();
        }

        /// <summary>
        /// Cancels settings changes and returns to main menu
        /// </summary>
        public void CancelSettings()
        {
            ShowMainMenu();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the UI manager
        /// </summary>
        private void Initialize()
        {
            SetupSliders();
            SetupBoardSizeButtons();
        }

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

        /// <summary>
        /// Handles board size button clicks
        /// </summary>
        /// <param name="index">Button index</param>
        private void OnBoardSizeButtonClicked(int index)
        {
            // Map button index to board size
            int[] boardSizes = { 9, 13, 15, 19 };
            if (index >= 0 && index < boardSizes.Length && boardSizeSlider != null)
            {
                boardSizeSlider.value = boardSizes[index];
            }
        }
        
        /// <summary>
        /// Sets the board size from a specific board size button
        /// </summary>
        /// <param name="size">Board size</param>
        private void SetBoardSizeFromButton(int size)
        {
            if (boardSizeSlider != null)
            {
                boardSizeSlider.value = size;
            }
            UpdateBoardSizeText(size);
        }

        /// <summary>
        /// Sets up the slider value displays
        /// </summary>
        private void SetupSliders()
        {
            if (boardSizeSlider != null)
            {
                boardSizeSlider.onValueChanged.AddListener(OnBoardSizeSliderChanged);
            }
            
            if (winConditionSlider != null)
            {
                winConditionSlider.onValueChanged.AddListener(OnWinConditionSliderChanged);
            }
        }

        /// <summary>
        /// Sets up the board size buttons
        /// </summary>
        private void SetupBoardSizeButtons()
        {
            // Setup the existing slider-based buttons if they exist
            if (boardSizeButtons != null && boardSizeButtons.Length > 0)
            {
                for (int i = 0; i < boardSizeButtons.Length; i++)
                {
                    int index = i; // Capture index for closure
                    boardSizeButtons[i].onClick.AddListener(() => OnBoardSizeButtonClicked(index));
                    AddButtonHoverEffects(boardSizeButtons[i]);
                }
            }
            
            // Setup the new specific board size buttons
            if (boardSize9x9Button != null)
            {
                boardSize9x9Button.onClick.AddListener(() => SetBoardSizeFromButton(9));
                AddButtonHoverEffects(boardSize9x9Button);
            }
            
            if (boardSize13x13Button != null)
            {
                boardSize13x13Button.onClick.AddListener(() => SetBoardSizeFromButton(13));
                AddButtonHoverEffects(boardSize13x13Button);
            }
            
            if (boardSize15x15Button != null)
            {
                boardSize15x15Button.onClick.AddListener(() => SetBoardSizeFromButton(15));
                AddButtonHoverEffects(boardSize15x15Button);
            }
            
            if (boardSize19x19Button != null)
            {
                boardSize19x19Button.onClick.AddListener(() => SetBoardSizeFromButton(19));
                AddButtonHoverEffects(boardSize19x19Button);
            }
        }

        /// <summary>
        /// Updates the current player text display
        /// </summary>
        private void UpdatePlayerText()
        {
            if (currentPlayerText != null)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    string player = gameManager.currentPlayer == GameManager.Player.Black ? "Black" : "White";
                    currentPlayerText.text = $"Current Player: {player}";

                    // Apply distinct visual styling for each player with theme support
                    GomokuGame.Themes.ThemeSettings themeSettings = null;
                    GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
                    if (themeManager != null)
                    {
                        themeSettings = themeManager.GetCurrentThemeSettings();
                    }

                    if (gameManager.currentPlayer == GameManager.Player.Black)
                    {
                        currentPlayerText.color = (themeSettings != null) ? themeSettings.blackPieceColor : Color.black;
                    }
                    else
                    {
                        currentPlayerText.color = (themeSettings != null) ? themeSettings.whitePieceColor : Color.white;
                        // Apply outline effect for better visibility
                        Outline outline = currentPlayerText.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.effectColor = (themeSettings != null) ? themeSettings.boardLineColor : Color.black;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the game state text display
        /// </summary>
        private void UpdateGameStateText()
        {
            if (gameStateText != null)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    switch (gameManager.currentState)
                    {
                        case GameManager.GameState.MainMenu:
                            gameStateText.text = "Game State: Main Menu";
                            break;
                        case GameManager.GameState.Playing:
                            gameStateText.text = "Game State: Playing";
                            break;
                        case GameManager.GameState.Paused:
                            gameStateText.text = "Game State: Paused";
                            break;
                        case GameManager.GameState.GameOver:
                            gameStateText.text = "Game State: Game Over";
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Provides visual feedback for player changes
        /// </summary>
        /// <param name="player">New player</param>
        private void ProvideVisualFeedbackForPlayerChange(GameManager.Player player)
        {
            // Simple visual feedback - could be enhanced with animations
            if (currentPlayerText != null)
            {
                // Flash the text to indicate change
                StartCoroutine(FlashText(currentPlayerText));
            }
        }

        /// <summary>
        /// Flashes the text to provide visual feedback
        /// </summary>
        /// <param name="text">Text to flash</param>
        /// <returns>Coroutine</returns>
        private IEnumerator FlashText(Text text)
        {
            Color originalColor = text.color;
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(0.1f);
            text.color = originalColor;
        }

        /// <summary>
        /// Hides all UI panels
        /// </summary>
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameUIPanel != null) gameUIPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Shows a specific panel
        /// </summary>
        /// <param name="panel">Panel to show</param>
        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// Loads current settings values into UI elements
        /// </summary>
        private void LoadSettingsValues()
        {
            int boardSize = PlayerPrefs.GetInt("BoardSize", 15);
            int winCondition = PlayerPrefs.GetInt("WinCondition", 5);

            // Validate board size is within acceptable range
            if (boardSize < 9) boardSize = 9;
            if (boardSize > 19) boardSize = 19;

            if (boardSizeSlider != null)
            {
                boardSizeSlider.minValue = 9;
                boardSizeSlider.maxValue = 19;
                boardSizeSlider.value = boardSize;
                UpdateBoardSizeText(boardSize);
            }

            if (winConditionSlider != null)
            {
                winConditionSlider.value = winCondition;
                UpdateWinConditionText(winCondition);
            }

            // Load theme selection
            if (themeDropdown != null)
            {
                string savedTheme = PlayerPrefs.GetString("BoardTheme", "Classic");
                for (int i = 0; i < themeDropdown.options.Count; i++)
                {
                    if (themeDropdown.options[i].text == savedTheme)
                    {
                        themeDropdown.value = i;
                        break;
                    }
                }
            }

            // Update visual feedback for board size buttons
            UpdateBoardSizeButtonSelection(boardSize);

            // Apply theme to settings UI elements
            ApplyThemeToSettingsUI();
        }

        /// <summary>
        /// Applies the current theme to settings UI elements
        /// </summary>
        private void ApplyThemeToSettingsUI()
        {
            // Get the current theme settings
            GomokuGame.Themes.ThemeSettings themeSettings = null;
            GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
            if (themeManager != null)
            {
                themeSettings = themeManager.GetCurrentThemeSettings();
            }

            // Apply theme to slider handles and backgrounds if they exist
            if (boardSizeSlider != null)
            {
                // Update slider colors based on theme
                Color handleColor = (themeSettings != null) ? themeSettings.blackPieceColor : Color.black;
                Color backgroundColor = (themeSettings != null) ? themeSettings.boardLineColor : Color.gray;

                // Note: Slider color customization would require more complex UI element access
                // For now, we'll rely on the default Unity slider appearance
            }

            if (winConditionSlider != null)
            {
                // Update slider colors based on theme
                Color handleColor = (themeSettings != null) ? themeSettings.blackPieceColor : Color.black;
                Color backgroundColor = (themeSettings != null) ? themeSettings.boardLineColor : Color.gray;

                // Note: Slider color customization would require more complex UI element access
                // For now, we'll rely on the default Unity slider appearance
            }

            // Apply theme to dropdown if it exists
            if (themeDropdown != null)
            {
                // Update dropdown colors based on theme
                // Note: Dropdown color customization would require more complex UI element access
                // For now, we'll rely on the default Unity dropdown appearance
            }
        }

        /// <summary>
        /// Updates the board size text display
        /// </summary>
        /// <param name="size">Board size value</param>
        private void UpdateBoardSizeText(int size)
        {
            if (boardSizeValueText != null)
            {
                boardSizeValueText.text = $"{size}x{size}";
            }
            
            // Update visual feedback for board size buttons
            UpdateBoardSizeButtonSelection(size);
        }
        
        /// <summary>
        /// Updates the visual feedback for board size buttons
        /// </summary>
        /// <param name="selectedSize">Selected board size</param>
        private void UpdateBoardSizeButtonSelection(int selectedSize)
        {
            // Reset all buttons to normal state
            if (boardSize9x9Button != null)
            {
                ColorBlock colors9x9 = boardSize9x9Button.colors;
                colors9x9.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                boardSize9x9Button.colors = colors9x9;
            }
            
            if (boardSize13x13Button != null)
            {
                ColorBlock colors13x13 = boardSize13x13Button.colors;
                colors13x13.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                boardSize13x13Button.colors = colors13x13;
            }
            
            if (boardSize15x15Button != null)
            {
                ColorBlock colors15x15 = boardSize15x15Button.colors;
                colors15x15.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                boardSize15x15Button.colors = colors15x15;
            }
            
            if (boardSize19x19Button != null)
            {
                ColorBlock colors19x19 = boardSize19x19Button.colors;
                colors19x19.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                boardSize19x19Button.colors = colors19x19;
            }
            
            // Highlight the selected button
            switch (selectedSize)
            {
                case 9:
                    if (boardSize9x9Button != null)
                    {
                        ColorBlock colors9x9 = boardSize9x9Button.colors;
                        colors9x9.normalColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Lighter color for selection
                        boardSize9x9Button.colors = colors9x9;
                    }
                    break;
                case 13:
                    if (boardSize13x13Button != null)
                    {
                        ColorBlock colors13x13 = boardSize13x13Button.colors;
                        colors13x13.normalColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Lighter color for selection
                        boardSize13x13Button.colors = colors13x13;
                    }
                    break;
                case 15:
                    if (boardSize15x15Button != null)
                    {
                        ColorBlock colors15x15 = boardSize15x15Button.colors;
                        colors15x15.normalColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Lighter color for selection
                        boardSize15x15Button.colors = colors15x15;
                    }
                    break;
                case 19:
                    if (boardSize19x19Button != null)
                    {
                        ColorBlock colors19x19 = boardSize19x19Button.colors;
                        colors19x19.normalColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Lighter color for selection
                        boardSize19x19Button.colors = colors19x19;
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates the win condition text display
        /// </summary>
        /// <param name="condition">Win condition value</param>
        private void UpdateWinConditionText(int condition)
        {
            if (winConditionValueText != null)
            {
                winConditionValueText.text = $"{condition} in a row";
            }
        }

        /// <summary>
        /// Provides visual feedback for state transitions
        /// </summary>
        /// <param name="state">New game state</param>
        private void ProvideVisualFeedbackForStateTransition(GameManager.GameState state)
        {
            // Simple visual feedback - could be enhanced with animations
            if (gameStateText != null)
            {
                // Get theme settings for color customization
                GomokuGame.Themes.ThemeSettings themeSettings = null;
                GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
                if (themeManager != null)
                {
                    themeSettings = themeManager.GetCurrentThemeSettings();
                }

                // Change color based on state with theme support
                switch (state)
                {
                    case GameManager.GameState.MainMenu:
                        gameStateText.color = (themeSettings != null) ? themeSettings.boardLineColor : Color.blue;
                        break;
                    case GameManager.GameState.Playing:
                        // Use a green color that works well with the theme
                        gameStateText.color = new Color(0.2f, 0.8f, 0.2f); // Bright green
                        break;
                    case GameManager.GameState.Paused:
                        // Use a yellow color that works well with the theme
                        gameStateText.color = new Color(0.9f, 0.9f, 0.2f); // Bright yellow
                        break;
                    case GameManager.GameState.GameOver:
                        // Use a red color that works well with the theme
                        gameStateText.color = new Color(0.9f, 0.2f, 0.2f); // Bright red
                        break;
                }

                // Optional: Add a simple scale animation
                // This would require adding animation components
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles game state changes
        /// </summary>
        /// <param name="state">New game state</param>
        private void OnGameStateChanged(GameManager.GameState state)
        {
            HideAllPanels();
            UpdateGameStateText();
            ProvideVisualFeedbackForStateTransition(state);

            switch (state)
            {
                case GameManager.GameState.MainMenu:
                    ShowPanel(mainMenuPanel);
                    // Apply theme to main menu UI when returning to main menu
                    if (mainMenuPanel != null)
                    {
                        GomokuGame.UI.MainMenuSetup mainMenuSetup = FindObjectOfType<GomokuGame.UI.MainMenuSetup>();
                        if (mainMenuSetup != null)
                        {
                            mainMenuSetup.ApplyThemeToUI(mainMenuPanel);
                        }
                    }
                    break;
                case GameManager.GameState.Playing:
                    ShowPanel(gameUIPanel);
                    UpdatePlayerText();
                    break;
                case GameManager.GameState.Paused:
                    // Could show a pause overlay
                    break;
                case GameManager.GameState.GameOver:
                    // Show the results screen panel instead of the simple gameOverPanel
                    if (resultsScreenPanel != null)
                    {
                        HideAllPanels();
                        ShowPanel(resultsScreenPanel);
                        // Apply theme to results screen UI
                        GomokuGame.UI.ResultsScreenController resultsController = FindObjectOfType<GomokuGame.UI.ResultsScreenController>();
                        if (resultsController != null)
                        {
                            resultsController.ApplyThemeToResultsUI();
                        }
                    }
                    else
                    {
                        ShowPanel(gameOverPanel);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles player changes
        /// </summary>
        /// <param name="player">New current player</param>
        private void OnPlayerChanged(GameManager.Player player)
        {
            UpdatePlayerText();
            ProvideVisualFeedbackForPlayerChange(player);
        }

        /// <summary>
        /// Handles game win events
        /// </summary>
        /// <param name="winner">Winning player</param>
        private void OnGameWon(GameManager.Player winner)
        {
            // Update the simple gameOverText for backward compatibility
            if (gameOverText != null)
            {
                string winnerName = winner == GameManager.Player.Black ? "Black" : "White";
                gameOverText.text = $"{winnerName} Player Wins!";
            }
            
            // Update the results screen controller if available
            if (resultsScreenController != null)
            {
                int winnerId = winner == GameManager.Player.Black ? 1 : 2;
                resultsScreenController.DisplayWinner(winnerId);
            }
        }

        /// <summary>
        /// Handles game draw events
        /// </summary>
        private void OnGameDraw()
        {
            // Update the simple gameOverText for backward compatibility
            if (gameOverText != null)
            {
                gameOverText.text = "Game Draw!";
            }
            
            // Update the results screen controller if available
            if (resultsScreenController != null)
            {
                resultsScreenController.DisplayWinner(0); // 0 indicates a draw
            }
        }

        /// <summary>
        /// Handles board size slider changes
        /// </summary>
        /// <param name="value">Slider value</param>
        private void OnBoardSizeSliderChanged(float value)
        {
            UpdateBoardSizeText((int)value);
        }

        /// <summary>
        /// Handles win condition slider changes
        /// </summary>
        /// <param name="value">Slider value</param>
        private void OnWinConditionSliderChanged(float value)
        {
            UpdateWinConditionText((int)value);
        }
        #endregion
    }
}


