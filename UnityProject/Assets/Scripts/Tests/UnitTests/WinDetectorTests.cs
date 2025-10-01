using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using GomokuGame.Core;

namespace GomokuGame.Tests
{
    /// <summary>
    /// Unit tests for the WinDetector class
    /// </summary>
    public class WinDetectorTests
    {
        private WinDetector winDetector;
        private BoardManager boardManager;
        private GameObject testObject;

        [SetUp]
        public void Setup()
        {
            // Create test object
            testObject = new GameObject("TestObject");
            
            // Add required components
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
        public void CheckWin_HorizontalWin_ReturnsTrue()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            
            // Place 5 pieces in a horizontal line
            for (int i = 0; i < 5; i++)
            {
                boardManager.PlacePiece(7, 7 + i, GameManager.Player.Black);
            }

            // Act
            bool result = winDetector.CheckWin(7, 11, GameManager.Player.Black);

            // Assert
            Assert.IsTrue(result, "Horizontal win should be detected");
        }

        [Test]
        public void CheckWin_VerticalWin_ReturnsTrue()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            
            // Place 5 pieces in a vertical line
            for (int i = 0; i < 5; i++)
            {
                boardManager.PlacePiece(7 + i, 7, GameManager.Player.Black);
            }

            // Act
            bool result = winDetector.CheckWin(11, 7, GameManager.Player.Black);

            // Assert
            Assert.IsTrue(result, "Vertical win should be detected");
        }

        [Test]
        public void CheckWin_DiagonalWin_ReturnsTrue()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            
            // Place 5 pieces in a diagonal line
            for (int i = 0; i < 5; i++)
            {
                boardManager.PlacePiece(7 + i, 7 + i, GameManager.Player.Black);
            }

            // Act
            bool result = winDetector.CheckWin(11, 11, GameManager.Player.Black);

            // Assert
            Assert.IsTrue(result, "Diagonal win should be detected");
        }

        [Test]
        public void CheckWin_NoWin_ReturnsFalse()
        {
            // Arrange
            boardManager.InitializeBoard(15);
            
            // Place pieces that don't form a win
            boardManager.PlacePiece(7, 7, GameManager.Player.Black);
            boardManager.PlacePiece(7, 8, GameManager.Player.Black);
            boardManager.PlacePiece(7, 9, GameManager.Player.Black);
            boardManager.PlacePiece(7, 10, GameManager.Player.Black); // Only 4 in a row

            // Act
            bool result = winDetector.CheckWin(7, 10, GameManager.Player.Black);

            // Assert
            Assert.IsFalse(result, "Should not detect win with only 4 in a row");
        }

        [Test]
        public void CheckDirection_InvalidBoardManager_ReturnsFalse()
        {
            // Arrange
            WinDetector detector = new GameObject().AddComponent<WinDetector>();
            
            // Act
            bool result = detector.CheckDirection(0, 0, 1, 0, GameManager.Player.Black);

            // Assert
            Assert.IsFalse(result, "Should return false when board manager is null");
            
            // Cleanup
            GameObject.DestroyImmediate(detector.gameObject);
        }
    }
}