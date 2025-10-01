using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsScreenController : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text winnerText;
    [SerializeField] private string playerOneWinsText = "Black Player Wins!";
    [SerializeField] private string playerTwoWinsText = "White Player Wins!";
    [SerializeField] private string drawText = "Game Draw!";
    
    // Called when the scene is loaded to display the winner
    public void DisplayWinner(int winner)
    {
        if (winnerText == null)
        {
            Debug.LogError("Winner Text is not assigned in ResultsScreenController");
            return;
        }
        
        switch (winner)
        {
            case 1:
                winnerText.text = playerOneWinsText;
                break;
            case 2:
                winnerText.text = playerTwoWinsText;
                break;
            default:
                winnerText.text = drawText;
                break;
        }
    }
    
    // Called when the "Play Again" button is clicked
    public void OnPlayAgainButtonClicked()
    {
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }
    
    // Called when the "Return to Menu" button is clicked
    public void OnReturnToMenuButtonClicked()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}