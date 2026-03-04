namespace Gomoku
{
    /// <summary>
    /// 胜负判定类 - 检查五子连线
    /// </summary>
    public static class WinChecker
    {
        // 四个方向：水平、垂直、左上-右下对角线、右上-左下对角线
        private static readonly int[][] Directions = new int[][]
        {
            new int[] { 1, 0 },   // 水平 →
            new int[] { 0, 1 },   // 垂直 ↓
            new int[] { 1, 1 },   // 对角线 ↘
            new int[] { 1, -1 }   // 反对角线 ↗
        };

        /// <summary>
        /// 检查是否获胜
        /// </summary>
        /// <param name="board">棋盘</param>
        /// <param name="x">最后落子的 x 坐标</param>
        /// <param name="y">最后落子的 y 坐标</param>
        /// <returns>如果当前玩家获胜返回 true</returns>
        public static bool CheckWin(Board board, int x, int y)
        {
            PieceType piece = board.GetPiece(x, y);
            if (piece == PieceType.None)
                return false;

            foreach (var dir in Directions)
            {
                int count = 1 + CountInDirection(board, x, y, dir[0], dir[1], piece)
                          + CountInDirection(board, x, y, -dir[0], -dir[1], piece);

                if (count >= 5)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查游戏状态
        /// </summary>
        public static GameState CheckGameState(Board board, int lastX, int lastY)
        {
            // 检查最后落子位置是否形成五连
            PieceType lastPiece = board.GetPiece(lastX, lastY);

            if (CheckWin(board, lastX, lastY))
            {
                return lastPiece == PieceType.Black ? GameState.BlackWin : GameState.WhiteWin;
            }

            // 检查是否平局
            if (board.IsFull())
            {
                return GameState.Draw;
            }

            return GameState.Playing;
        }

        /// <summary>
        /// 计算某个方向上连续相同棋子的数量
        /// </summary>
        private static int CountInDirection(Board board, int startX, int startY, int dx, int dy, PieceType targetPiece)
        {
            int count = 0;
            int x = startX + dx;
            int y = startY + dy;

            while (board.IsValidPosition(x, y) && board.GetPiece(x, y) == targetPiece)
            {
                count++;
                x += dx;
                y += dy;
            }

            return count;
        }

        /// <summary>
        /// 获取获胜的连线位置（用于高亮显示）
        /// </summary>
        public static bool TryGetWinningLine(Board board, int lastX, int lastY, out System.Collections.Generic.List<(int x, int y)> winningPositions)
        {
            winningPositions = null;
            PieceType piece = board.GetPiece(lastX, lastY);

            if (piece == PieceType.None)
                return false;

            foreach (var dir in Directions)
            {
                var positions = new System.Collections.Generic.List<(int x, int y)> { (lastX, lastY) };

                // 正方向
                int x = lastX + dir[0];
                int y = lastY + dir[1];
                while (board.IsValidPosition(x, y) && board.GetPiece(x, y) == piece)
                {
                    positions.Add((x, y));
                    x += dir[0];
                    y += dir[1];
                }

                // 反方向
                x = lastX - dir[0];
                y = lastY - dir[1];
                while (board.IsValidPosition(x, y) && board.GetPiece(x, y) == piece)
                {
                    positions.Add((x, y));
                    x -= dir[0];
                    y -= dir[1];
                }

                if (positions.Count >= 5)
                {
                    winningPositions = positions;
                    return true;
                }
            }

            return false;
        }
    }
}
