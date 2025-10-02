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
        
        [Header("Main Menu Elements")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Header("Settings UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider boardSizeSlider;
        [SerializeField] private Slider winConditionSlider;
        [SerializeField] private Text boardSizeValueText;
        [SerializeField] private Text winConditionValueText;
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
                // Simple restart implementation
                gameManager.ReturnToMainMenu();
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
                PlayerPrefs.Save();
                
                // Update GameManager if it exists
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    // Note: UpdateSettings method needs to be implemented in GameManager
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
                    
                    // Apply distinct visual styling for each player
                    if (gameManager.currentPlayer == GameManager.Player.Black)
                    {
                        currentPlayerText.color = Color.black;
                    }
                    else
                    {
                        currentPlayerText.color = Color.white;
                        currentPlayerText.GetComponent<Outline>().effectColor = Color.black;
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
            
            if (boardSizeSlider != null)
            {
                boardSizeSlider.value = boardSize;
                UpdateBoardSizeText(boardSize);
            }
            
            if (winConditionSlider != null)
            {
                winConditionSlider.value = winCondition;
                UpdateWinConditionText(winCondition);
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
                // Change color based on state
                switch (state)
                {
                    case GameManager.GameState.MainMenu:
                        gameStateText.color = Color.blue;
                        break;
                    case GameManager.GameState.Playing:
                        gameStateText.color = Color.green;
                        break;
                    case GameManager.GameState.Paused:
                        gameStateText.color = Color.yellow;
                        break;
                    case GameManager.GameState.GameOver:
                        gameStateText.color = Color.red;
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
                    break;
                case GameManager.GameState.Playing:
                    ShowPanel(gameUIPanel);
                    UpdatePlayerText();
                    break;
                case GameManager.GameState.Paused:
                    // Could show a pause overlay
                    break;
                case GameManager.GameState.GameOver:
                    ShowPanel(gameOverPanel);
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
            if (gameOverText != null)
            {
                string winnerName = winner == GameManager.Player.Black ? "Black" : "White";
                gameOverText.text = $"{winnerName} Player Wins!";
            }
        }

        /// <summary>
        /// Handles game draw events
        /// </summary>
        private void OnGameDraw()
        {
            if (gameOverText != null)
            {
                gameOverText.text = "Game Draw!";
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