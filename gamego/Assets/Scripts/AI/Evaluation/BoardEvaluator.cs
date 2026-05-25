using System.Collections.Generic;

namespace Gomoku
{
    /// <summary>
    /// 棋盘评估器 - 统一的棋型评估逻辑
    /// 供 MinimaxAI 和 MctsAI 共用
    /// </summary>
    public class BoardEvaluator
    {
        private readonly PatternScores _scores;

        // 四个方向：水平、垂直、主对角、副对角
        private static readonly int[][] Directions =
        {
            new int[] { 1, 0 },   // 水平
            new int[] { 0, 1 },   // 垂直
            new int[] { 1, 1 },   // 主对角
            new int[] { 1, -1 }   // 副对角
        };

        public BoardEvaluator(PatternScores scores = null)
        {
            _scores = scores ?? PatternScores.GetDefault();
        }

        /// <summary>
        /// 评估整个棋盘的分数
        /// </summary>
        public int Evaluate(Board board, PieceType player)
        {
            var opponent = player == PieceType.Black ? PieceType.White : PieceType.Black;
            int playerScore = EvaluateForPiece(board, player);
            int opponentScore = EvaluateForPiece(board, opponent);
            return playerScore - opponentScore;
        }

        /// <summary>
        /// 评估某方的棋型总分
        /// </summary>
        private int EvaluateForPiece(Board board, PieceType piece)
        {
            int score = 0;
            var counted = new HashSet<(int, int, int)>();  // (x, y, directionIndex)

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (board.GetPiece(x, y) != piece) continue;

                    for (int d = 0; d < Directions.Length; d++)
                    {
                        if (counted.Contains((x, y, d))) continue;

                        var pattern = DetectPattern(board, x, y, Directions[d][0], Directions[d][1], piece, out int patternLength);

                        // 标记这条线上的所有位置已计数
                        if (pattern != PatternType.None)
                        {
                            for (int i = 0; i < patternLength; i++)
                            {
                                int px = x + Directions[d][0] * i;
                                int py = y + Directions[d][1] * i;
                                counted.Add((px, py, d));
                            }
                        }

                        score += _scores.GetScore(pattern);
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// 检测特定位置的棋型
        /// </summary>
        public PatternType DetectPattern(Board board, int x, int y, int dx, int dy, PieceType piece, out int patternLength)
        {
            patternLength = 1;

            if (board.GetPiece(x, y) != piece)
                return PatternType.None;

            // 双向扫描
            int count = 1;
            int block = 0;
            int space = 0;

            // 正向扫描
            var (c1, s1, b1) = ScanDirection(board, x, y, dx, dy, piece);
            count += c1;
            space += s1;
            if (b1 > 0) block++;

            // 反向扫描
            var (c2, s2, b2) = ScanDirection(board, x, y, -dx, -dy, piece);
            count += c2;
            space += s2;
            if (b2 > 0) block++;

            patternLength = count;

            // 分类棋型
            return ClassifyPattern(count, block, space);
        }

        /// <summary>
        /// 扫描一个方向
        /// </summary>
        private (int count, int space, int blocked) ScanDirection(
            Board board, int startX, int startY, int dx, int dy, PieceType piece)
        {
            int count = 0;
            int space = 0;
            int blocked = 0;

            int x = startX + dx;
            int y = startY + dy;

            // 跳过紧邻的空位（跳活三等情况）
            if (board.IsValidPosition(x, y) && board.GetPiece(x, y) == PieceType.None)
            {
                space++;
                x += dx;
                y += dy;
            }

            // 计算连续棋子
            while (board.IsValidPosition(x, y) && board.GetPiece(x, y) == piece)
            {
                count++;
                x += dx;
                y += dy;
            }

            // 检查是否被堵
            if (!board.IsValidPosition(x, y) || board.GetPiece(x, y) != PieceType.None)
            {
                blocked = 1;
            }

            return (count, space, blocked);
        }

        /// <summary>
        /// 根据连续数、堵截数和空格数分类棋型
        /// </summary>
        private PatternType ClassifyPattern(int count, int block, int space)
        {
            if (count >= 5) return PatternType.Five;

            // 两端都被堵
            if (block >= 2) return PatternType.None;

            bool isLive = block == 0;

            if (count == 4)
            {
                return isLive ? PatternType.LiveFour : PatternType.Four;
            }
            if (count == 3)
            {
                // 活三需要足够空间
                if (isLive && space >= 1) return PatternType.LiveThree;
                return PatternType.Three;
            }
            if (count == 2)
            {
                if (isLive && space >= 2) return PatternType.LiveTwo;
                return PatternType.Two;
            }
            if (count == 1) return PatternType.One;

            return PatternType.None;
        }

        /// <summary>
        /// 评估特定位置的即时分数（用于候选排序）
        /// 使用 try-finally 确保棋盘状态始终还原
        /// </summary>
        public int EvaluatePosition(Board board, int x, int y, PieceType piece)
        {
            if (board.GetPiece(x, y) != PieceType.None)
                return 0;

            int score = 0;

            // 评估进攻价值
            foreach (var dir in Directions)
            {
                try
                {
                    board.PlacePiece(x, y, piece);
                    var pattern = DetectPattern(board, x, y, dir[0], dir[1], piece, out _);
                    score += _scores.GetScore(pattern);
                }
                finally
                {
                    board.RemovePiece(x, y);
                }
            }

            // 评估防守价值（阻止对手）
            var opponent = piece == PieceType.Black ? PieceType.White : PieceType.Black;
            foreach (var dir in Directions)
            {
                try
                {
                    board.PlacePiece(x, y, opponent);
                    var pattern = DetectPattern(board, x, y, dir[0], dir[1], opponent, out _);
                    score += _scores.GetScore(pattern) / 2;
                }
                finally
                {
                    board.RemovePiece(x, y);
                }
            }

            return score;
        }

        /// <summary>
        /// 获取所有候选位置并按分数排序
        /// </summary>
        public List<(int x, int y, int score)> GetSortedCandidates(Board board, PieceType piece, int topN = 20)
        {
            var candidates = new List<(int x, int y, int score)>();
            var added = new HashSet<(int, int)>();

            // 收集已有棋子附近的位置
            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (board.GetPiece(x, y) == PieceType.None) continue;

                    // 检查周围 2 格范围
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
                                int score = EvaluatePosition(board, nx, ny, piece);
                                candidates.Add((nx, ny, score));
                                added.Add((nx, ny));
                            }
                        }
                    }
                }
            }

            // 如果没有候选（开局），返回中心
            if (candidates.Count == 0)
            {
                int center = Board.BOARD_SIZE / 2;
                candidates.Add((center, center, 1000));
            }

            // 按分数降序排序
            candidates.Sort((a, b) => b.score.CompareTo(a.score));

            // 返回前 topN 个
            if (candidates.Count > topN)
            {
                candidates = candidates.GetRange(0, topN);
            }

            return candidates;
        }
    }
}
