using System.Collections.Generic;
using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 简单 AI - 基于规则和随机选择
    /// MVP 版本：能够识别基本威胁和机会
    /// </summary>
    public class SimpleAI : IAIPlayer
    {
        private const int WINNING_SCORE = 100000;
        private const int FOUR_SCORE = 10000;
        private const int THREE_SCORE = 1000;
        private const int TWO_SCORE = 100;

        public (int x, int y) GetMove(Board board, PieceType myPiece)
        {
            PieceType opponentPiece = myPiece == PieceType.Black ? PieceType.White : PieceType.Black;

            // 1. 检查是否能赢（五连）
            var winningMove = FindPatternMove(board, myPiece, 4);
            if (winningMove.HasValue)
                return winningMove.Value;

            // 2. 阻止对手赢棋
            var blockingMove = FindPatternMove(board, opponentPiece, 4);
            if (blockingMove.HasValue)
                return blockingMove.Value;

            // 3. 创建活四
            var createFourMove = FindPatternMove(board, myPiece, 3);
            if (createFourMove.HasValue)
                return createFourMove.Value;

            // 4. 阻止对手活四
            var blockFourMove = FindPatternMove(board, opponentPiece, 3);
            if (blockFourMove.HasValue)
                return blockFourMove.Value;

            // 5. 创建活三
            var createThreeMove = FindPatternMove(board, myPiece, 2);
            if (createThreeMove.HasValue)
                return createThreeMove.Value;

            // 6. 阻止对手活三
            var blockThreeMove = FindPatternMove(board, opponentPiece, 2);
            if (blockThreeMove.HasValue)
                return blockThreeMove.Value;

            // 7. 找一个靠近已有棋子的空位
            var nearbyMove = FindNearbyMove(board);
            if (nearbyMove.HasValue)
                return nearbyMove.Value;

            // 8. 随机选择（通常是第一步）
            return GetRandomEmptyPosition(board);
        }

        /// <summary>
        /// 查找能形成指定连子数的落子位置
        /// </summary>
        private (int x, int y)? FindPatternMove(Board board, PieceType piece, int targetCount)
        {
            List<(int x, int y, int score)> candidates = new List<(int x, int y, int score)>();

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (!board.IsEmpty(x, y)) continue;

                    int score = EvaluatePosition(board, x, y, piece, targetCount);
                    if (score > 0)
                    {
                        candidates.Add((x, y, score));
                    }
                }
            }

            if (candidates.Count == 0)
                return null;

            // 返回得分最高的位置
            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            return (candidates[0].x, candidates[0].y);
        }

        /// <summary>
        /// 评估某个位置的得分
        /// </summary>
        private int EvaluatePosition(Board board, int x, int y, PieceType piece, int targetCount)
        {
            int maxCount = 0;
            int[][] directions = new int[][]
            {
                new int[] { 1, 0 },
                new int[] { 0, 1 },
                new int[] { 1, 1 },
                new int[] { 1, -1 }
            };

            foreach (var dir in directions)
            {
                int count = 1;
                count += CountConsecutive(board, x + dir[0], y + dir[1], dir[0], dir[1], piece);
                count += CountConsecutive(board, x - dir[0], y - dir[1], -dir[0], -dir[1], piece);

                if (count > maxCount)
                    maxCount = count;
            }

            // 只有当能形成目标数量或更多时才返回分数
            if (maxCount >= targetCount)
            {
                return maxCount * 100;
            }

            return 0;
        }

        /// <summary>
        /// 计算某个方向上连续相同棋子的数量
        /// </summary>
        private int CountConsecutive(Board board, int x, int y, int dx, int dy, PieceType piece)
        {
            int count = 0;
            while (board.IsValidPosition(x, y) && board.GetPiece(x, y) == piece)
            {
                count++;
                x += dx;
                y += dy;
            }
            return count;
        }

        /// <summary>
        /// 查找靠近已有棋子的空位
        /// </summary>
        private (int x, int y)? FindNearbyMove(Board board)
        {
            List<(int x, int y)> candidates = new List<(int x, int y)>();

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (!board.IsEmpty(x, y)) continue;

                    // 检查周围2格内是否有棋子
                    if (HasNeighbor(board, x, y, 2))
                    {
                        candidates.Add((x, y));
                    }
                }
            }

            if (candidates.Count == 0)
                return null;

            // 随机选择一个候选位置
            int index = Random.Range(0, candidates.Count);
            return candidates[index];
        }

        /// <summary>
        /// 检查某个位置周围是否有棋子
        /// </summary>
        private bool HasNeighbor(Board board, int x, int y, int range)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (board.IsValidPosition(nx, ny) && !board.IsEmpty(nx, ny))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取随机空位（通常是第一步）
        /// </summary>
        private (int x, int y) GetRandomEmptyPosition(Board board)
        {
            // 优先选择中心位置
            int center = Board.BOARD_SIZE / 2;

            if (board.IsEmpty(center, center))
                return (center, center);

            // 随机选择一个空位
            List<(int x, int y)> emptyPositions = new List<(int x, int y)>();

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (board.IsEmpty(x, y))
                    {
                        emptyPositions.Add((x, y));
                    }
                }
            }

            if (emptyPositions.Count == 0)
                return (0, 0);

            int index = Random.Range(0, emptyPositions.Count);
            return emptyPositions[index];
        }
    }
}
