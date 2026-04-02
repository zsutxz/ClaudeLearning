using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Gomoku;

namespace Gomoku.Tests
{
    /// <summary>
    /// AI 系统测试 - 覆盖 BoardEvaluator、MinimaxAI、MctsAI、AIFactory
    /// 在 Unity Test Runner 的 PlayMode 标签下运行
    /// </summary>
    public class AITests
    {
        private Board _board;

        [SetUp]
        public void Setup()
        {
            _board = new Board();
        }

        #region BoardEvaluator Tests

        [Test]
        public void BoardEvaluator_LiveFour_Detected()
        {
            // . X X X X .  => 活四
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            var pattern = evaluator.DetectPattern(_board, 7, 7, 1, 0, PieceType.Black, out int len);

            Assert.AreEqual(PatternType.LiveFour, pattern, "Should detect LiveFour for 4 consecutive pieces with open ends");
        }

        [Test]
        public void BoardEvaluator_Five_Detected()
        {
            // X X X X X => 五连
            for (int i = 0; i < 5; i++)
                _board.PlacePiece(5 + i, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            var pattern = evaluator.DetectPattern(_board, 7, 7, 1, 0, PieceType.Black, out _);

            Assert.AreEqual(PatternType.Five, pattern, "Should detect Five for 5 consecutive pieces");
        }

        [Test]
        public void BoardEvaluator_LiveThree_Detected()
        {
            // . X X X . .  => 活三（两端开放）
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            var pattern = evaluator.DetectPattern(_board, 7, 7, 1, 0, PieceType.Black, out _);

            Assert.AreEqual(PatternType.LiveThree, pattern, "Should detect LiveThree for 3 consecutive pieces with open ends");
        }

        [Test]
        public void BoardEvaluator_BlockedFour_Detected()
        {
            // O X X X X .  => 冲四（一端被堵）
            _board.PlacePiece(4, 7, PieceType.White);  // 堵一端
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            var pattern = evaluator.DetectPattern(_board, 7, 7, 1, 0, PieceType.Black, out _);

            Assert.AreEqual(PatternType.Four, pattern, "Should detect Four (blocked) for 4 pieces with one end blocked");
        }

        [Test]
        public void BoardEvaluator_Evaluate_ReturnsPositiveForAdvantage()
        {
            // 黑棋有 3 子连线，白棋没有
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            int score = evaluator.Evaluate(_board, PieceType.Black);

            Assert.Greater(score, 0, "Black should have positive score with 3 in a row");
        }

        [Test]
        public void BoardEvaluator_EvaluatePosition_RanksWinningMoveHighest()
        {
            // 黑棋有 4 子连线，第 5 个位置应该得分最高
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            int winScore = evaluator.EvaluatePosition(_board, 9, 7, PieceType.Black);
            int randomScore = evaluator.EvaluatePosition(_board, 0, 0, PieceType.Black);

            Assert.Greater(winScore, randomScore, "Winning move should score higher than random position");
        }

        [Test]
        public void BoardEvaluator_GetSortedCandidates_ReturnsBestFirst()
        {
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);

            var evaluator = new BoardEvaluator();
            var candidates = evaluator.GetSortedCandidates(_board, PieceType.Black, topN: 5);

            Assert.Greater(candidates.Count, 0, "Should have candidates");
            // 第一个候选应该是能形成 4 连的位置
            Assert.IsTrue(candidates[0].x == 4 || candidates[0].x == 8,
                $"Best candidate should extend the line, got ({candidates[0].x}, {candidates[0].y})");
        }

        #endregion

        #region MinimaxAI Tests

        [Test]
        public void MinimaxAI_FindsImmediateWin()
        {
            // 黑棋 4 连，应该立即下第 5 个
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            // 对手有一些无关的棋子
            _board.PlacePiece(0, 0, PieceType.White);
            _board.PlacePiece(1, 0, PieceType.White);

            var ai = new MinimaxAI(2);
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.AreEqual(9, move.x, "Should find winning move at x=9");
            Assert.AreEqual(7, move.y, "Should find winning move at y=7");
        }

        [Test]
        public void MinimaxAI_BlocksOpponentWin()
        {
            // 白棋 4 连，黑棋应该堵住
            _board.PlacePiece(0, 0, PieceType.Black);  // 黑棋先手
            _board.PlacePiece(5, 7, PieceType.White);
            _board.PlacePiece(6, 7, PieceType.White);
            _board.PlacePiece(7, 7, PieceType.White);
            _board.PlacePiece(8, 7, PieceType.White);

            var ai = new MinimaxAI(2);
            var move = ai.GetMove(_board, PieceType.Black);

            // 应该堵住 4 或 9
            Assert.IsTrue(move.x == 4 || move.x == 9,
                $"Should block opponent's 4-in-a-row, got ({move.x}, {move.y})");
            Assert.AreEqual(7, move.y);
        }

        [Test]
        public void MinimaxAI_Depth1_FasterThanDepth3()
        {
            // 设置一些棋子
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 8, PieceType.White);
            _board.PlacePiece(7, 8, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.White);

            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var ai1 = new MinimaxAI(1);
            ai1.GetMove(_board, PieceType.Black);
            sw1.Stop();

            var sw3 = System.Diagnostics.Stopwatch.StartNew();
            var ai3 = new MinimaxAI(3);
            ai3.GetMove(_board, PieceType.Black);
            sw3.Stop();

            Assert.Less(sw1.ElapsedMilliseconds, sw3.ElapsedMilliseconds + 100,
                "Depth 1 should be faster than depth 3");
        }

        #endregion

        #region MctsAI Tests

        [Test]
        public void MctsAI_FindsWinningMove()
        {
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);
            _board.PlacePiece(0, 0, PieceType.White);
            _board.PlacePiece(1, 0, PieceType.White);

            var ai = new MctsAI(simulationCount: 200);
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.AreEqual(9, move.x, "MCTS should find winning move");
            Assert.AreEqual(7, move.y);
        }

        [Test]
        public void MctsAI_PlaysCenterOnEmptyBoard()
        {
            var ai = new MctsAI(simulationCount: 100);
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.AreEqual(7, move.x, "Should play center on empty board");
            Assert.AreEqual(7, move.y);
        }

        [Test]
        public void MctsAI_BlocksOpponentFour()
        {
            _board.PlacePiece(0, 0, PieceType.Black);
            _board.PlacePiece(5, 7, PieceType.White);
            _board.PlacePiece(6, 7, PieceType.White);
            _board.PlacePiece(7, 7, PieceType.White);
            _board.PlacePiece(8, 7, PieceType.White);

            var ai = new MctsAI(simulationCount: 500);
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.IsTrue(move.x == 4 || move.x == 9,
                $"MCTS should block opponent's 4-in-a-row, got ({move.x}, {move.y})");
            Assert.AreEqual(7, move.y);
        }

        #endregion

        #region AIFactory Tests

        [Test]
        public void AIFactory_CreatesSimpleAI()
        {
            var ai = AIFactory.CreateAI(AIType.Simple, Difficulty.Easy);
            Assert.IsNotNull(ai, "Factory should create SimpleAI");
            Assert.IsInstanceOf<SimpleAI>(ai, "Should be SimpleAI instance");
        }

        [Test]
        public void AIFactory_CreatesMinimaxAI()
        {
            var ai = AIFactory.CreateAI(AIType.Minimax, Difficulty.Medium);
            Assert.IsNotNull(ai, "Factory should create MinimaxAI");
            Assert.IsInstanceOf<MinimaxAI>(ai, "Should be MinimaxAI instance");
        }

        [Test]
        public void AIFactory_CreatesMctsAI()
        {
            var ai = AIFactory.CreateAI(AIType.Mcts, Difficulty.Hard);
            Assert.IsNotNull(ai, "Factory should create MctsAI");
            Assert.IsInstanceOf<MctsAI>(ai, "Should be MctsAI instance");
        }

        [Test]
        public void AIFactory_AllTypesAndDifficulties_ReturnNonNull()
        {
            var types = new[] { AIType.Simple, AIType.Minimax, AIType.Mcts };
            var diffs = new[] { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };

            foreach (var type in types)
            {
                foreach (var diff in diffs)
                {
                    var ai = AIFactory.CreateAI(type, diff);
                    Assert.IsNotNull(ai, $"Factory should return non-null for {type}/{diff}");
                }
            }
        }

        #endregion

        #region SimpleAI Tests

        [Test]
        public void SimpleAI_FindsWinningMove()
        {
            _board.PlacePiece(5, 7, PieceType.Black);
            _board.PlacePiece(6, 7, PieceType.Black);
            _board.PlacePiece(7, 7, PieceType.Black);
            _board.PlacePiece(8, 7, PieceType.Black);

            var ai = new SimpleAI(0.0f);  // 无随机
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.AreEqual(9, move.x, "SimpleAI should find winning move");
            Assert.AreEqual(7, move.y);
        }

        [Test]
        public void SimpleAI_BlocksOpponentWin()
        {
            _board.PlacePiece(0, 0, PieceType.Black);
            _board.PlacePiece(5, 7, PieceType.White);
            _board.PlacePiece(6, 7, PieceType.White);
            _board.PlacePiece(7, 7, PieceType.White);
            _board.PlacePiece(8, 7, PieceType.White);

            var ai = new SimpleAI(0.0f);
            var move = ai.GetMove(_board, PieceType.Black);

            Assert.IsTrue(move.x == 4 || move.x == 9,
                "SimpleAI should block opponent's winning move");
            Assert.AreEqual(7, move.y);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void AI_Battle_MinimaxVsSimple_MinimaxShouldWin()
        {
            int minimaxWins = 0;
            int games = 5;

            for (int g = 0; g < games; g++)
            {
                var board = new Board();
                var minimax = new MinimaxAI(2);
                var simple = new SimpleAI(0.0f);
                PieceType current = PieceType.Black;

                for (int move = 0; move < 100; move++)
                {
                    var (x, y) = current == PieceType.Black
                        ? minimax.GetMove(board, PieceType.Black)
                        : simple.GetMove(board, PieceType.White);

                    board.PlacePiece(x, y, current);

                    if (WinChecker.CheckWin(board, x, y))
                    {
                        if (current == PieceType.Black) minimaxWins++;
                        break;
                    }

                    if (board.IsFull()) break;
                    current = current == PieceType.Black ? PieceType.White : PieceType.Black;
                }
            }

            Assert.Greater(minimaxWins, 0,
                $"Minimax should win at least 1 of {games} games against SimpleAI, won {minimaxWins}");
        }

        #endregion
    }
}
