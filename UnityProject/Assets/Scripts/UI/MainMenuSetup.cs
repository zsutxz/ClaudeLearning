using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GomokuGame.UI
{
    /// <summary>
    /// Sets up the main menu scene with all required UI elements
    /// </summary>
    public class MainMenuSetup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject mainMenuPrefab;
        
        void Start()
        {
            SetupMainMenu();
        }
        
        /// <summary>
        /// Sets up the main menu with all required UI elements
        /// </summary>
        private void SetupMainMenu()
        {
            // Check if we already have a canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            if (canvases.Length > 0)
            {
                // Canvas already exists, don't create a new one
                return;
            }
            
            // Create canvas if it doesn't exist
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // If we have a main menu prefab, instantiate it
            if (mainMenuPrefab != null)
            {
                GameObject mainMenuInstance = Instantiate(mainMenuPrefab, canvasObject.transform);
                mainMenuInstance.name = "MainMenu";
                
                // Connect buttons to MainMenuController
                ConnectButtons(mainMenuInstance);
            }
            else
            {
                // Create the main menu panel manually
                GameObject mainMenuPanel = CreateMainMenuPanel(canvasObject);
                
                // Connect buttons to MainMenuController
                ConnectButtons(mainMenuPanel);
            }
        }
        
        /// <summary>
        /// Connects UI buttons to the MainMenuController
        /// </summary>
        /// <param name="mainMenuPanel">Main menu panel object</param>
        private void ConnectButtons(GameObject mainMenuPanel)
        {
            MainMenuController controller = FindObjectOfType<MainMenuController>();
            if (controller == null)
            {
                controller = mainMenuPanel.AddComponent<MainMenuController>();
            }
            
            // Find and connect buttons
            Button startButton = FindButtonByName(mainMenuPanel, "StartGameButton");
            Button settingsButton = FindButtonByName(mainMenuPanel, "SettingsButton");
            Button exitButton = FindButtonByName(mainMenuPanel, "ExitGameButton");
            
            if (startButton != null) controller.startGameButton = startButton;
            if (settingsButton != null) controller.settingsButton = settingsButton;
            if (exitButton != null) controller.exitGameButton = exitButton;
        }
        
        /// <summary>
        /// Finds a button by name in the hierarchy
        /// </summary>
        /// <param name="parent">Parent object to search in</param>
        /// <param name="name">Name of the button to find</param>
        /// <returns>Button component if found, null otherwise</returns>
        private Button FindButtonByName(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                return child.GetComponent<Button>();
            }
            return null;
        }
        
        /// <summary>
        /// Creates the main menu panel with all UI elements
        /// </summary>
        /// <param name="canvasObject">Parent canvas object</param>
        /// <returns>Main menu panel object</returns>
        private GameObject CreateMainMenuPanel(GameObject canvasObject)
        {
            // Create main menu panel
            GameObject mainMenuPanel = new GameObject("MainMenuPanel");
            RectTransform mainMenuRect = mainMenuPanel.AddComponent<RectTransform>();
            mainMenuRect.SetParent(canvasObject.transform, false);
            mainMenuRect.anchorMin = Vector2.zero;
            mainMenuRect.anchorMax = Vector2.one;
            mainMenuRect.offsetMin = Vector2.zero;
            mainMenuRect.offsetMax = Vector2.zero;
            
            // Create game title
            GameObject titleObject = new GameObject("GameTitle");
            RectTransform titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.SetParent(mainMenuPanel.transform, false);
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(600, 100);
            
            Text titleText = titleObject.AddComponent<Text>();
            titleText.text = "GOMOKU";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            
            // Create start game button
            GameObject startButtonObject = new GameObject("StartGameButton");
            RectTransform startButtonRect = startButtonObject.AddComponent<RectTransform>();
            startButtonRect.SetParent(mainMenuPanel.transform, false);
            startButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            startButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            startButtonRect.anchoredPosition = new Vector2(0, 100);
            startButtonRect.sizeDelta = new Vector2(300, 60);
            
            Image startButtonImage = startButtonObject.AddComponent<Image>();
            startButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Text startButtonText = startButtonObject.AddComponent<Text>();
            startButtonText.text = "Start Game";
            startButtonText.fontSize = 24;
            startButtonText.alignment = TextAnchor.MiddleCenter;
            startButtonText.color = Color.white;
            
            Button startButton = startButtonObject.AddComponent<Button>();
            startButton.transition = Selectable.Transition.ColorTint;
            ColorBlock startColors = startButton.colors;
            startColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            startColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            startButton.colors = startColors;
            
            // Create settings button
            GameObject settingsButtonObject = new GameObject("SettingsButton");
            RectTransform settingsButtonRect = settingsButtonObject.AddComponent<RectTransform>();
            settingsButtonRect.SetParent(mainMenuPanel.transform, false);
            settingsButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            settingsButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            settingsButtonRect.anchoredPosition = new Vector2(0, 20);
            settingsButtonRect.sizeDelta = new Vector2(300, 60);
            
            Image settingsButtonImage = settingsButtonObject.AddComponent<Image>();
            settingsButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Text settingsButtonText = settingsButtonObject.AddComponent<Text>();
            settingsButtonText.text = "Settings";
            settingsButtonText.fontSize = 24;
            settingsButtonText.alignment = TextAnchor.MiddleCenter;
            settingsButtonText.color = Color.white;
            
            Button settingsButton = settingsButtonObject.AddComponent<Button>();
            settingsButton.transition = Selectable.Transition.ColorTint;
            ColorBlock settingsColors = settingsButton.colors;
            settingsColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            settingsColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            settingsButton.colors = settingsColors;
            
            // Create exit button
            GameObject exitButtonObject = new GameObject("ExitGameButton");
            RectTransform exitButtonRect = exitButtonObject.AddComponent<RectTransform>();
            exitButtonRect.SetParent(mainMenuPanel.transform, false);
            exitButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            exitButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            exitButtonRect.anchoredPosition = new Vector2(0, -60);
            exitButtonRect.sizeDelta = new Vector2(300, 60);
            
            Image exitButtonImage = exitButtonObject.AddComponent<Image>();
            exitButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Text exitButtonText = exitButtonObject.AddComponent<Text>();
            exitButtonText.text = "Exit Game";
            exitButtonText.fontSize = 24;
            exitButtonText.alignment = TextAnchor.MiddleCenter;
            exitButtonText.color = Color.white;
            
            Button exitButton = exitButtonObject.AddComponent<Button>();
            exitButton.transition = Selectable.Transition.ColorTint;
            ColorBlock exitColors = exitButton.colors;
            exitColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            exitColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            exitButton.colors = exitColors;
            
            // Create settings panel
            CreateSettingsPanel(canvasObject);
            
            return mainMenuPanel;
        }
        
        /// <summary>
        /// Creates the settings panel with all UI elements
        /// </summary>
        /// <param name="canvasObject">Parent canvas object</param>
        private void CreateSettingsPanel(GameObject canvasObject)
        {
            // Create settings panel
            GameObject settingsPanel = new GameObject("SettingsPanel");
            RectTransform settingsRect = settingsPanel.AddComponent<RectTransform>();
            settingsRect.SetParent(canvasObject.transform, false);
            settingsRect.anchorMin = Vector2.zero;
            settingsRect.anchorMax = Vector2.one;
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;
            
            // Set inactive by default
            settingsPanel.SetActive(false);
            
            // Create settings title
            GameObject titleObject = new GameObject("SettingsTitle");
            RectTransform titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.SetParent(settingsPanel.transform, false);
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(600, 100);
            
            Text titleText = titleObject.AddComponent<Text>();
            titleText.text = "SETTINGS";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            
            // Create board size label
            GameObject boardSizeLabel = new GameObject("BoardSizeLabel");
            RectTransform boardSizeLabelRect = boardSizeLabel.AddComponent<RectTransform>();
            boardSizeLabelRect.SetParent(settingsPanel.transform, false);
            boardSizeLabelRect.anchorMin = new Vector2(0.3f, 0.6f);
            boardSizeLabelRect.anchorMax = new Vector2(0.3f, 0.6f);
            boardSizeLabelRect.anchoredPosition = Vector2.zero;
            boardSizeLabelRect.sizeDelta = new Vector2(300, 60);
            
            Text boardSizeLabelText = boardSizeLabel.AddComponent<Text>();
            boardSizeLabelText.text = "Board Size";
            boardSizeLabelText.fontSize = 24;
            boardSizeLabelText.alignment = TextAnchor.MiddleLeft;
            boardSizeLabelText.color = Color.white;
            
            // Create win condition label
            GameObject winConditionLabel = new GameObject("WinConditionLabel");
            RectTransform winConditionLabelRect = winConditionLabel.AddComponent<RectTransform>();
            winConditionLabelRect.SetParent(settingsPanel.transform, false);
            winConditionLabelRect.anchorMin = new Vector2(0.3f, 0.5f);
            winConditionLabelRect.anchorMax = new Vector2(0.3f, 0.5f);
            winConditionLabelRect.anchoredPosition = Vector2.zero;
            winConditionLabelRect.sizeDelta = new Vector2(300, 60);
            
            Text winConditionLabelText = winConditionLabel.AddComponent<Text>();
            winConditionLabelText.text = "Win Condition";
            winConditionLabelText.fontSize = 24;
            winConditionLabelText.alignment = TextAnchor.MiddleLeft;
            winConditionLabelText.color = Color.white;
            
            // Create save button
            GameObject saveButtonObject = new GameObject("SaveButton");
            RectTransform saveButtonRect = saveButtonObject.AddComponent<RectTransform>();
            saveButtonRect.SetParent(settingsPanel.transform, false);
            saveButtonRect.anchorMin = new Vector2(0.5f, 0.3f);
            saveButtonRect.anchorMax = new Vector2(0.5f, 0.3f);
            saveButtonRect.anchoredPosition = new Vector2(-150, 0);
            saveButtonRect.sizeDelta = new Vector2(200, 60);
            
            Image saveButtonImage = saveButtonObject.AddComponent<Image>();
            saveButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Text saveButtonText = saveButtonObject.AddComponent<Text>();
            saveButtonText.text = "Save";
            saveButtonText.fontSize = 24;
            saveButtonText.alignment = TextAnchor.MiddleCenter;
            saveButtonText.color = Color.white;
            
            Button saveButton = saveButtonObject.AddComponent<Button>();
            saveButton.transition = Selectable.Transition.ColorTint;
            ColorBlock saveColors = saveButton.colors;
            saveColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            saveColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            saveButton.colors = saveColors;
            
            // Create cancel button
            GameObject cancelButtonObject = new GameObject("CancelButton");
            RectTransform cancelButtonRect = cancelButtonObject.AddComponent<RectTransform>();
            cancelButtonRect.SetParent(settingsPanel.transform, false);
            cancelButtonRect.anchorMin = new Vector2(0.5f, 0.3f);
            cancelButtonRect.anchorMax = new Vector2(0.5f, 0.3f);
            cancelButtonRect.anchoredPosition = new Vector2(150, 0);
            cancelButtonRect.sizeDelta = new Vector2(200, 60);
            
            Image cancelButtonImage = cancelButtonObject.AddComponent<Image>();
            cancelButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Text cancelButtonText = cancelButtonObject.AddComponent<Text>();
            cancelButtonText.text = "Cancel";
            cancelButtonText.fontSize = 24;
            cancelButtonText.alignment = TextAnchor.MiddleCenter;
            cancelButtonText.color = Color.white;
            
            Button cancelButton = cancelButtonObject.AddComponent<Button>();
            cancelButton.transition = Selectable.Transition.ColorTint;
            ColorBlock cancelColors = cancelButton.colors;
            cancelColors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            cancelColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            cancelButton.colors = cancelColors;
        }
    }
}