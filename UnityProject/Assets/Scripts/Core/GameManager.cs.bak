using UnityEngine;
using System;
using GomokuGame.Core;
using GomokuGame.UI;
using GomokuGame.Utilities;
using GomokuGame.UI;
namespace GomokuGame.Core{
public class GameManager : MonoBehaviour
{
    public enum Player
    {
        None = 0,
        Black = 1,
        White = 2
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public enum WinConditionType
    {
        FiveInRow,
        FiveInRowNoOverlines,
        FiveInRowFreeThree
    }

    [Header("Game State")]
    public Player currentPlayer = Player.Black;
    public GameState currentState = GameState.MainMenu;

    [Header("Board Settings")]
    public GomokuGame.Core.BoardManager boardManager;
    public WinDetector winDetector;
    [SerializeField] private int selectedBoardSize = 15; // Default board size
    [SerializeField] private string selectedWinConditionType = "Standard"; // Default win condition
        internal GameState gameState;

        // Events
        public event Action<GameState> OnGameStateChanged;
    public event Action<Player> OnPlayerChanged;
    public event Action<Player> OnGameWon;
    public event Action OnGameDraw;

    void Start()
    {
        // Initialize the board view
        if (boardManager != null)
        {
            var boardView = boardManager.GetComponent<GomokuGame.UI.BoardViewManager>();
            if (boardView != null)
            {
                boardView.InitializeBoard();
            }
        }

        // Subscribe to capture events if board manager exists
        if (boardManager != null)
        {
            boardManager.OnCaptureMade += OnCaptureMade;
        }

        // Ensure default settings are applied if this is the first run
        EnsureDefaultSettings();
    }

    void Update()
    {
        // Update game timer for time-based win conditions
        if (currentState == GameState.Playing && winDetector != null)
        {
            winDetector.UpdateGameTimer(Time.deltaTime);
        }
    }

    public void SwitchPlayer()
    {
        currentPlayer = currentPlayer == Player.Black ? Player.White : Player.Black;
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    public bool CheckWin(int x, int y)
    {
        if (winDetector != null)
        {
            return winDetector.CheckWin(x, y, currentPlayer);
        }
        return false;
    }

    public void StartNewGame()
    {
        // Load settings from PlayerPrefs using PlayerPrefsManager
        selectedBoardSize = PlayerPrefsManager.LoadBoardSize();
        selectedWinConditionType = PlayerPrefs.GetString("WinConditionType", "Standard");

        // Initialize board with selected size
        if (boardManager != null)
        {
            boardManager.InitializeBoard(selectedBoardSize);
        }

        // Reset win detector with selected win condition
        if (winDetector != null)
        {
            // Set the win condition type
            winDetector.SetWinConditionType(selectedWinConditionType);
            winDetector.ResetWinDetector();
        }

        // Initialize visuals in view if present
        if (boardManager != null)
        {
            var view = boardManager.GetComponent<GomokuGame.UI.BoardViewManager>();
            if (view != null) 
            {
                //view.boardSize = selectedBoardSize;
                view.InitializeBoard();
            }
        }

        // Ensure consistent state transitions
        currentState = GameState.Playing;
        currentPlayer = Player.Black;
        OnGameStateChanged?.Invoke(currentState);
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    public void EndGame(Player winner)
    {
        // Only allow transition from Playing to GameOver
        if (currentState == GameState.Playing)
        {
            currentState = GameState.GameOver;
            OnGameStateChanged?.Invoke(currentState);
            OnGameWon?.Invoke(winner);
        }
    }

    public void DeclareDraw()
    {
        // Only allow transition from Playing to GameOver
        if (currentState == GameState.Playing)
        {
            currentState = GameState.GameOver;
            OnGameStateChanged?.Invoke(currentState);
            OnGameDraw?.Invoke();
        }
    }

    public void PauseGame()
    {
        // Only allow pause from Playing state
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ResumeGame()
    {
        // Only allow resume from Paused state
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ReturnToMainMenu()
    {
        // Allow return to menu from any state except MainMenu
        if (currentState != GameState.MainMenu)
        {
            currentState = GameState.MainMenu;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    /// <summary>
    /// Sets the board size for the next game
    /// </summary>
    /// <param name="size">Board size (size x size)</param>
    public void SetBoardSize(int size)
    {
        // Validate board size (reasonable limits)
        if (size >= 9 && size <= 19)
        {
            selectedBoardSize = size;
        }
        else
        {
            // Clamp to valid range
            selectedBoardSize = Mathf.Clamp(size, 9, 19);
        }
    }

    /// <summary>
    /// Sets the win condition type for the next game
    /// </summary>
    /// <param name="conditionType">Win condition type</param>
    public void SetWinConditionType(string conditionType)
    {
        if (conditionType == "Standard" || conditionType == "Capture" || conditionType == "TimeBased")
        {
            selectedWinConditionType = conditionType;
        }
        else
        {
            // Default to Standard if invalid
            selectedWinConditionType = "Standard";
        }
    }

    /// <summary>
    /// Gets the currently selected board size
    /// </summary>
    /// <returns>Board size</returns>
    public int GetBoardSize()
    {
        return selectedBoardSize;
    }

    /// <summary>
    /// Gets the currently selected win condition type
    /// </summary>
    /// <returns>Win condition type</returns>
    public string GetWinConditionType()
    {
        return selectedWinConditionType;
    }

    /// <summary>
    /// Gets the BoardViewManager component
    /// </summary>
    /// <returns>BoardViewManager instance</returns>
    public GomokuGame.UI.BoardViewManager GetBoardView()
    {
        if (boardManager != null)
        {
            return boardManager.GetComponent<GomokuGame.UI.BoardViewManager>();
        }
        return null;
    }

    /// <summary>
    /// Ensures default settings are applied when no saved preferences exist
    /// </summary>
    private void EnsureDefaultSettings()
    {
        if (PlayerPrefsManager.IsFirstRun() || !PlayerPrefsManager.ValidateSettings())
        {
            // Apply default settings for first run or invalid settings
            PlayerPrefsManager.ResetToDefaults();
            PlayerPrefs.SetString("WinConditionType", "Standard");
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Handles capture events from the board manager
    /// </summary>
    /// <param name="player">Player who made the capture</param>
    /// <param name="captureCount">Number of pieces captured</param>
    private void OnCaptureMade(GameManager.Player player, int captureCount)
    {
        if (winDetector != null)
        {
            // Record the capture in the win detector
            for (int i = 0; i < captureCount; i++)
            {
                winDetector.RecordCapture(player);
            }

            // Check if this capture results in a win
            if (winDetector.CheckCaptureWin(player))
            {
                EndGame(player);
            }
        }
    }

        internal bool CheckDraw()
        {
            throw new NotImplementedException();
        }

        internal void EndGame()
        {
            throw new NotImplementedException();
        }

        internal void RestartGame()
        {
            throw new NotImplementedException();
        }
    }
}
