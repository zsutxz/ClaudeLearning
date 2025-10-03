using UnityEngine;`r`nusing System;`r`nusing GomokuGame.Core;`r`nusing GomokuGame.UI;`r`n
using GomokuGame.UI;
namespace GomokuGame.Core{
public class GameManager : MonoBehaviour
{
    public enum Player
    {
        None = 0,
        Black = 1,
        White = 2
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    [Header("Game State")]
    public Player currentPlayer = Player.Black;
    public GameState currentState = GameState.MainMenu;
    
    [Header("Board Settings")]
    public GomokuGame.Core.BoardManager boardManager;
    public WinDetector winDetector;

    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action<Player> OnPlayerChanged;
    public event Action<Player> OnGameWon;
    public event Action OnGameDraw;

    void Start()
    {
        // Initialize the board
        if (BoardViewManager != null)
        {
            BoardViewManager.InitializeBoard();
        }
    }

    public void SwitchPlayer()
    {
        currentPlayer = currentPlayer == Player.Black ? Player.White : Player.Black;
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    public bool CheckWin(int x, int y)
    {
        if (winDetector != null)
        {
            return winDetector.CheckWin(x, y, currentPlayer);
        }
        return false;
    }

    public void StartNewGame()
    {
        currentState = GameState.Playing;
        currentPlayer = Player.Black;
        OnGameStateChanged?.Invoke(currentState);
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    public void EndGame(Player winner)
    {
        currentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(currentState);
        OnGameWon?.Invoke(winner);
    }

    public void DeclareDraw()
    {
        currentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(currentState);
        OnGameDraw?.Invoke();
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        OnGameStateChanged?.Invoke(currentState);
    }
}





}
