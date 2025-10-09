using UnityEngine;
using UnityEngine.UI;
using GomokuGame.Core;
using System.Collections;

namespace GomokuGame.UI
{
    /// <summary>
    /// Test script to verify UI functionality for different board sizes
    /// </summary>
    public class BoardSizeUITest : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardViewManager boardViewManager;

        void Start()
        {
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();

            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            if (boardViewManager == null)
                boardViewManager = FindObjectOfType<BoardViewManager>();

            // Run UI tests
            StartCoroutine(RunUITests());
        }

        private IEnumerator RunUITests()
        {
            Debug.Log("Starting Board Size UI Tests");

            // Test UI elements for different board sizes
            int[] testSizes = { 9, 13, 15, 19 };

            foreach (int size in testSizes)
            {
                Debug.Log($"Testing UI for board size: {size}x{size}");

                // Test setting board size through UI
                yield return StartCoroutine(TestBoardSizeUISettings(size));

                // Test visual display of board
                yield return StartCoroutine(TestBoardVisualization(size));

                // Test game start with selected size
                yield return StartCoroutine(TestGameStartWithSize(size));
            }

            Debug.Log("Board Size UI Tests Completed");
        }

        private IEnumerator TestBoardSizeUISettings(int size)
        {
            Debug.Log($"Testing UI settings for {size}x{size} board");

            // Test slider value setting
            if (uiManager != null)
            {
                // Simulate setting board size through PlayerPrefs like the UI does
                PlayerPrefs.SetInt("BoardSize", size);
                PlayerPrefs.Save();

                // Load settings into UI
                // This would normally be called when the settings panel is opened
                // We'll simulate it by directly calling the method
                var settingsMethod = uiManager.GetType().GetMethod("LoadSettingsValues",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (settingsMethod != null)
                {
                    settingsMethod.Invoke(uiManager, null);
                }

                // Verify the setting was saved
                int savedSize = PlayerPrefs.GetInt("BoardSize", 15);
                Debug.Log($"Board size {size} saved to PlayerPrefs: {savedSize == size}");

                // Test that GameManager receives the setting
                if (gameManager != null)
                {
                    gameManager.SetBoardSize(size);
                    int gameManagerSize = gameManager.GetBoardSize();
                    Debug.Log($"GameManager received board size {size}: {gameManagerSize == size}");
                }
            }

            yield return null;
        }

        private IEnumerator TestBoardVisualization(int size)
        {
            Debug.Log($"Testing board visualization for {size}x{size} board");

            if (gameManager != null && boardViewManager != null)
            {
                // Set the board size
                gameManager.SetBoardSize(size);

                // Start a new game to initialize the board
                gameManager.StartNewGame();

                // Wait a frame for initialization
                yield return null;

                // Check that BoardViewManager has the correct size
                int viewSize = boardViewManager.boardSize;
                Debug.Log($"BoardViewManager size matches {size}: {viewSize == size}");

                // Check that the board container exists
                bool hasContainer = boardViewManager.boardContainer != null;
                Debug.Log($"Board container exists: {hasContainer}");

                // Check that grid lines were created
                // We can't directly access private methods, but we can check if the boardContainer has children
                if (boardViewManager.boardContainer != null)
                {
                    int childCount = boardViewManager.boardContainer.transform.childCount;
                    bool hasChildren = childCount > 0;
                    Debug.Log($"Board container has children (grid lines): {hasChildren} (Child count: {childCount})");
                }
            }

            yield return null;
        }

        private IEnumerator TestGameStartWithSize(int size)
        {
            Debug.Log($"Testing game start with {size}x{size} board");

            if (uiManager != null && gameManager != null)
            {
                // Set the board size
                PlayerPrefs.SetInt("BoardSize", size);
                PlayerPrefs.Save();
                gameManager.SetBoardSize(size);

                // Simulate starting a game through UI
                uiManager.StartGame();

                // Wait a frame for initialization
                yield return null;

                // Check that game state is Playing
                bool isPlaying = gameManager.currentState == GameManager.GameState.Playing;
                Debug.Log($"Game started with {size}x{size} board: {isPlaying}");

                // Check that board size is correct
                int actualSize = gameManager.GetBoardSize();
                Debug.Log($"Board size after game start: {actualSize} (Expected: {size})");

                // Check that BoardManager has the correct size
                if (gameManager.boardManager != null)
                {
                    int boardManagerSize = gameManager.boardManager.BoardSize;
                    Debug.Log($"BoardManager size after game start: {boardManagerSize} (Expected: {size})");
                }
            }

            yield return null;
        }
    }
}