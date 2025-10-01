using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using GomokuGame.Core;

namespace GomokuGame.Tests
{
    /// <summary>
    /// Integration tests for the complete game flow
    /// </summary>
    public class GameFlowTests
    {
        private GameManager gameManager;
        private BoardManager boardManager;
        private WinDetector winDetector;
        private GameObject testObject;

        [SetUp]
        public void Setup()
        {
            // Create test object
            testObject = new GameObject("TestObject");
            
            // Add required components
            gameManager = testObject.AddComponent<GameManager>();
            boardManager = testObject.AddComponent<BoardManager>();
            winDetector = testObject.AddComponent<WinDetector>();
            
            // Set up win detector
            winDetector.GetType().GetField("boardManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(winDetector, boardManager);
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
        public void GameFlow_StartGame_InitializesCorrectly()
        {
            // Act
            gameManager.StartNewGame();

            // Assert
            Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentState, "Game should be in playing state");
            Assert.AreEqual(GameManager.Player.Black, gameManager.CurrentPlayer, "First player should be Black");
        }

        [Test]
        public void GameFlow_PlacePiece_SwitchesPlayer()
        {
            // Arrange
            gameManager.StartNewGame();
            GameManager.Player initialPlayer = gameManager.CurrentPlayer;

            // Act
            boardManager.PlacePiece(7, 7, initialPlayer);
            gameManager.SwitchPlayer();

            // Assert
            Assert.AreNotEqual(initialPlayer, gameManager.CurrentPlayer, "Player should switch after placing piece");
        }

        [Test]
        public void GameFlow_HorizontalWin_DeclaresWinner()
        {
            // Arrange
            gameManager.StartNewGame();
            GameManager.Player winner = GameManager.Player.Black;
            bool gameWon = false;
            
            gameManager.OnGameWon += (player) => 
            {
                if (player == winner) gameWon = true;
            };

            // Place 5 pieces in a horizontal line
            for (int i = 0; i < 5; i++)
            {
                boardManager.PlacePiece(7, 7 + i, winner);
                if (i < 4) gameManager.SwitchPlayer(); // Don't switch after the last piece
            }

            // Check for win after placing the last piece
            bool isWin = winDetector.CheckWin(7, 11, winner);

            // Act
            if (isWin)
            {
                gameManager.DeclareWinner(winner);
            }

            // Assert
            Assert.IsTrue(isWin, "Win condition should be detected");
            Assert.IsTrue(gameWon, "Game should declare the correct winner");
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game should be in game over state");
        }

        [Test]
        public void GameFlow_VerticalWin_DeclaresWinner()
        {
            // Arrange
            gameManager.StartNewGame();
            GameManager.Player winner = GameManager.Player.White;
            bool gameWon = false;
            
            gameManager.OnGameWon += (player) => 
            {
                if (player == winner) gameWon = true;
            };

            // Place 5 pieces in a vertical line
            for (int i = 0; i < 5; i++)
            {
                gameManager.SwitchPlayer(); // Switch to White player first
                boardManager.PlacePiece(7 + i, 7, winner);
                if (i < 4) gameManager.SwitchPlayer(); // Don't switch after the last piece
            }

            // Check for win after placing the last piece
            bool isWin = winDetector.CheckWin(11, 7, winner);

            // Act
            if (isWin)
            {
                gameManager.DeclareWinner(winner);
            }

            // Assert
            Assert.IsTrue(isWin, "Win condition should be detected");
            Assert.IsTrue(gameWon, "Game should declare the correct winner");
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game should be in game over state");
        }

        [Test]
        public void GameFlow_DrawCondition_DeclaresDraw()
        {
            // Arrange
            gameManager.StartNewGame();
            boardManager.InitializeBoard(3); // Small board for easier testing
            bool gameDrawn = false;
            
            gameManager.OnGameDraw += () => gameDrawn = true;

            // Fill the board without creating a win (this is a simplified test)
            // In a real 3x3 game, it's impossible to fill without winning, but we'll simulate
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    GameManager.Player player = (x + y) % 2 == 0 ? GameManager.Player.Black : GameManager.Player.White;
                    boardManager.PlacePiece(x, y, player);
                    if (!(x == 2 && y == 2)) // Don't switch after the last piece
                    {
                        gameManager.SwitchPlayer();
                    }
                }
            }

            // Check for draw
            bool isDraw = winDetector.CheckDraw();

            // Act
            if (isDraw)
            {
                gameManager.DeclareDraw();
            }

            // Assert
            Assert.IsTrue(isDraw, "Draw condition should be detected");
            Assert.IsTrue(gameDrawn, "Game should declare a draw");
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game should be in game over state");
        }
    }
}