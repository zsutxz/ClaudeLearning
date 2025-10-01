# Frontend Architecture

## Component Architecture

### Component Organization

```
Assets/
├── Scripts/
│   ├── Core/                 # Core game logic
│   │   ├── GameManager.cs
│   │   ├── BoardManager.cs
│   │   ├── PieceManager.cs
│   │   └── WinDetector.cs
│   ├── UI/                   # User interface components
│   │   ├── UIManager.cs
│   │   ├── MenuController.cs
│   │   ├── GameUI.cs
│   │   └── SettingsUI.cs
│   ├── Utilities/            # Helper classes
│   │   ├── EventManager.cs
│   │   └── PlayerPrefsManager.cs
│   └── Tests/                # Game tests
│       ├── UnitTests/
│       └── IntegrationTests/
├── Prefabs/                  # Reusable game objects
├── Scenes/                   # Game scenes
└── Resources/                # Game assets
```

### Component Template

```csharp
using UnityEngine;

public class ComponentTemplate : MonoBehaviour
{
    // Public variables for inspector configuration
    [SerializeField] private SomeComponent dependency;
    
    // Private variables
    private SomeData data;
    
    // Unity lifecycle methods
    private void Awake()
    {
        Initialize();
    }
    
    // Public methods for external interaction
    public void DoSomething()
    {
        // Implementation
    }
    
    // Private helper methods
    private void Initialize()
    {
        // Initialization logic
    }
}
```

## State Management Architecture

### State Structure

```csharp
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    [SerializeField] private GameState currentState;
    
    public GameState CurrentState => currentState;
    
    public event System.Action<GameState> OnGameStateChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
```

### State Management Patterns

- **Singleton Pattern:** For global state managers
- **Event-Driven Updates:** For state change notifications
- **ScriptableObject Configuration:** For static configuration data

## Routing Architecture

### Route Organization

In Unity, "routing" is managed through scenes and UI panels:

```
Scenes/
├── MainMenu.unity
├── GameScene.unity
└── SettingsScene.unity

UI Panels (within scenes):
├── MainMenuPanel
├── GamePanel
├── SettingsPanel
└── PausePanel
```

### Protected Route Pattern

```csharp
public class SceneController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        // Check if scene can be loaded based on game state
        if (CanLoadScene(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Handle unauthorized scene access
            Debug.LogWarning($"Cannot load scene {sceneName} in current state");
        }
    }
    
    private bool CanLoadScene(string sceneName)
    {
        // Implement scene access logic based on game state
        return true;
    }
}
```

## Frontend Services Layer

### API Client Setup

For a local game, there's no external API, but we have internal service classes:

```csharp
public class GameService : MonoBehaviour
{
    private static GameService _instance;
    public static GameService Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartNewGame(int boardSize)
    {
        // Implementation
    }
    
    public bool PlacePiece(int x, int y, int player)
    {
        // Implementation
        return true;
    }
    
    public int CheckWinCondition(int[,] board)
    {
        // Implementation
        return 0;
    }
}
```

### Service Example

```csharp
public class SettingsService : MonoBehaviour
{
    private const string BOARD_SIZE_KEY = "BoardSize";
    private const string THEME_KEY = "Theme";
    
    public void SaveBoardSize(int size)
    {
        PlayerPrefs.SetInt(BOARD_SIZE_KEY, size);
        PlayerPrefs.Save();
    }
    
    public int LoadBoardSize()
    {
        return PlayerPrefs.GetInt(BOARD_SIZE_KEY, 15); // Default to 15x15
    }
    
    public void SaveTheme(string theme)
    {
        PlayerPrefs.SetString(THEME_KEY, theme);
        PlayerPrefs.Save();
    }
    
    public string LoadTheme()
    {
        return PlayerPrefs.GetString(THEME_KEY, "Default");
    }
}
```
