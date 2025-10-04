using UnityEngine;
using GomokuGame.UI;

namespace GomokuGame.Core
{
    /// <summary>
    /// Test script to verify board size configuration works correctly
    /// </summary>
    public class BoardSizeTest : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardViewManager boardViewManager;

        void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            if (boardViewManager == null)
                boardViewManager = FindObjectOfType<BoardViewManager>();

            // Run tests
            TestBoardSizeConfiguration();
        }

        private void TestBoardSizeConfiguration()
        {
            Debug.Log("Starting Board Size Configuration Tests");

            // Test default board size
            int defaultSize = gameManager.GetBoardSize();
            Debug.Log($"Default board size: {defaultSize}");

            // Test setting different board sizes
            int[] testSizes = { 9, 13, 15, 19 };

            foreach (int size in testSizes)
            {
                gameManager.SetBoardSize(size);
                int retrievedSize = gameManager.GetBoardSize();
                Debug.Log($"Set board size to {size}, retrieved {retrievedSize}. Match: {size == retrievedSize}");

                // Test that BoardManager has the correct size
                if (gameManager.boardManager != null)
                {
                    int boardManagerSize = gameManager.boardManager.BoardSize;
                    Debug.Log($"BoardManager size: {boardManagerSize}. Match: {size == boardManagerSize}");
                }

                // Test that BoardViewManager has the correct size
                if (boardViewManager != null)
                {
                    int boardViewSize = boardViewManager.boardSize;
                    Debug.Log($"BoardViewManager size: {boardViewSize}. Match: {size == boardViewSize}");
                }
            }

            Debug.Log("Board Size Configuration Tests Completed");
        }
    }
}