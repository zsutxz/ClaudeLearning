using UnityEngine;

namespace GomokuGame.Core
{
    /// <summary>
    /// Detects win conditions in the Gomoku game
    /// </summary>
    public class WinDetector : MonoBehaviour
    {
        #region Fields
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private int winCondition = 5; // Default 5 in a row to win
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
            if (boardManager == null)
                return false;
                
            // Check all four directions: horizontal, vertical, and two diagonals
            return CheckDirection(x, y, 1, 0, player) ||  // Horizontal
                   CheckDirection(x, y, 0, 1, player) ||  // Vertical
                   CheckDirection(x, y, 1, 1, player) ||  // Diagonal /
                   CheckDirection(x, y, 1, -1, player);   // Diagonal \
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
            if (boardManager == null)
                return false;
                
            int count = 1; // Count the piece that was just placed
            
            // Check in positive direction
            int tx = x + dx;
            int ty = y + dy;
            while (tx >= 0 && tx < boardManager.BoardSize && 
                   ty >= 0 && ty < boardManager.BoardSize && 
                   boardManager.GetPieceAt(tx, ty) == player)
            {
                count++;
                tx += dx;
                ty += dy;
            }
            
            // Check in negative direction
            tx = x - dx;
            ty = y - dy;
            while (tx >= 0 && tx < boardManager.BoardSize && 
                   ty >= 0 && ty < boardManager.BoardSize && 
                   boardManager.GetPieceAt(tx, ty) == player)
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
            if (boardManager == null)
                return false;
                
            for (int x = 0; x < boardManager.BoardSize; x++)
            {
                for (int y = 0; y < boardManager.BoardSize; y++)
                {
                    if (boardManager.IsPositionEmpty(x, y))
                        return false;
                }
            }
            
            return true;
        }
        #endregion
    }
}