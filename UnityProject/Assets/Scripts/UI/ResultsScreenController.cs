using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultsScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text winnerText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button returnToMenuButton;
    
    [Header("Display Texts")]
    [SerializeField] private string playerOneWinsText = "Black Player Wins!";
    [SerializeField] private string playerTwoWinsText = "White Player Wins!";
    [SerializeField] private string drawText = "Game Draw!";
    
    private void Start()
    {
        // Add button click listeners
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
            AddButtonHoverEffects(playAgainButton);
        }
        
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);
            AddButtonHoverEffects(returnToMenuButton);
        }
    }
    
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
        SceneManager.LoadScene("MainGame");
    }
    
    // Called when the "Return to Menu" button is clicked
    public void OnReturnToMenuButtonClicked()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
    
    // Add hover effects to a button
    private void AddButtonHoverEffects(Button button)
    {
        // Add hover color change effect
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray when hovered
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Darker gray when pressed
        button.colors = colors;
    }
}