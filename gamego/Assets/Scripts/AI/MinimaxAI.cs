using System.Collections.Generic;
using System.Diagnostics;

namespace Gomoku
{
    public enum TTNodeType
    {
        Exact,
        Alpha,
        Beta
    }

    public struct TTEntry
    {
        public ulong Key;
        public int Depth;
        public int Score;
        public (int x, int y) BestMove;
        public TTNodeType NodeType;
    }

    /// <summary>
    /// Minimax + Alpha-Beta + 置换表 + 可选迭代加深
    /// </summary>
    public class MinimaxAI : IAIPlayer
    {
        private readonly int _maxDepth;
        private readonly bool _useIterativeDeepening;
        private readonly int _timeLimitMs;
        private const int BOARD_SIZE = Board.BOARD_SIZE;
        private readonly Dictionary<ulong, TTEntry> _transpositionTable;

        private long _searchStartTicks;
        private bool _isTimeout;
        private int _nodeCount;

        public MinimaxAI(int maxDepth = 3, bool useIterativeDeepening = false, int timeLimitMs = 2000)
        {
            _maxDepth = maxDepth;
            _useIterativeDeepening = useIterativeDeepening;
            _timeLimitMs = timeLimitMs;
            _transpositionTable = new Dictionary<ulong, TTEntry>();
        }

        public (int x, int y) GetMove(Board board, PieceType myPiece)
        {
            _nodeCount = 0;
            _isTimeout = false;
            _searchStartTicks = Stopwatch.GetTimestamp();

            PieceType opponentPiece = myPiece == PieceType.Black ? PieceType.White : PieceType.Black;
            List<(int x, int y)> candidates = GetCandidateMoves(board);

            if (candidates.Count == 0)
                return (BOARD_SIZE / 2, BOARD_SIZE / 2);

            // 查置换表获取上一次最佳走法，优先搜索
            candidates = ReorderWithTTBestMove(board.ZobristKey, candidates);

            int bestScore = int.MinValue;
            (int x, int y) bestMove = candidates[0];

            if (_useIterativeDeepening)
            {
                // 迭代加深：从深度 1 逐层搜索到 _maxDepth
                for (int depth = 1; depth <= _maxDepth; depth++)
                {
                    if (IsTimeUp()) break;

                    var (score, move) = SearchAtDepth(board, depth, candidates, myPiece, opponentPiece);

                    if (!_isTimeout)
                    {
                        bestScore = score;
                        bestMove = move;
                        // 将本层最佳走法提到候选列表首位，提高下一层剪枝效率
                        candidates = ReorderCandidates(candidates, move);
                    }
                    else
                    {
                        // 超时但得到了部分结果，使用比无结果好
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                        break;
                    }
                }
            }
            else
            {
                (bestScore, bestMove) = SearchAtDepth(board, _maxDepth, candidates, myPiece, opponentPiece);
            }

            // 存储根节点结果
            _transpositionTable[board.ZobristKey] = new TTEntry
            {
                Key = board.ZobristKey,
                Depth = _maxDepth,
                Score = bestScore,
                BestMove = bestMove,
                NodeType = TTNodeType.Exact
            };

            return bestMove;
        }

        private (int score, (int x, int y) move) SearchAtDepth(
            Board board, int depth, List<(int x, int y)> candidates,
            PieceType myPiece, PieceType opponentPiece)
        {
            int bestScore = int.MinValue;
            (int x, int y) bestMove = candidates[0];

            foreach (var (x, y) in candidates)
            {
                board.PlacePiece(x, y, myPiece);

                if (WinChecker.CheckWin(board, x, y))
                {
                    board.RemovePiece(x, y);
                    return (100000, (x, y));
                }

                int score = Minimax(board, depth - 1, int.MinValue, int.MaxValue, false, myPiece, opponentPiece);
                board.RemovePiece(x, y);

                if (_isTimeout) break;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (x, y);
                }
            }

            return (bestScore, bestMove);
        }

        private int Minimax(Board board, int depth, int alpha, int beta, bool isMaximizing,
                           PieceType myPiece, PieceType opponentPiece)
        {
            _nodeCount++;

            // 周期性检查超时（每 4096 个节点检查一次）
            if ((_nodeCount & 4095) == 0 && IsTimeUp())
            {
                _isTimeout = true;
                return EvaluateBoard(board, myPiece, opponentPiece);
            }

            // 置换表查询
            ulong key = board.ZobristKey;
            bool hasTTEntry = _transpositionTable.TryGetValue(key, out var entry);
            if (hasTTEntry && entry.Depth >= depth)
            {
                switch (entry.NodeType)
                {
                    case TTNodeType.Exact:
                        return entry.Score;
                    case TTNodeType.Alpha:
                        if (entry.Score <= alpha) return alpha;
                        break;
                    case TTNodeType.Beta:
                        if (entry.Score >= beta) return beta;
                        break;
                }
            }

            if (depth == 0)
            {
                int eval = EvaluateBoard(board, myPiece, opponentPiece);
                _transpositionTable[key] = new TTEntry
                {
                    Key = key, Depth = 0, Score = eval,
                    BestMove = (-1, -1), NodeType = TTNodeType.Exact
                };
                return eval;
            }

            List<(int x, int y)> candidates = GetCandidateMoves(board);

            if (candidates.Count == 0)
            {
                int eval = EvaluateBoard(board, myPiece, opponentPiece);
                _transpositionTable[key] = new TTEntry
                {
                    Key = key, Depth = depth, Score = eval,
                    BestMove = (-1, -1), NodeType = TTNodeType.Exact
                };
                return eval;
            }

            // 使用置换表中的最佳走法作为首选候选（即使深度不够也可用于排序）
            if (hasTTEntry && entry.BestMove.x >= 0 && entry.BestMove.y >= 0)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].x == entry.BestMove.x && candidates[i].y == entry.BestMove.y)
                    {
                        var best = candidates[i];
                        candidates.RemoveAt(i);
                        candidates.Insert(0, best);
                        break;
                    }
                }
            }

            int originalAlpha = alpha;

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                (int x, int y) bestMove = candidates[0];

                foreach (var (x, y) in candidates)
                {
                    if (_isTimeout) break;

                    board.PlacePiece(x, y, myPiece);

                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        _transpositionTable[key] = new TTEntry
                        {
                            Key = key, Depth = depth, Score = 100000,
                            BestMove = (x, y), NodeType = TTNodeType.Exact
                        };
                        return 100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, false, myPiece, opponentPiece);
                    board.RemovePiece(x, y);

                    if (_isTimeout) return maxScore == int.MinValue ? EvaluateBoard(board, myPiece, opponentPiece) : maxScore;

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestMove = (x, y);
                    }
                    alpha = System.Math.Max(alpha, score);

                    if (beta <= alpha)
                        break;
                }

                TTNodeType nodeType = maxScore <= originalAlpha ? TTNodeType.Alpha :
                                      maxScore >= beta ? TTNodeType.Beta : TTNodeType.Exact;

                _transpositionTable[key] = new TTEntry
                {
                    Key = key, Depth = depth, Score = maxScore,
                    BestMove = bestMove, NodeType = nodeType
                };

                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                (int x, int y) bestMove = candidates[0];

                foreach (var (x, y) in candidates)
                {
                    if (_isTimeout) break;

                    board.PlacePiece(x, y, opponentPiece);

                    if (WinChecker.CheckWin(board, x, y))
                    {
                        board.RemovePiece(x, y);
                        _transpositionTable[key] = new TTEntry
                        {
                            Key = key, Depth = depth, Score = -100000,
                            BestMove = (x, y), NodeType = TTNodeType.Exact
                        };
                        return -100000;
                    }

                    int score = Minimax(board, depth - 1, alpha, beta, true, myPiece, opponentPiece);
                    board.RemovePiece(x, y);

                    if (_isTimeout) return minScore == int.MaxValue ? EvaluateBoard(board, myPiece, opponentPiece) : minScore;

                    if (score < minScore)
                    {
                        minScore = score;
                        bestMove = (x, y);
                    }
                    beta = System.Math.Min(beta, score);

                    if (beta <= alpha)
                        break;
                }

                int originalBeta = beta;
                TTNodeType nodeType = minScore <= alpha ? TTNodeType.Alpha :
                                      minScore >= originalBeta ? TTNodeType.Beta : TTNodeType.Exact;

                _transpositionTable[key] = new TTEntry
                {
                    Key = key, Depth = depth, Score = minScore,
                    BestMove = bestMove, NodeType = nodeType
                };

                return minScore;
            }
        }

        private bool IsTimeUp()
        {
            long elapsedMs = (Stopwatch.GetTimestamp() - _searchStartTicks) * 1000 / Stopwatch.Frequency;
            return elapsedMs >= _timeLimitMs;
        }

        private List<(int x, int y)> ReorderWithTTBestMove(ulong key, List<(int x, int y)> candidates)
        {
            if (_transpositionTable.TryGetValue(key, out var entry) && entry.BestMove.x >= 0)
            {
                return ReorderCandidates(candidates, entry.BestMove);
            }
            return candidates;
        }

        private static List<(int x, int y)> ReorderCandidates(List<(int x, int y)> candidates, (int x, int y) priority)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].x == priority.x && candidates[i].y == priority.y)
                {
                    var best = candidates[i];
                    candidates.RemoveAt(i);
                    candidates.Insert(0, best);
                    break;
                }
            }
            return candidates;
        }

        public void ClearTranspositionTable()
        {
            _transpositionTable.Clear();
        }

        private int EvaluateBoard(Board board, PieceType myPiece, PieceType opponentPiece)
        {
            int myScore = EvaluatePiece(board, myPiece);
            int opponentScore = EvaluatePiece(board, opponentPiece);
            return myScore - opponentScore;
        }

        private int EvaluatePiece(Board board, PieceType piece)
        {
            int score = 0;

            for (int y = 0; y < BOARD_SIZE; y++)
                score += EvaluateLine(board, 0, y, 1, 0, piece);

            for (int x = 0; x < BOARD_SIZE; x++)
                score += EvaluateLine(board, x, 0, 0, 1, piece);

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                score += EvaluateLine(board, i, 0, 1, 1, piece);
                if (i > 0)
                    score += EvaluateLine(board, 0, i, 1, 1, piece);
            }

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                score += EvaluateLine(board, i, 0, -1, 1, piece);
                if (i > 0)
                    score += EvaluateLine(board, BOARD_SIZE - 1, i, -1, 1, piece);
            }

            return score;
        }

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
                    if (empty == 0) empty = 1;
                    else break;
                }
                else
                {
                    block++;
                    break;
                }

                x += dx;
                y += dy;
            }

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
