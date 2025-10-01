using UnityEngine;
using UnityEngine.SceneManagement;

namespace GomokuGame.Core
{
    /// <summary>
    /// Main game manager that controls the overall game flow and state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion

        #region Enums
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        public enum Player
        {
            None = 0,
            Black = 1,
            White = 2
        }
        #endregion

        #region Fields
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private Player currentPlayer = Player.Black;
        [SerializeField] private int boardSize = 15; // Default 15x15 board
        [SerializeField] private int winCondition = 5; // Default 5 in a row to win
        #endregion

        #region Properties
        public GameState CurrentState => currentState;
        public Player CurrentPlayer => currentPlayer;
        public int BoardSize => boardSize;
        public int WinCondition => winCondition;
        #endregion

        #region Events
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<Player> OnPlayerChanged;
        public event System.Action OnGameStarted;
        public event System.Action<Player> OnGameWon;
        public event System.Action OnGameDraw;
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame()
        {
            ChangeState(GameState.Playing);
            currentPlayer = Player.Black;
            OnPlayerChanged?.Invoke(currentPlayer);
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Switches to the next player
        /// </summary>
        public void SwitchPlayer()
        {
            currentPlayer = currentPlayer == Player.Black ? Player.White : Player.Black;
            OnPlayerChanged?.Invoke(currentPlayer);
        }

        /// <summary>
        /// Ends the current game
        /// </summary>
        public void EndGame()
        {
            ChangeState(GameState.GameOver);
        }

        /// <summary>
        /// Restarts the current game
        /// </summary>
        public void RestartGame()
        {
            // Reload the current scene to restart the game
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        /// <summary>
        /// Returns to the main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Declares a winner
        /// </summary>
        /// <param name="winner">The winning player</param>
        public void DeclareWinner(Player winner)
        {
            EndGame();
            OnGameWon?.Invoke(winner);
        }

        /// <summary>
        /// Declares a draw
        /// </summary>
        public void DeclareDraw()
        {
            EndGame();
            OnGameDraw?.Invoke();
        }

        /// <summary>
        /// Updates game settings
        /// </summary>
        /// <param name="newBoardSize">New board size</param>
        /// <param name="newWinCondition">New win condition</param>
        public void UpdateSettings(int newBoardSize, int newWinCondition)
        {
            boardSize = newBoardSize;
            winCondition = newWinCondition;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the game manager
        /// </summary>
        private void Initialize()
        {
            // Load saved settings
            LoadSettings();
        }

        /// <summary>
        /// Changes the current game state
        /// </summary>
        /// <param name="newState">The new game state</param>
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Loads saved settings
        /// </summary>
        private void LoadSettings()
        {
            boardSize = PlayerPrefs.GetInt("BoardSize", 15);
            winCondition = PlayerPrefs.GetInt("WinCondition", 5);
        }
        #endregion
    }
}