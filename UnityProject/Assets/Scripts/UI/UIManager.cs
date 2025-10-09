using UnityEngine;

namespace GomokuGame.UI
{
    /// <summary>
    /// Simplified UI manager that does not reference concrete UI components.
    /// It instantiates/uses prefabs for panels and communicates with the game
    /// manager via GameObject.SendMessage to avoid direct component coupling.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Prefabs")]
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject gameUIPrefab;
        [SerializeField] private GameObject resultsPrefab;
        [SerializeField] private GameObject settingsPrefab;

        private GameObject mainMenuInstance;
        private GameObject gameUIInstance;
        private GameObject resultsInstance;
        private GameObject settingsInstance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            InstantiatePrefab(ref mainMenuInstance, mainMenuPrefab);
            InstantiatePrefab(ref gameUIInstance, gameUIPrefab);
            InstantiatePrefab(ref resultsInstance, resultsPrefab);
            InstantiatePrefab(ref settingsInstance, settingsPrefab);

            // Start with main menu active if available
            HideAllPanels();
            if (mainMenuInstance != null) mainMenuInstance.SetActive(true);
        }

        private void InstantiatePrefab(ref GameObject instance, GameObject prefab)
        {
            if (prefab == null) return;
            if (instance == null)
            {
                instance = Instantiate(prefab);
                instance.SetActive(false);
                DontDestroyOnLoad(instance);
            }
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuInstance != null) mainMenuInstance.SetActive(true);
            SendMessageToManager("OnUIShowMainMenu");
        }

        public void StartGame()
        {
            HideAllPanels();
            SendMessageToManager("StartNewGame");
            if (gameUIInstance != null) gameUIInstance.SetActive(true);
        }

        public void RestartGame()
        {
            SendMessageToManager("RestartGame");
            HideAllPanels();
            if (gameUIInstance != null) gameUIInstance.SetActive(true);
        }

        public void ReturnToMainMenu()
        {
            SendMessageToManager("ReturnToMainMenu");
            HideAllPanels();
            if (mainMenuInstance != null) mainMenuInstance.SetActive(true);
        }

        public void ShowSettings()
        {
            HideAllPanels();
            if (settingsInstance != null) settingsInstance.SetActive(true);
            SendMessageToManager("OnSettingsOpened");
        }

        public void SaveSettings()
        {
            SendMessageToManager("SaveSettingsFromUI");
            ShowMainMenu();
        }

        public void CancelSettings()
        {
            SendMessageToManager("CancelSettingsFromUI");
            ShowMainMenu();
        }

        private void HideAllPanels()
        {
            if (mainMenuInstance != null) mainMenuInstance.SetActive(false);
            if (gameUIInstance != null) gameUIInstance.SetActive(false);
            if (resultsInstance != null) resultsInstance.SetActive(false);
            if (settingsInstance != null) settingsInstance.SetActive(false);
        }

        private void SendMessageToManager(string methodName)
        {
            // Try to find a GameObject named "GameManager", then by tag "GameManager".
            GameObject manager = GameObject.Find("GameManager");
            if (manager == null)
            {
                try { manager = GameObject.FindWithTag("GameManager"); } catch { manager = null; }
            }

            if (manager != null)
            {
                manager.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning($"UIManager: Could not find GameManager to send '{methodName}'. Ensure a GameObject named or tagged 'GameManager' exists.");
            }
        }
    }
}
