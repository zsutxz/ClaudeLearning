// 测试文件 - 需要先安装 Test Framework
// Package Manager → Unity Registry → Test Framework
// 安装后取消下面的注释

/*
using UnityEngine;
using NUnit.Framework;

namespace Gomoku.Tests
{
    public class BoardTests
    {
        private Board _board;

        [SetUp]
        public void Setup()
        {
            _board = new Board();
        }

        [Test]
        public void TestBoardInitialization()
        {
            Assert.AreEqual(Board.BOARD_SIZE, 15);
        }

        [Test]
        public void TestPlacePiece()
        {
            Assert.IsTrue(_board.PlacePiece(7, 7, PieceType.Black));
            Assert.AreEqual(PieceType.Black, _board.GetPiece(7, 7));
        }

        [Test]
        public void TestHorizontalWin()
        {
            for (int i = 0; i < 5; i++)
            {
                _board.PlacePiece(5 + i, 7, PieceType.Black);
            }
            Assert.IsTrue(WinChecker.CheckWin(_board, 9, 7));
        }
    }
}
*/
