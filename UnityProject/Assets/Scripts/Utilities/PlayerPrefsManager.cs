using UnityEngine;

namespace GomokuGame.Utilities
{
    /// <summary>
    /// Manages player preferences and settings persistence
    /// </summary>
    public static class PlayerPrefsManager
    {
        #region Constants
        private const string BOARD_SIZE_KEY = "BoardSize";
        private const string WIN_CONDITION_KEY = "WinCondition";
        private const string THEME_KEY = "Theme";
        private const string FIRST_RUN_KEY = "FirstRun";
        #endregion

        #region Default Values
        private const int DEFAULT_BOARD_SIZE = 15;
        private const int DEFAULT_WIN_CONDITION = 5;
        private const string DEFAULT_THEME = "Default";
        #endregion

        #region Board Settings
        /// <summary>
        /// Saves the board size setting
        /// </summary>
        /// <param name="size">Board size</param>
        public static void SaveBoardSize(int size)
        {
            PlayerPrefs.SetInt(BOARD_SIZE_KEY, size);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the board size setting
        /// </summary>
        /// <returns>Board size</returns>
        public static int LoadBoardSize()
        {
            return PlayerPrefs.GetInt(BOARD_SIZE_KEY, DEFAULT_BOARD_SIZE);
        }

        /// <summary>
        /// Saves the win condition setting
        /// </summary>
        /// <param name="condition">Win condition</param>
        public static void SaveWinCondition(int condition)
        {
            PlayerPrefs.SetInt(WIN_CONDITION_KEY, condition);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the win condition setting
        /// </summary>
        /// <returns>Win condition</returns>
        public static int LoadWinCondition()
        {
            return PlayerPrefs.GetInt(WIN_CONDITION_KEY, DEFAULT_WIN_CONDITION);
        }
        #endregion

        #region Theme Settings
        /// <summary>
        /// Saves the theme setting
        /// </summary>
        /// <param name="theme">Theme name</param>
        public static void SaveTheme(string theme)
        {
            PlayerPrefs.SetString(THEME_KEY, theme);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the theme setting
        /// </summary>
        /// <returns>Theme name</returns>
        public static string LoadTheme()
        {
            return PlayerPrefs.GetString(THEME_KEY, DEFAULT_THEME);
        }
        #endregion

        #region First Run
        /// <summary>
        /// Checks if this is the first run of the application
        /// </summary>
        /// <returns>True if first run, false otherwise</returns>
        public static bool IsFirstRun()
        {
            return !PlayerPrefs.HasKey(FIRST_RUN_KEY);
        }

        /// <summary>
        /// Marks the application as having been run
        /// </summary>
        public static void MarkFirstRunComplete()
        {
            PlayerPrefs.SetInt(FIRST_RUN_KEY, 1);
            PlayerPrefs.Save();
        }
        #endregion

        #region Reset
        /// <summary>
        /// Resets all player preferences to default values
        /// </summary>
        public static void ResetToDefaults()
        {
            SaveBoardSize(DEFAULT_BOARD_SIZE);
            SaveWinCondition(DEFAULT_WIN_CONDITION);
            SaveTheme(DEFAULT_THEME);
            MarkFirstRunComplete();
        }
        #endregion

        #region Error Handling & Reliability
        /// <summary>
        /// Validates that all required settings exist and have valid values
        /// </summary>
        /// <returns>True if settings are valid, false otherwise</returns>
        public static bool ValidateSettings()
        {
            try
            {
                // Check if all required keys exist
                if (!PlayerPrefs.HasKey(BOARD_SIZE_KEY) ||
                    !PlayerPrefs.HasKey(WIN_CONDITION_KEY) ||
                    !PlayerPrefs.HasKey(THEME_KEY))
                {
                    return false;
                }

                // Validate board size range
                int boardSize = LoadBoardSize();
                if (boardSize < 9 || boardSize > 19)
                {
                    return false;
                }

                // Validate win condition range
                int winCondition = LoadWinCondition();
                if (winCondition < 3 || winCondition > 10)
                {
                    return false;
                }

                // Validate theme is not empty
                string theme = LoadTheme();
                if (string.IsNullOrEmpty(theme))
                {
                    return false;
                }

                return true;
            }
            catch (System.Exception)
            {
                // If any exception occurs during validation, consider settings invalid
                return false;
            }
        }

        /// <summary>
        /// Creates a backup of current settings
        /// </summary>
        public static void CreateBackup()
        {
            try
            {
                // Store current values as backup
                PlayerPrefs.SetInt(BOARD_SIZE_KEY + "_backup", LoadBoardSize());
                PlayerPrefs.SetInt(WIN_CONDITION_KEY + "_backup", LoadWinCondition());
                PlayerPrefs.SetString(THEME_KEY + "_backup", LoadTheme());
                PlayerPrefs.Save();
            }
            catch (System.Exception)
            {
                // Silently fail - backup is optional
            }
        }

        /// <summary>
        /// Restores settings from backup if available
        /// </summary>
        /// <returns>True if restore was successful, false otherwise</returns>
        public static bool RestoreFromBackup()
        {
            try
            {
                if (PlayerPrefs.HasKey(BOARD_SIZE_KEY + "_backup") &&
                    PlayerPrefs.HasKey(WIN_CONDITION_KEY + "_backup") &&
                    PlayerPrefs.HasKey(THEME_KEY + "_backup"))
                {
                    SaveBoardSize(PlayerPrefs.GetInt(BOARD_SIZE_KEY + "_backup"));
                    SaveWinCondition(PlayerPrefs.GetInt(WIN_CONDITION_KEY + "_backup"));
                    SaveTheme(PlayerPrefs.GetString(THEME_KEY + "_backup"));
                    return true;
                }
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Safely saves all settings with error handling and backup
        /// </summary>
        public static void SafeSaveAllSettings(int boardSize, int winCondition, string theme)
        {
            try
            {
                // Create backup before making changes
                CreateBackup();

                // Save new values
                SaveBoardSize(boardSize);
                SaveWinCondition(winCondition);
                SaveTheme(theme);
            }
            catch (System.Exception)
            {
                // If save fails, attempt to restore from backup
                RestoreFromBackup();
            }
        }
        #endregion
    }
}