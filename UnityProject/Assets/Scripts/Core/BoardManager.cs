
using GomokuGame.UI;
using UnityEngine;
namespace GomokuGame.Core
{
    /// <summary>
    /// Manages the game board state and piece placement
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        #region Fields
        [SerializeField] private int boardSize = 15;
        [SerializeField] private GameObject boardPiecePrefab;
        [SerializeField] private Transform boardParent;
        
        private int[,] boardState;
        private GameObject[,] boardPieces;
        #endregion

        #region Properties
        public int[,] BoardState => boardState;
        public int BoardSize => boardSize;
        #endregion

        #region Events
        public event System.Action<int, int, GameManager.Player> OnPiecePlaced;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeBoard();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the board with the specified size
        /// </summary>
        /// <param name="size">Board size (size x size)</param>
        public void InitializeBoard(int size = 15)
        {
            boardSize = size;
            boardState = new int[boardSize, boardSize];
            boardPieces = new GameObject[boardSize, boardSize];
            
            // Clear any existing board pieces
            if (boardParent != null)
            {
                foreach (Transform child in boardParent)
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Initialize board state to empty
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    boardState[x, y] = 0; // 0 = empty, 1 = black, 2 = white
                }
            }
            
            // Notify any listeners that the board has been initialized
            // This could be used to update the visual representation
        }

        /// <summary>
        /// Places a piece on the board
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="player">Player placing the piece</param>
        /// <returns>True if piece was placed successfully, false otherwise</returns>
        public bool PlacePiece(int x, int y, GameManager.Player player)
        {
            // Validate coordinates
            if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
                return false;
                
            // Check if position is empty
            if (boardState[x, y] != 0)
                return false;
                
            // Place the piece
            boardState[x, y] = (int)player;
            
            // Notify listeners
            OnPiecePlaced?.Invoke(x, y, player);
            
            return true;
        }

        /// <summary>
        /// Gets the piece at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The player at the position, or None if empty</returns>
        public GameManager.Player GetPieceAt(int x, int y)
        {
            if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
                return GameManager.Player.None;
                
            return (GameManager.Player)boardState[x, y];
        }

        /// <summary>
        /// Checks if a position is empty
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if position is empty, false otherwise</returns>
        public bool IsPositionEmpty(int x, int y)
        {
            if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
                return false;
                
            return boardState[x, y] == 0;
        }

        /// <summary>
        /// Clears the board
        /// </summary>
        public void ClearBoard()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    boardState[x, y] = 0;
                }
            }
        }
        #endregion
    }
}


