using UnityEngine;
using GomokuGame.UI;

namespace GomokuGame.Core
{
    /// <summary>
    /// Initializes the game when the scene loads
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        void Start()
        {
            // Ensure UIManager exists
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.Log("UIManager not found in scene, creating one");
                GameObject uiManagerObject = new GameObject("UIManager");
                uiManagerObject.AddComponent<UIManager>();
            }
            
            // Ensure GameManager exists and is properly initialized
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("GameInitializer: GameManager found, ensuring proper initialization");
                
                // UIManager should already be set up to subscribe to game events in its Start method
                uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    Debug.Log("GameInitializer: UIManager found, it should handle event subscriptions");
                }
            }
            else
            {
                Debug.LogWarning("GameInitializer: GameManager not found in scene!");
            }
            
            // Ensure InputManager exists
            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                Debug.Log("InputManager not found in scene, creating one");
                GameObject inputManagerObject = new GameObject("InputManager");
                inputManagerObject.AddComponent<InputManager>();
            }
        }
    }
}