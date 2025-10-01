using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using GomokuGame.Core;

namespace GomokuGame.Tests
{
    /// <summary>
    /// Unit tests for the GameManager class
    /// </summary>
    public class GameManagerTests
    {
        private GameManager gameManager;
        private GameObject testObject;

        [SetUp]
        public void Setup()
        {
            // Create test object
            testObject = new GameObject("TestObject");
            gameManager = testObject.AddComponent<GameManager>();
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
        public void GameManager_Singleton_InstanceCreated()
        {
            // Assert
            Assert.IsNotNull(GameManager.Instance, "GameManager instance should be created");
        }

        [Test]
        public void StartNewGame_SetsCorrectInitialState()
        {
            // Arrange
            bool gameStarted = false;
            bool stateChanged = false;
            
            gameManager.OnGameStarted += () => gameStarted = true;
            gameManager.OnGameStateChanged += (state) => stateChanged = (state == GameManager.GameState.Playing);

            // Act
            gameManager.StartNewGame();

            // Assert
            Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentState, "Game state should be Playing");
            Assert.AreEqual(GameManager.Player.Black, gameManager.CurrentPlayer, "Current player should be Black");
            Assert.IsTrue(gameStarted, "GameStarted event should be fired");
            Assert.IsTrue(stateChanged, "GameStateChanged event should be fired with Playing state");
        }

        [Test]
        public void SwitchPlayer_TogglesBetweenPlayers()
        {
            // Arrange
            GameManager.Player initialPlayer = gameManager.CurrentPlayer;
            GameManager.Player switchedPlayer = GameManager.Player.None;
            
            gameManager.OnPlayerChanged += (player) => switchedPlayer = player;

            // Act
            gameManager.SwitchPlayer();

            // Assert
            Assert.AreNotEqual(initialPlayer, gameManager.CurrentPlayer, "Player should have switched");
            Assert.AreEqual(switchedPlayer, gameManager.CurrentPlayer, "PlayerChanged event should fire with correct player");
        }

        [Test]
        public void SwitchPlayer_Twice_ReturnsToOriginalPlayer()
        {
            // Arrange
            GameManager.Player originalPlayer = gameManager.CurrentPlayer;

            // Act
            gameManager.SwitchPlayer();
            gameManager.SwitchPlayer();

            // Assert
            Assert.AreEqual(originalPlayer, gameManager.CurrentPlayer, "Player should return to original after two switches");
        }

        [Test]
        public void EndGame_SetsGameOverState()
        {
            // Arrange
            GameManager.GameState initialState = gameManager.CurrentState;
            GameManager.GameState finalState = GameManager.GameState.MainMenu;
            
            gameManager.OnGameStateChanged += (state) => finalState = state;

            // Act
            gameManager.EndGame();

            // Assert
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game state should be GameOver");
            Assert.AreEqual(GameManager.GameState.GameOver, finalState, "GameStateChanged event should fire with GameOver state");
        }

        [Test]
        public void DeclareWinner_SetsGameOverAndFiresEvent()
        {
            // Arrange
            GameManager.Player winner = GameManager.Player.Black;
            GameManager.Player declaredWinner = GameManager.Player.None;
            bool gameEnded = false;
            
            gameManager.OnGameWon += (player) => declaredWinner = player;
            gameManager.OnGameStateChanged += (state) => gameEnded = (state == GameManager.GameState.GameOver);

            // Act
            gameManager.DeclareWinner(winner);

            // Assert
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game state should be GameOver");
            Assert.AreEqual(winner, declaredWinner, "GameWon event should fire with correct winner");
            Assert.IsTrue(gameEnded, "GameStateChanged event should fire with GameOver state");
        }

        [Test]
        public void DeclareDraw_SetsGameOverAndFiresEvent()
        {
            // Arrange
            bool gameDrawn = false;
            bool gameEnded = false;
            
            gameManager.OnGameDraw += () => gameDrawn = true;
            gameManager.OnGameStateChanged += (state) => gameEnded = (state == GameManager.GameState.GameOver);

            // Act
            gameManager.DeclareDraw();

            // Assert
            Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState, "Game state should be GameOver");
            Assert.IsTrue(gameDrawn, "GameDraw event should fire");
            Assert.IsTrue(gameEnded, "GameStateChanged event should fire with GameOver state");
        }

        [Test]
        public void UpdateSettings_UpdatesBoardSizeAndWinCondition()
        {
            // Arrange
            int newBoardSize = 9;
            int newWinCondition = 3;

            // Act
            gameManager.UpdateSettings(newBoardSize, newWinCondition);

            // Assert
            Assert.AreEqual(newBoardSize, gameManager.BoardSize, "Board size should be updated");
            Assert.AreEqual(newWinCondition, gameManager.WinCondition, "Win condition should be updated");
        }
    }
}