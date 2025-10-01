using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using GomokuGame.Core;

namespace GomokuGame.Tests
{
    /// <summary>
    /// Unit tests for the BoardManager class
    /// </summary>
    public class BoardManagerTests
    {
        private BoardManager boardManager;
        private GameObject testObject;

        [SetUp]
        public void Setup()
        {
            // Create test object
            testObject = new GameObject("TestObject");
            boardManager = testObject.AddComponent<BoardManager>();
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up
            if (testObject != null)
            {
                GameObject.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void InitializeBoard_DefaultSize_Creates15x15Board()
        {
            // Act
            boardManager.InitializeBoard();

            // Assert
            Assert.AreEqual(15, boardManager.BoardSize, "Board size should be 15");
            
            // Check that board state is initialized
            var boardState = boardManager.BoardState;
            Assert.IsNotNull(boardState, "Board state should not be null");
            Assert.AreEqual(15, boardState.GetLength(0), "Board state X dimension should be 15");
            Assert.AreEqual(15, boardState.GetLength(1), "Board state Y dimension should be 15");
        }

        [Test]
        public void InitializeBoard_CustomSize_CreatesCorrectSizeBoard()
        {
            // Arrange
            int customSize = 9;

            // Act
            boardManager.InitializeBoard(customSize);

            // Assert
            Assert.AreEqual(customSize, boardManager.BoardSize, $"Board size should be {customSize}");
            
            var boardState = boardManager.BoardState;
            Assert.IsNotNull(boardState, "Board state should not be null");
            Assert.AreEqual(customSize, boardState.GetLength(0), $"Board state X dimension should be {customSize}");
            Assert.AreEqual(customSize, boardState.GetLength(1), $"Board state Y dimension should be {customSize}");
        }

        [Test]
        public void PlacePiece_ValidPosition_PiecePlaced()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = 7, y = 7;
            GameManager.Player player = GameManager.Player.Black;

            // Act
            bool result = boardManager.PlacePiece(x, y, player);

            // Assert
            Assert.IsTrue(result, "Piece should be placed successfully");
            Assert.AreEqual(player, boardManager.GetPieceAt(x, y), "Piece should be placed at correct position");
        }

        [Test]
        public void PlacePiece_InvalidPosition_ReturnsFalse()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = -1, y = 7; // Invalid X coordinate
            GameManager.Player player = GameManager.Player.Black;

            // Act
            bool result = boardManager.PlacePiece(x, y, player);

            // Assert
            Assert.IsFalse(result, "Should return false for invalid position");
        }

        [Test]
        public void PlacePiece_OccupiedPosition_ReturnsFalse()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = 7, y = 7;
            GameManager.Player player1 = GameManager.Player.Black;
            GameManager.Player player2 = GameManager.Player.White;

            // Place first piece
            boardManager.PlacePiece(x, y, player1);

            // Act
            bool result = boardManager.PlacePiece(x, y, player2);

            // Assert
            Assert.IsFalse(result, "Should return false for occupied position");
            Assert.AreEqual(player1, boardManager.GetPieceAt(x, y), "Original piece should remain");
        }

        [Test]
        public void GetPieceAt_ValidPosition_ReturnsCorrectPiece()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = 7, y = 7;
            GameManager.Player player = GameManager.Player.White;
            boardManager.PlacePiece(x, y, player);

            // Act
            GameManager.Player result = boardManager.GetPieceAt(x, y);

            // Assert
            Assert.AreEqual(player, result, "Should return correct piece at position");
        }

        [Test]
        public void GetPieceAt_InvalidPosition_ReturnsNone()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = -1, y = 7; // Invalid X coordinate

            // Act
            GameManager.Player result = boardManager.GetPieceAt(x, y);

            // Assert
            Assert.AreEqual(GameManager.Player.None, result, "Should return None for invalid position");
        }

        [Test]
        public void IsPositionEmpty_EmptyPosition_ReturnsTrue()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = 7, y = 7;

            // Act
            bool result = boardManager.IsPositionEmpty(x, y);

            // Assert
            Assert.IsTrue(result, "Empty position should return true");
        }

        [Test]
        public void IsPositionEmpty_OccupiedPosition_ReturnsFalse()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            int x = 7, y = 7;
            boardManager.PlacePiece(x, y, GameManager.Player.Black);

            // Act
            bool result = boardManager.IsPositionEmpty(x, y);

            // Assert
            Assert.IsFalse(result, "Occupied position should return false");
        }

        [Test]
        public void ClearBoard_AllPositionsEmpty()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            boardManager.PlacePiece(7, 7, GameManager.Player.Black);
            boardManager.PlacePiece(8, 8, GameManager.Player.White);

            // Act
            boardManager.ClearBoard();

            // Assert
            Assert.AreEqual(GameManager.Player.None, boardManager.GetPieceAt(7, 7), "Position should be empty after clear");
            Assert.AreEqual(GameManager.Player.None, boardManager.GetPieceAt(8, 8), "Position should be empty after clear");
            
            // Check all positions are empty
            for (int x = 0; x < boardManager.BoardSize; x++)
            {
                for (int y = 0; y < boardManager.BoardSize; y++)
                {
                    Assert.IsTrue(boardManager.IsPositionEmpty(x, y), $"Position ({x},{y}) should be empty");
                }
            }
        }
    }
}