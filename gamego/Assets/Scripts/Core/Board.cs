using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 棋盘数据管理类 - 纯逻辑，不依赖 MonoBehaviour
    /// </summary>
    public class Board
    {
        public const int BOARD_SIZE = 15;  // 15x15 棋盘

        private PieceType[,] _grid;
        private int _pieceCount;

        public int PieceCount => _pieceCount;
        public PieceType[,] Grid => _grid;

        public Board()
        {
            _grid = new PieceType[BOARD_SIZE, BOARD_SIZE];
            Reset();
        }

        /// <summary>
        /// 重置棋盘
        /// </summary>
        public void Reset()
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    _grid[x, y] = PieceType.None;
                }
            }
            _pieceCount = 0;
        }

        /// <summary>
        /// 获取指定位置的棋子类型
        /// </summary>
        public PieceType GetPiece(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return PieceType.None;
            return _grid[x, y];
        }

        /// <summary>
        /// 放置棋子
        /// </summary>
        public bool PlacePiece(int x, int y, PieceType piece)
        {
            if (!IsValidPosition(x, y) || _grid[x, y] != PieceType.None)
                return false;

            _grid[x, y] = piece;
            _pieceCount++;
            return true;
        }

        /// <summary>
        /// 移除棋子（用于悔棋）
        /// </summary>
        public bool RemovePiece(int x, int y)
        {
            if (!IsValidPosition(x, y) || _grid[x, y] == PieceType.None)
                return false;

            _grid[x, y] = PieceType.None;
            _pieceCount--;
            return true;
        }

        /// <summary>
        /// 检查位置是否有效
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }

        /// <summary>
        /// 检查是否为空位
        /// </summary>
        public bool IsEmpty(int x, int y)
        {
            return IsValidPosition(x, y) && _grid[x, y] == PieceType.None;
        }

        /// <summary>
        /// 检查棋盘是否已满
        /// </summary>
        public bool IsFull()
        {
            return _pieceCount >= BOARD_SIZE * BOARD_SIZE;
        }
    }
}
