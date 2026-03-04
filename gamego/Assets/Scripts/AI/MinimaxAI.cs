using System.Collections.Generic;

namespace Gomoku
{
    /// <summary>
    /// 更高级的 AI - 使用 Minimax 算法 + Alpha-Beta 剪枝
    /// 后续版本使用，当前 SimpleAI 已足够
    /// </summary>
    public class MinimaxAI : IAIPlayer
    {
        private const int MAX_DEPTH = 2;  // 搜索深度，越大越强但越慢
        private const int BOARD_SIZE = Board.BOARD_SIZE;

        private int _nodeCount;  // 用于调试

        public (int x, int y) GetMove(Board board, PieceType myPiece)
        {
            _nodeCount = 0;

            PieceType opponentPiece = myPiece == PieceType.Black ? PieceType.White : PieceType.Black;

            // 获取候选位置（只考虑已有棋子附近）
            List<(int x, int y)> candidates = GetCandidateMoves(board);

            if (candidates.Count == 0)
            {
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);  // 中心点
            }

            int bestScore = int.MinValue;
            (int x, int y) bestMove = candidates[0];

            foreach (var (x, y) in candidates)
            {
                // 模拟落子
                board.PlacePiece(x, y, myPiece);

                // Minimax 搜索
                int score = Minimax(board, MAX_DEPTH - 1, int.MinValue, int.MaxValue, false, myPiece, opponentPiece);

                // 撤销落子
                board.RemovePiece(x, y);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (x, y);
                }
            }

            return bestMove;
        }

        private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizing,
                           PieceType myPiece, PieceType opponentPiece)
        {
            _nodeCount++;

            // 终止条件
            if (depth == 0)
            {
                return EvaluateBoard(board, myPiece, opponentPiece);
            }

            List<(int x, int y)> candidates = GetCandidateMoves(board);

            if (candidates.Count == 0)
            {
                return EvaluateBoard(board, myPiece, opponentPiece);
            }

            if (isMaximizing)
            {
                int maxScore = int.MinValue;

                foreach (var (x, y) in candidates)
                {
                    board.PlacePiece(x, y, myPiece);

                    // 检查是否获胜
                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        return 100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, false, myPiece, opponentPiece);
                    board.RemovePiece(x, y);

                    maxScore = System.Math.Max(maxScore, score);
                    alpha = System.Math.Max(alpha, score);

                    if (beta <= alpha)
                        break;  // Alpha-Beta 剪枝
                }

                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;

                foreach (var (x, y) in candidates)
                {
                    board.PlacePiece(x, y, opponentPiece);

                    // 检查对手是否获胜
                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        return -100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, true, myPiece, opponentPiece);
                    board.RemovePiece(x, y);

                    minScore = System.Math.Min(minScore, score);
                    beta = System.Math.Min(beta, score);

                    if (beta <= alpha)
                        break;  // Alpha-Beta 剪枝
                }

                return minScore;
            }
        }

        /// <summary>
        /// 评估棋盘局势
        /// </summary>
        private int EvaluateBoard(Board board, PieceType myPiece, PieceType opponentPiece)
        {
            int myScore = EvaluatePiece(board, myPiece);
            int opponentScore = EvaluatePiece(board, opponentPiece);

            return myScore - opponentScore;
        }

        /// <summary>
        /// 评估某方的棋型分数
        /// </summary>
        private int EvaluatePiece(Board board, PieceType piece)
        {
            int score = 0;

            // 横向
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                score += EvaluateLine(board, 0, y, 1, 0, piece);
            }

            // 纵向
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                score += EvaluateLine(board, x, 0, 0, 1, piece);
            }

            // 对角线
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                score += EvaluateLine(board, i, 0, 1, 1, piece);
                if (i > 0)
                    score += EvaluateLine(board, 0, i, 1, 1, piece);
            }

            // 反对角线
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                score += EvaluateLine(board, i, 0, -1, 1, piece);
                if (i > 0)
                    score += EvaluateLine(board, BOARD_SIZE - 1, i, -1, 1, piece);
            }

            return score;
        }

        /// <summary>
        /// 评估一条线的分数
        /// </summary>
        private int EvaluateLine(Board board, int startX, int startY, int dx, int dy, PieceType piece)
        {
            int score = 0;
            int count = 0;
            int block = 0;
            int empty = 0;

            int x = startX;
            int y = startY;

            while (board.IsValidPosition(x, y))
            {
                if (board.GetPiece(x, y) == piece)
                {
                    count++;
                }
                else if (board.GetPiece(x, y) == PieceType.None)
                {
                    if (empty == 0)
                    {
                        empty = 1;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    block++;
                    break;
                }

                x += dx;
                y += dy;
            }

            // 简化的评分规则
            if (count >= 5)
                score += 100000;
            else if (count == 4)
                score += block == 0 ? 10000 : 1000;
            else if (count == 3)
                score += block == 0 ? 1000 : 100;
            else if (count == 2)
                score += block == 0 ? 100 : 10;

            return score;
        }

        /// <summary>
        /// 获取候选落子位置（已有棋子附近）
        /// </summary>
        private List<(int x, int y)> GetCandidateMoves(Board board)
        {
            List<(int x, int y)> candidates = new List<(int x, int y)>();
            HashSet<(int, int)> added = new HashSet<(int, int)>();

            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (board.GetPiece(x, y) != PieceType.None)
                    {
                        // 检查周围的空位
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = -2; dy <= 2; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (board.IsValidPosition(nx, ny) &&
                                    board.GetPiece(nx, ny) == PieceType.None &&
                                    !added.Contains((nx, ny)))
                                {
                                    candidates.Add((nx, ny));
                                    added.Add((nx, ny));
                                }
                            }
                        }
                    }
                }
            }

            return candidates;
        }
    }
}
