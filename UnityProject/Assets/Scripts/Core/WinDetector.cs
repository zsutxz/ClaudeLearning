using GomokuGame.UI;
using UnityEngine;
namespace GomokuGame.Core
{
    /// <summary>
    /// Detects win conditions in the Gomoku game
    /// </summary>
    public class WinDetector : MonoBehaviour
    {
        #region Fields
        [SerializeField] private BoardViewManager BoardViewManager;
        [SerializeField] private int winCondition = 5; // Default 5 in a row to win
        [SerializeField] private string winConditionType = "Standard"; // Default win condition type
        private int blackCaptures = 0;
        private int whiteCaptures = 0;
        private float gameTimer = 0f;
        private float timeLimit = 300f; // 5 minutes default
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the last move resulted in a win
        /// </summary>
        /// <param name="x">X coordinate of last move</param>
        /// <param name="y">Y coordinate of last move</param>
        /// <param name="player">Player who made the move</param>
        /// <returns>True if the move resulted in a win, false otherwise</returns>
        public bool CheckWin(int x, int y, GameManager.Player player)
        {
            if (BoardViewManager == null || BoardViewManager.coreBoardManager == null)
                return false;

            // Handle different win condition types
            switch (winConditionType)
            {
                case "Standard":
                    // Standard 5-in-a-row win condition
                    return CheckDirection(x, y, 1, 0, player) ||  // Horizontal
                           CheckDirection(x, y, 0, 1, player) ||  // Vertical
                           CheckDirection(x, y, 1, 1, player) ||  // Diagonal /
                           CheckDirection(x, y, 1, -1, player);   // Diagonal \\

                case "Capture":
                    // Capture win condition - check if player has captured enough pieces
                    return CheckCaptureWin(player);

                case "TimeBased":
                    // Time-based win condition - check if time limit has been reached
                    return CheckTimeBasedWin(player);

                default:
                    // Default to standard win condition
                    return CheckDirection(x, y, 1, 0, player) ||
                           CheckDirection(x, y, 0, 1, player) ||
                           CheckDirection(x, y, 1, 1, player) ||
                           CheckDirection(x, y, 1, -1, player);
            }
        }

        /// <summary>
        /// Checks for win condition in a specific direction
        /// </summary>
        /// <param name="x">Starting X coordinate</param>
        /// <param name="y">Starting Y coordinate</param>
        /// <param name="dx">X direction increment</param>
        /// <param name="dy">Y direction increment</param>
        /// <param name="player">Player to check for</param>
        /// <returns>True if win condition is met in this direction, false otherwise</returns>
        public bool CheckDirection(int x, int y, int dx, int dy, GameManager.Player player)
        {
            if (BoardViewManager == null || BoardViewManager.coreBoardManager == null)
                return false;

            // Validate starting position
            if (!BoardViewManager.coreBoardManager.IsValidPosition(x, y))
                return false;

            int count = 1; // Count the piece that was just placed

            // Check in positive direction
            int tx = x + dx;
            int ty = y + dy;
            while (BoardViewManager.coreBoardManager.IsValidPosition(tx, ty) &&
                   BoardViewManager.coreBoardManager.GetPieceAt(tx, ty) == player)
            {
                count++;
                tx += dx;
                ty += dy;
            }

            // Check in negative direction
            tx = x - dx;
            ty = y - dy;
            while (BoardViewManager.coreBoardManager.IsValidPosition(tx, ty) &&
                   BoardViewManager.coreBoardManager.GetPieceAt(tx, ty) == player)
            {
                count++;
                tx -= dx;
                ty -= dy;
            }

            return count >= winCondition;
        }

        /// <summary>
        /// Checks if the board is full (draw condition)
        /// </summary>
        /// <returns>True if board is full, false otherwise</returns>
        public bool CheckDraw()
        {
            if (BoardViewManager == null || BoardViewManager.coreBoardManager == null)
                return false;

            for (int x = 0; x < BoardViewManager.boardSize; x++)
            {
                for (int y = 0; y < BoardViewManager.boardSize; y++)
                {
                    if (BoardViewManager.coreBoardManager.IsPositionEmpty(x, y))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks capture win condition
        /// </summary>
        /// <param name="player">Player to check for</param>
        /// <returns>True if player has captured enough pieces to win</returns>
        public bool CheckCaptureWin(GameManager.Player player)
        {
            // For capture win condition, player wins if they capture enough opponent pieces
            int requiredCaptures = 5; // Win by capturing 5 pieces
            
            if (player == GameManager.Player.Black)
            {
                return blackCaptures >= requiredCaptures;
            }
            else if (player == GameManager.Player.White)
            {
                return whiteCaptures >= requiredCaptures;
            }
            
            return false;
        }

        /// <summary>
        /// Checks time-based win condition
        /// </summary>
        /// <param name="player">Player to check for</param>
        /// <returns>True if time limit has been reached</returns>
        public bool CheckTimeBasedWin(GameManager.Player player)
        {
            // For time-based win condition, game ends when time limit is reached
            // Player with most pieces wins
            return gameTimer >= timeLimit;
        }

        /// <summary>
        /// Updates the game timer for time-based win conditions
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public void UpdateGameTimer(float deltaTime)
        {
            gameTimer += deltaTime;
        }

        /// <summary>
        /// Records a capture for a player
        /// </summary>
        /// <param name="player">Player who made the capture</param>
        public void RecordCapture(GameManager.Player player)
        {
            if (player == GameManager.Player.Black)
            {
                blackCaptures++;
            }
            else if (player == GameManager.Player.White)
            {
                whiteCaptures++;
            }
        }

        /// <summary>
        /// Gets the current capture count for a player
        /// </summary>
        /// <param name="player">Player to get capture count for</param>
        /// <returns>Number of captures</returns>
        public int GetCaptureCount(GameManager.Player player)
        {
            if (player == GameManager.Player.Black)
            {
                return blackCaptures;
            }
            else if (player == GameManager.Player.White)
            {
                return whiteCaptures;
            }
            
            return 0;
        }

        /// <summary>
        /// Gets the remaining time for time-based games
        /// </summary>
        /// <returns>Remaining time in seconds</returns>
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, timeLimit - gameTimer);
        }

        /// <summary>
        /// Sets the win condition type
        /// </summary>
        /// <param name="conditionType">Win condition type to set</param>
        public void SetWinConditionType(string conditionType)
        {
            winConditionType = conditionType;
        }

        /// <summary>
        /// Resets the win detector for a new game
        /// </summary>
        public void ResetWinDetector()
        {
            blackCaptures = 0;
            whiteCaptures = 0;
            gameTimer = 0f;
        }
        #endregion
    }
}