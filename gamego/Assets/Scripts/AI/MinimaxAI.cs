using System.Collections.Generic;
using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 更高级的 AI - 使用 Minimax 算法 + Alpha-Beta 剪枝
    /// 支持可配置的搜索深度，使用 BoardEvaluator 统一评估
    /// </summary>
    public class MinimaxAI : IAIPlayer
    {
        private readonly int _maxDepth;
        private const int BOARD_SIZE = Board.BOARD_SIZE;

        private int _nodeCount;
        private readonly BoardEvaluator _evaluator;

        /// <summary>
        /// 创建 MinimaxAI 实例
        /// </summary>
        /// <param name="maxDepth">最大搜索深度 (1-4)</param>
        public MinimaxAI(int maxDepth = 2)
        {
            _maxDepth = Mathf.Clamp(maxDepth, 1, 4);
            _evaluator = new BoardEvaluator();
        }

        public (int x, int y) GetMove(Board board, PieceType myPiece)
        {
            _nodeCount = 0;

            // 获取排序后的候选位置（只取前 15 个最有价值的）
            var sorted = _evaluator.GetSortedCandidates(board, myPiece, topN: 15);

            if (sorted.Count == 0)
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);

            // 检查第一候选是否直接赢
            if (sorted[0].score >= 100000)
                return (sorted[0].x, sorted[0].y);

            int bestScore = int.MinValue;
            (int x, int y) bestMove = (sorted[0].x, sorted[0].y);

            foreach (var (x, y, _) in sorted)
            {
                board.PlacePiece(x, y, myPiece);

                // 检查直接获胜
                if (WinChecker.CheckWin(board, x, y))
                {
                    board.RemovePiece(x, y);
                    return (x, y);
                }

                int score = Minimax(board, _maxDepth - 1, int.MinValue, int.MaxValue, false, myPiece);
                board.RemovePiece(x, y);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (x, y);
                }
            }

            return bestMove;
        }

        private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizing, PieceType myPiece)
        {
            _nodeCount++;

            if (depth == 0)
                return _evaluator.Evaluate(board, myPiece);

            var opponentPiece = myPiece == PieceType.Black ? PieceType.White : PieceType.Black;
            PieceType currentPiece = isMaximizing ? myPiece : opponentPiece;

            // 获取排序候选（限制数量以控制性能）
            var sorted = _evaluator.GetSortedCandidates(board, currentPiece, topN: 10);

            if (sorted.Count == 0)
                return _evaluator.Evaluate(board, myPiece);

            if (isMaximizing)
            {
                int maxScore = int.MinValue;

                foreach (var (x, y, _) in sorted)
                {
                    board.PlacePiece(x, y, myPiece);

                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        return 100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, false, myPiece);
                    board.RemovePiece(x, y);

                    maxScore = System.Math.Max(maxScore, score);
                    alpha = System.Math.Max(alpha, score);

                    if (beta <= alpha)
                        break;
                }

                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;

                foreach (var (x, y, _) in sorted)
                {
                    board.PlacePiece(x, y, opponentPiece);

                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        return -100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, true, myPiece);
                    board.RemovePiece(x, y);

                    minScore = System.Math.Min(minScore, score);
                    beta = System.Math.Min(beta, score);

                    if (beta <= alpha)
                        break;
                }

                return minScore;
            }
        }
    }
}
