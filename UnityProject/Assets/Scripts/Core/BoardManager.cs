
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
        
        // Performance optimization: Cache for frequently accessed values
        private bool isLargeBoard = false;
        #endregion

        #region Properties
        public int[,] BoardState => boardState;
        public int BoardSize => boardSize;
        #endregion

        #region Events
        public event System.Action<int, int, GameManager.Player> OnPiecePlaced;
        public event System.Action<GameManager.Player, int> OnCaptureMade;
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
            
            // Performance optimization: Cache board size classification
            isLargeBoard = boardSize > 15;
            
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
            // Validate coordinates with comprehensive boundary checking
            if (!IsValidPosition(x, y))
                return false;
                
            // Check if position is empty
            if (boardState[x, y] != 0)
                return false;
                
            // Validate player is valid (not None)
            if (player == GameManager.Player.None)
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
            if (!IsValidPosition(x, y))
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
            if (!IsValidPosition(x, y))
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

        /// <summary>
        /// Checks for captures after a piece is placed
        /// </summary>
        /// <param name="x">X coordinate of placed piece</param>
        /// <param name="y">Y coordinate of placed piece</param>
        /// <param name="player">Player who placed the piece</param>
        /// <returns>Number of captures made</returns>
        public int CheckForCaptures(int x, int y, GameManager.Player player)
        {
            int captures = 0;
            GameManager.Player opponent = player == GameManager.Player.Black ? GameManager.Player.White : GameManager.Player.Black;

            // Check all four directions for capture patterns
            captures += CheckDirectionForCapture(x, y, 1, 0, player, opponent);  // Horizontal
            captures += CheckDirectionForCapture(x, y, 0, 1, player, opponent);  // Vertical
            captures += CheckDirectionForCapture(x, y, 1, 1, player, opponent);  // Diagonal /
            captures += CheckDirectionForCapture(x, y, 1, -1, player, opponent); // Diagonal \

            if (captures > 0)
            {
                OnCaptureMade?.Invoke(player, captures);
            }

            return captures;
        }

        /// <summary>
        /// Checks a specific direction for capture patterns
        /// </summary>
        /// <param name="x">Starting X coordinate</param>
        /// <param name="y">Starting Y coordinate</param>
        /// <param name="dx">X direction increment</param>
        /// <param name="dy">Y direction increment</param>
        /// <param name="player">Current player</param>
        /// <param name="opponent">Opponent player</param>
        /// <returns>Number of captures in this direction</returns>
        private int CheckDirectionForCapture(int x, int y, int dx, int dy, GameManager.Player player, GameManager.Player opponent)
        {
            int captures = 0;

            // Performance optimization: Early exit for edge cases
            if (!IsValidPosition(x, y))
                return 0;

            // Check for capture pattern: opponent's piece, opponent's piece, player's piece
            // This represents surrounding two opponent pieces

            // Check in positive direction
            int tx1 = x + dx;
            int ty1 = y + dy;
            int tx2 = x + 2 * dx;
            int ty2 = y + 2 * dy;
            int tx3 = x + 3 * dx;
            int ty3 = y + 3 * dy;

            if (IsValidPosition(tx1, ty1) && IsValidPosition(tx2, ty2) && IsValidPosition(tx3, ty3))
            {
                // Pattern: opponent, opponent, player
                if (GetPieceAt(tx1, ty1) == opponent &&
                    GetPieceAt(tx2, ty2) == opponent &&
                    GetPieceAt(tx3, ty3) == player)
                {
                    // Capture the two opponent pieces
                    boardState[tx1, ty1] = 0;
                    boardState[tx2, ty2] = 0;
                    captures += 2;
                }
            }

            // Check in negative direction
            tx1 = x - dx;
            ty1 = y - dy;
            tx2 = x - 2 * dx;
            ty2 = y - 2 * dy;
            tx3 = x - 3 * dx;
            ty3 = y - 3 * dy;

            if (IsValidPosition(tx1, ty1) && IsValidPosition(tx2, ty2) && IsValidPosition(tx3, ty3))
            {
                // Pattern: opponent, opponent, player
                if (GetPieceAt(tx1, ty1) == opponent &&
                    GetPieceAt(tx2, ty2) == opponent &&
                    GetPieceAt(tx3, ty3) == player)
                {
                    // Capture the two opponent pieces
                    boardState[tx1, ty1] = 0;
                    boardState[tx2, ty2] = 0;
                    captures += 2;
                }
            }

            return captures;
        }

        /// <summary>
        /// Checks if a position is valid on the board
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if position is valid, false otherwise</returns>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
        }
        #endregion
    }
}


