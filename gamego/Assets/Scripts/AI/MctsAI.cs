using System.Collections.Generic;
using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 蒙特卡洛树搜索 AI
    /// 使用 UCB1 选择 + 随机模拟 + 回溯更新
    /// </summary>
    public class MctsAI : IAIPlayer
    {
        private readonly int _simulationCount;
        private readonly float _explorationFactor;
        private readonly System.Random _random;
        private readonly BoardEvaluator _evaluator;

        public MctsAI(int simulationCount = 1000, float explorationFactor = 1.41f)
        {
            _simulationCount = simulationCount;
            _explorationFactor = explorationFactor;
            _random = new System.Random();
            _evaluator = new BoardEvaluator();
        }

        public (int x, int y) GetMove(Board board, PieceType myPiece)
        {
            // 空棋盘直接下中心
            if (board.PieceCount == 0)
                return (Board.BOARD_SIZE / 2, Board.BOARD_SIZE / 2);

            var candidates = GetCandidateMoves(board);
            if (candidates.Count == 0)
                return (Board.BOARD_SIZE / 2, Board.BOARD_SIZE / 2);

            if (candidates.Count == 1)
                return candidates[0];

            // 必胜检查
            var winMove = FindWinningMove(board, myPiece);
            if (winMove.HasValue) return winMove.Value;

            // 阻止对手必胜
            var opponent = myPiece == PieceType.Black ? PieceType.White : PieceType.Black;
            var blockMove = FindWinningMove(board, opponent);
            if (blockMove.HasValue) return blockMove.Value;

            // MCTS 搜索
            var root = new MctsNode(null, (-1, -1), myPiece);
            root.Visits = 1;  // 避免首次 UCB1 中 log(0)
            root.Expand(candidates, opponent);

            for (int i = 0; i < _simulationCount; i++)
            {
                var simBoard = CloneBoard(board);
                PieceType currentPlayer = myPiece;
                var node = root;

                // 1. 选择 - 沿树向下走
                while (node.Children.Count > 0)
                {
                    node = SelectChild(node);
                    simBoard.PlacePiece(node.Move.x, node.Move.y, currentPlayer);
                    currentPlayer = Opponent(currentPlayer);

                    if (WinChecker.CheckWin(simBoard, node.Move.x, node.Move.y))
                        break;
                }

                // 2. 扩展
                if (node.Visits > 0 && !IsTerminal(simBoard, node.Move))
                {
                    var childCandidates = GetCandidateMoves(simBoard);
                    if (childCandidates.Count > 0)
                    {
                        node.Expand(childCandidates, currentPlayer);
                        node = SelectChild(node);
                        simBoard.PlacePiece(node.Move.x, node.Move.y, currentPlayer);
                        currentPlayer = Opponent(currentPlayer);
                    }
                }

                // 3. 模拟
                double result = Simulate(simBoard, currentPlayer, myPiece);

                // 4. 回溯 - 从每个节点自己的 Player 视角记录胜率
                while (node != null)
                {
                    node.Visits++;
                    // result 是从 myPiece 视角的胜率
                    // 转换为从 node.Player 视角
                    if (node.Player == myPiece)
                        node.Wins += result;
                    else
                        node.Wins += (1.0 - result);
                    node = node.Parent;
                }
            }

            return SelectBestMove(root);
        }

        /// <summary>
        /// 选择子节点 - 从父节点视角最大化胜率
        /// 子节点的 Wins 是从子节点 Player 视角的，
        /// 所以需要翻转：exploitation = 1 - child.Wins/child.Visits
        /// </summary>
        private MctsNode SelectChild(MctsNode node)
        {
            MctsNode bestChild = null;
            double bestUcb = double.MinValue;

            foreach (var child in node.Children)
            {
                double ucb;
                if (child.Visits == 0)
                {
                    ucb = double.MaxValue;
                }
                else
                {
                    // 从父节点视角：翻转子节点的胜率
                    double exploitation = 1.0 - (child.Wins / child.Visits);
                    double exploration = _explorationFactor * System.Math.Sqrt(
                        System.Math.Log(node.Visits) / child.Visits);
                    ucb = exploitation + exploration;
                }

                if (ucb > bestUcb)
                {
                    bestUcb = ucb;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        /// <summary>
        /// 随机模拟 - 返回从 myPiece 视角的胜率 (0-1)
        /// </summary>
        private double Simulate(Board board, PieceType currentPlayer, PieceType myPiece)
        {
            var simBoard = CloneBoard(board);

            for (int i = 0; i < 50; i++)
            {
                var candidates = GetCandidateMoves(simBoard);
                if (candidates.Count == 0)
                    return 0.5;

                var move = candidates[_random.Next(candidates.Count)];
                simBoard.PlacePiece(move.x, move.y, currentPlayer);

                if (WinChecker.CheckWin(simBoard, move.x, move.y))
                    return currentPlayer == myPiece ? 1.0 : 0.0;

                currentPlayer = Opponent(currentPlayer);
            }

            // 超时用评估器判断
            int score = _evaluator.Evaluate(simBoard, myPiece);
            if (score > 10000) return 0.8;
            if (score > 1000) return 0.6;
            if (score < -10000) return 0.2;
            if (score < -1000) return 0.4;
            return 0.5;
        }

        private (int x, int y) SelectBestMove(MctsNode root)
        {
            (int x, int y) bestMove = (Board.BOARD_SIZE / 2, Board.BOARD_SIZE / 2);
            int maxVisits = -1;

            foreach (var child in root.Children)
            {
                if (child.Visits > maxVisits)
                {
                    maxVisits = child.Visits;
                    bestMove = child.Move;
                }
            }
            return bestMove;
        }

        private (int x, int y)? FindWinningMove(Board board, PieceType piece)
        {
            var candidates = GetCandidateMoves(board);
            foreach (var (x, y) in candidates)
            {
                board.PlacePiece(x, y, piece);
                bool wins = WinChecker.CheckWin(board, x, y);
                board.RemovePiece(x, y);
                if (wins) return (x, y);
            }
            return null;
        }

        private bool IsTerminal(Board board, (int x, int y) lastMove)
        {
            if (lastMove.x < 0 || lastMove.y < 0) return false;
            return WinChecker.CheckWin(board, lastMove.x, lastMove.y) || board.IsFull();
        }

        private static PieceType Opponent(PieceType piece)
        {
            return piece == PieceType.Black ? PieceType.White : PieceType.Black;
        }

        private List<(int x, int y)> GetCandidateMoves(Board board)
        {
            var candidates = new List<(int x, int y)>();
            var added = new HashSet<(int, int)>();

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (board.GetPiece(x, y) == PieceType.None) continue;

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
            return candidates;
        }

        private Board CloneBoard(Board original)
        {
            var clone = new Board();
            // 直接复制 Grid 并通过 PlacePiece 同步 _pieceCount
            var src = original.Grid;
            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    if (src[x, y] != PieceType.None)
                    {
                        clone.PlacePiece(x, y, src[x, y]);
                    }
                }
            }
            return clone;
        }

        /// <summary>
        /// MCTS 节点
        /// </summary>
        private class MctsNode
        {
            public MctsNode Parent;
            public readonly List<MctsNode> Children;
            public (int x, int y) Move;
            public PieceType Player;  // 执行此着法的玩家
            public int Visits;
            public double Wins;  // 从 Player 视角的胜次数

            public MctsNode(MctsNode parent, (int x, int y) move, PieceType player)
            {
                Parent = parent;
                Move = move;
                Player = player;
                Children = new List<MctsNode>();
                Visits = 0;
                Wins = 0;
            }

            public void Expand(List<(int x, int y)> candidates, PieceType nextPlayer)
            {
                foreach (var (x, y) in candidates)
                {
                    if (Children.Exists(c => c.Move.x == x && c.Move.y == y))
                        continue;
                    Children.Add(new MctsNode(this, (x, y), nextPlayer));
                }
            }
        }
    }
}
