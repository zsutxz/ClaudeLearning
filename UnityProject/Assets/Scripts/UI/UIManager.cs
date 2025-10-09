u        #region Fields
        #region Fields
        [Header("Game UI Elements")]
        [SerializeField] private Text currentPlayerText;
        [SerializeField] private Text gameStateText;
        [SerializeField] private Text gameOverText;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject resultsScreenPanel;
        [SerializeField] private ResultsScreenController resultsScreenController;
        
        [Header("Game Controls")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        
        [Header("Main Menu Elements")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Header("Settings UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider boardSizeSlider;
        [SerializeField] private Slider winConditionSlider;
        [SerializeField] private Text boardSizeValueText;
        [SerializeField] private Text winConditionValueText;
        [SerializeField] private Button[] boardSizeButtons;
        [SerializeField] private Button boardSize9x9Button;
        [SerializeField] private Button boardSize13x13Button;
        [SerializeField] private Button boardSize15x15Button;
        [SerializeField] private Button boardSize19x19Button;
        [SerializeField] private Dropdown themeDropdown;

        [Header("Win Condition UI Elements")]
        [SerializeField] private Button winConditionStandardButton;
        [SerializeField] private Button winConditionCaptureButton;
        [SerializeField] private Button winConditionTimeBasedButton;
        [SerializeField] private Text winConditionDescriptionText;

        // Public read-only properties for external access if needed
        public Text CurrentPlayerText => currentPlayerText;
        public Text GameStateText => gameStateText;
        public Text GameOverText => gameOverText;
        public GameObject GameUIPanel => gameUIPanel;
        public GameObject GameOverPanel => gameOverPanel;
        public GameObject ResultsScreenPanel => resultsScreenPanel;
        public ResultsScreenController ResultsScreenController => resultsScreenController;
        public Button RestartButton => restartButton;
        public Button MainMenuButton => mainMenuButton;
        public GameObject MainMenuPanel => mainMenuPanel;
        public GameObject SettingsPanel => settingsPanel;
        public Slider BoardSizeSlider => boardSizeSlider;
        public Slider WinConditionSlider => winConditionSlider;
        public Text BoardSizeValueText => boardSizeValueText;
        public Text WinConditionValueText => winConditionValueText;
        public Button[] BoardSizeButtons => boardSizeButtons;
        public Button BoardSize9x9Button => boardSize9x9Button;
        public Button BoardSize13x13Button => boardSize13x13Button;
        public Button BoardSize15x15Button => boardSize15x15Button;
        public Button BoardSize19x19Button => boardSize19x19Button;
        public Dropdown ThemeDropdown => themeDropdown;
        public Button WinConditionStandardButton => winConditionStandardButton;
        public Button WinConditionCaptureButton => winConditionCaptureButton;
        public Button WinConditionTimeBasedButton => winConditionTimeBasedButton;
        public Text WinConditionDescriptionText => winConditionDescriptionText;

        // Try to auto-assign common component references when possible
        private void OnValidate()
        {
            // Runs in editor when script is changed or values reset; attempt to assign missing refs
            TryAutoAssignReferences();
        }

        private void TryAutoAssignReferences()
        {
            // Assign common UI components if they're null by finding the first instance in scene.
            AssignIfNull(ref currentPlayerText);
            AssignIfNull(ref gameStateText);
            AssignIfNull(ref gameOverText);
            AssignIfNull(ref resultsScreenController);
            AssignIfNull(ref restartButton);
            AssignIfNull(ref mainMenuButton);
            AssignIfNull(ref boardSizeSlider);
            AssignIfNull(ref winConditionSlider);
            AssignIfNull(ref boardSizeValueText);
            AssignIfNull(ref winConditionValueText);
            AssignIfNull(ref themeDropdown);
            AssignIfNull(ref winConditionDescriptionText);
        }

        private void AssignIfNull<T>(ref T field) where T : Component
        {
            if (field == null)
            {
                T found = FindObjectOfType<T>();
                if (found != null)
                {
                    field = found;
                }
            }
        }

        #endregion
