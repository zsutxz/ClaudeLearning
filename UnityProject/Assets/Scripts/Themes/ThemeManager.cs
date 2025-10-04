using UnityEngine;
using System.Collections.Generic;

namespace GomokuGame.Themes
{
    public enum BoardTheme
    {
        Classic,
        Modern,
        Nature
    }

    [System.Serializable]
    public class ThemeSettings
    {
        public BoardTheme theme;
        public Color boardLineColor;
        public Color boardPointColor;
        public Color blackPieceColor;
        public Color whitePieceColor;
        public Material blackPieceMaterial;
        public Material whitePieceMaterial;
        public Material boardLineMaterial;
        public Material boardPointMaterial;
    }

    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        [Header("Theme Settings")]
        public ThemeSettings[] themes;

        [Header("Default Theme")]
        public BoardTheme defaultTheme = BoardTheme.Classic;

        private BoardTheme currentTheme;
        private ThemeSettings currentThemeSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeThemes();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeThemes()
        {
            // Ensure themes array is initialized
            if (themes == null || themes.Length == 0)
            {
                themes = new ThemeSettings[3];
            }

            // Initialize Classic theme
            if (themes[0] == null) themes[0] = new ThemeSettings();
            themes[0].theme = BoardTheme.Classic;
            themes[0].boardLineColor = Color.black;
            themes[0].boardPointColor = Color.black;
            themes[0].blackPieceColor = Color.black;
            themes[0].whitePieceColor = Color.white;

            // Load Classic theme materials
            themes[0].blackPieceMaterial = Resources.Load<Material>("Materials/Themes/Classic/Classic_Black_Mat");
            themes[0].whitePieceMaterial = Resources.Load<Material>("Materials/Themes/Classic/Classic_White_Mat");
            themes[0].boardLineMaterial = Resources.Load<Material>("Materials/Themes/Classic/Classic_Board_Mat");
            themes[0].boardPointMaterial = Resources.Load<Material>("Materials/Themes/Classic/Classic_Point_Mat");

            // Initialize Modern theme
            if (themes[1] == null) themes[1] = new ThemeSettings();
            themes[1].theme = BoardTheme.Modern;
            themes[1].boardLineColor = new Color(0.2f, 0.2f, 0.2f); // Dark gray
            themes[1].boardPointColor = new Color(0.3f, 0.3f, 0.3f); // Medium gray
            themes[1].blackPieceColor = new Color(0.1f, 0.1f, 0.1f); // Very dark gray
            themes[1].whitePieceColor = new Color(0.9f, 0.9f, 0.9f); // Light gray

            // Load Modern theme materials
            themes[1].blackPieceMaterial = Resources.Load<Material>("Materials/Themes/Modern/Modern_Black_Mat");
            themes[1].whitePieceMaterial = Resources.Load<Material>("Materials/Themes/Modern/Modern_White_Mat");
            themes[1].boardLineMaterial = Resources.Load<Material>("Materials/Themes/Modern/Modern_Board_Mat");
            themes[1].boardPointMaterial = Resources.Load<Material>("Materials/Themes/Modern/Modern_Point_Mat");

            // Initialize Nature theme
            if (themes[2] == null) themes[2] = new ThemeSettings();
            themes[2].theme = BoardTheme.Nature;
            themes[2].boardLineColor = new Color(0.4f, 0.2f, 0.1f); // Brown
            themes[2].boardPointColor = new Color(0.3f, 0.6f, 0.3f); // Green
            themes[2].blackPieceColor = new Color(0.1f, 0.1f, 0.1f); // Dark
            themes[2].whitePieceColor = new Color(0.8f, 0.7f, 0.6f); // Light brown

            // Load Nature theme materials
            themes[2].blackPieceMaterial = Resources.Load<Material>("Materials/Themes/Nature/Nature_Black_Mat");
            themes[2].whitePieceMaterial = Resources.Load<Material>("Materials/Themes/Nature/Nature_White_Mat");
            themes[2].boardLineMaterial = Resources.Load<Material>("Materials/Themes/Nature/Nature_Board_Mat");
            themes[2].boardPointMaterial = Resources.Load<Material>("Materials/Themes/Nature/Nature_Point_Mat");

            // Load saved theme or use default
            string savedTheme = PlayerPrefs.GetString("BoardTheme", defaultTheme.ToString());
            currentTheme = defaultTheme;

            // Try to parse the saved theme
            if (System.Enum.TryParse<BoardTheme>(savedTheme, out BoardTheme parsedTheme))
            {
                currentTheme = parsedTheme;
            }

            SetCurrentTheme(currentTheme);
        }

        public void SetCurrentTheme(BoardTheme theme)
        {
            currentTheme = theme;
            currentThemeSettings = GetThemeSettings(theme);
            PlayerPrefs.SetString("BoardTheme", theme.ToString());
            PlayerPrefs.Save();
        }

        public BoardTheme GetCurrentTheme()
        {
            return currentTheme;
        }

        public ThemeSettings GetCurrentThemeSettings()
        {
            return currentThemeSettings;
        }

        public ThemeSettings GetThemeSettings(BoardTheme theme)
        {
            foreach (var themeSettings in themes)
            {
                if (themeSettings != null && themeSettings.theme == theme)
                {
                    return themeSettings;
                }
            }
            
            // Return default theme if not found
            return themes[0];
        }

        public string[] GetThemeNames()
        {
            string[] names = new string[System.Enum.GetValues(typeof(BoardTheme)).Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = ((BoardTheme)i).ToString();
            }
            return names;
        }
    }
}