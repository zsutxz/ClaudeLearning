using UnityEngine;

namespace GomokuGame.Core
{
    /// <summary>
    /// Handles user input for the Gomoku game
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameManager gameManager;
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            // Handle mouse input
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                HandleMouseClick();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles mouse click input
        /// </summary>
        private void HandleMouseClick()
        {
            // Convert mouse position to world position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if we hit the board
            if (Physics.Raycast(ray, out hit))
            {
                // Convert world position to board coordinates
                Vector3 hitPosition = hit.point;
                int x, y;

                if (WorldToBoardPosition(hitPosition, out x, out y))
                {
                    // Try to place a piece
                    if (boardManager != null && gameManager != null)
                    {
                        if (boardManager.PlacePiece(x, y, gameManager.currentPlayer))
                        {
                            // Check for win
                            if (gameManager.CheckWin(x, y))
                            {
                                Debug.Log($"Player {gameManager.currentPlayer} wins!");
                                gameManager.EndGame(gameManager.currentPlayer);
                            }
                            else
                            {
                                // Check for draw
                                if (gameManager.winDetector != null && gameManager.winDetector.CheckDraw())
                                {
                                    Debug.Log("Game is a draw!");
                                    gameManager.DeclareDraw();
                                }
                                else
                                {
                                    // Switch player after successful placement
                                    gameManager.SwitchPlayer();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts world position to board coordinates
        /// </summary>
        /// <param name="worldPosition">World position to convert</param>
        /// <param name="x">Output board x coordinate</param>
        /// <param name="y">Output board y coordinate</param>
        /// <returns>True if conversion was successful, false otherwise</returns>
        private bool WorldToBoardPosition(Vector3 worldPosition, out int x, out int y)
        {
            x = 0;
            y = 0;
            
            if (boardManager == null)
                return false;
                
            // Get board size and cell size
            int boardSize = boardManager.BoardSize;
            float cellSize = 1.0f; // Default cell size
            
            // Calculate board half size
            float boardHalfSize = (boardSize - 1) * cellSize * 0.5f;
            
            // Convert world position to board coordinates
            // The board is centered at (0, 0, 0) with y = 0
            x = Mathf.RoundToInt((worldPosition.x + boardHalfSize) / cellSize);
            y = Mathf.RoundToInt((worldPosition.z + boardHalfSize) / cellSize);
            
            // Check if coordinates are within board bounds
            return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
        }
        #endregion
    }
}