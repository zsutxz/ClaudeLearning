using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Player
    {
        None = 0,
        Black = 1,
        White = 2
    }

    [Header("Game State")]
    public Player currentPlayer = Player.Black;
    
    [Header("Board Settings")]
    public BoardManager boardManager;
    public WinDetector winDetector;

    void Start()
    {
        // Initialize the board
        if (boardManager != null)
        {
            boardManager.InitializeBoard();
        }
    }

    public void SwitchPlayer()
    {
        currentPlayer = currentPlayer == Player.Black ? Player.White : Player.Black;
    }

    public bool CheckWin(int x, int y)
    {
        if (winDetector != null)
        {
            return winDetector.CheckWin(x, y, currentPlayer);
        }
        return false;
    }
}