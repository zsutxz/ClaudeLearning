using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GomokuGame.UI{
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

    /// <summary>
    /// Applies the current theme to results screen UI elements
    /// </summary>
    public void ApplyThemeToResultsUI()
    {
        // Get the current theme settings
        GomokuGame.Themes.ThemeSettings themeSettings = null;
        GomokuGame.Themes.ThemeManager themeManager = FindObjectOfType<GomokuGame.Themes.ThemeManager>();
        if (themeManager != null)
        {
            themeSettings = themeManager.GetCurrentThemeSettings();
        }

        // Apply theme to winner text
        if (winnerText != null)
        {
            // Use white piece color for winner text
            winnerText.color = (themeSettings != null) ? themeSettings.whitePieceColor : Color.white;
        }

        // Apply theme to button texts
        if (playAgainButton != null)
        {
            Text playAgainText = playAgainButton.GetComponentInChildren<Text>();
            if (playAgainText != null)
            {
                playAgainText.color = (themeSettings != null) ? themeSettings.whitePieceColor : Color.white;
            }
        }

        if (returnToMenuButton != null)
        {
            Text returnToMenuText = returnToMenuButton.GetComponentInChildren<Text>();
            if (returnToMenuText != null)
            {
                returnToMenuText.color = (themeSettings != null) ? themeSettings.whitePieceColor : Color.white;
            }
        }

        // Apply theme to button backgrounds
        ApplyThemeToButtonBackground(playAgainButton, themeSettings);
        ApplyThemeToButtonBackground(returnToMenuButton, themeSettings);
    }

    /// <summary>
    /// Applies theme to button background color
    /// </summary>
    /// <param name="button">Button to apply theme to</param>
    /// <param name="themeSettings">Current theme settings</param>
    private void ApplyThemeToButtonBackground(Button button, GomokuGame.Themes.ThemeSettings themeSettings)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Use a darker version of the theme's board line color or default to dark gray
                Color backgroundColor = (themeSettings != null) ? themeSettings.boardLineColor : new Color(0.2f, 0.2f, 0.2f, 1f);
                // Darken the color further for button background
                backgroundColor.r *= 0.8f;
                backgroundColor.g *= 0.8f;
                backgroundColor.b *= 0.8f;
                buttonImage.color = backgroundColor;
            }
        }
    }
}
}
