using UnityEngine;
using UnityEngine.Events;

namespace Gomoku
{
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;

        [Header("Messages")]
        [SerializeField] private string blackTurnMessage = "黑棋回合";
        [SerializeField] private string whiteTurnMessage = "白棋回合";
        [SerializeField] private string blackWinMessage = "黑棋获胜！";
        [SerializeField] private string whiteWinMessage = "白棋获胜！";
        [SerializeField] private string drawMessage = "平局！";

        [Header("Dialog Settings")]
        [SerializeField] private Color dialogBgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        [SerializeField] private Color winTextColor = Color.yellow;
        [SerializeField] private int dialogWidth = 400;
        [SerializeField] private int dialogHeight = 200;

        public UnityEvent<string> OnStatusChanged;
        public UnityEvent<GameState> OnGameEnded;

        private bool _showDialog = false;
        private string _dialogMessage = "";
        private GameState _currentGameState;
        private AIDifficulty _currentAIDifficulty = AIDifficulty.Simple;
        private AIDifficulty _blackAIDifficulty = AIDifficulty.Simple;
        private AIDifficulty _whiteAIDifficulty = AIDifficulty.Hard;
        private string _currentTurnMessage = "黑棋回合";

        private static readonly string[] DifficultyNames = { "简单", "中等", "困难" };
        private static readonly AIDifficulty[] Difficulties = { AIDifficulty.Simple, AIDifficulty.Medium, AIDifficulty.Hard };

        private void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();

            if (gameManager != null)
            {
                gameManager.OnTurnChanged.AddListener(OnTurnChanged);
                gameManager.OnGameEnded.AddListener(OnGameEndedInternal);
            }
        }

        private void OnTurnChanged(PieceType currentPlayer)
        {
            _currentTurnMessage = currentPlayer == PieceType.Black ? blackTurnMessage : whiteTurnMessage;
            OnStatusChanged?.Invoke(_currentTurnMessage);
        }

        private void OnGameEndedInternal(GameState state)
        {
            _currentGameState = state;
            _dialogMessage = state switch
            {
                GameState.BlackWin => blackWinMessage,
                GameState.WhiteWin => whiteWinMessage,
                GameState.Draw => drawMessage,
                _ => ""
            };

            _showDialog = true;
            OnStatusChanged?.Invoke(_dialogMessage);
            OnGameEnded?.Invoke(state);
        }

        private void OnGUI()
        {
            if (!_showDialog)
            {
                GUI.Label(new Rect(10, 10, 150, 30), _currentTurnMessage);

                DrawModeButtons();

                if (gameManager != null && gameManager.GameMode == GameMode.PvAI)
                {
                    DrawAIDifficultyButtons();
                }

                if (gameManager != null && gameManager.GameMode == GameMode.AIvsAI)
                {
                    DrawSelfPlayConfig();
                    DrawSelfPlayControls();
                }

                DrawUndoButton();
            }

            if (_showDialog)
            {
                DrawWinDialog();
            }
        }

        private void DrawModeButtons()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontSize = 13 };
            float y = 10;
            GameMode current = gameManager != null ? gameManager.GameMode : GameMode.PvAI;

            string[] labels = { "人机", "双人", "AI对弈" };
            GameMode[] modes = { GameMode.PvAI, GameMode.PvP, GameMode.AIvsAI };

            for (int i = 0; i < 3; i++)
            {
                bool isActive = current == modes[i];
                string label = isActive ? $"[{labels[i]}]" : labels[i];

                if (GUI.Button(new Rect(160 + i * 65, y, 60, 25), label, style))
                {
                    if (gameManager != null)
                    {
                        gameManager.SetGameMode(modes[i]);
                        gameManager.StartNewGame();
                        _showDialog = false;
                    }
                }
            }
        }

        private void DrawAIDifficultyButtons()
        {
            float y = 40;
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontSize = 14 };

            for (int i = 0; i < 3; i++)
            {
                string label = _currentAIDifficulty == Difficulties[i]
                    ? $"● {DifficultyNames[i]}"
                    : $"  {DifficultyNames[i]}";

                if (GUI.Button(new Rect(10 + i * 75, y, 70, 25), label, style))
                {
                    _currentAIDifficulty = Difficulties[i];
                    gameManager.SetAIDifficulty(Difficulties[i]);
                    gameManager.StartNewGame();
                    _showDialog = false;
                }
            }
        }

        private void DrawSelfPlayConfig()
        {
            float y = 40;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };

            // 黑方 AI
            GUI.Label(new Rect(10, y, 40, 22), "黑方", labelStyle);
            for (int i = 0; i < 3; i++)
            {
                string label = _blackAIDifficulty == Difficulties[i]
                    ? $"●{DifficultyNames[i]}"
                    : DifficultyNames[i];

                if (GUI.Button(new Rect(50 + i * 60, y, 55, 22), label, btnStyle))
                {
                    _blackAIDifficulty = Difficulties[i];
                    gameManager.SetBlackAIDifficulty(Difficulties[i]);
                }
            }

            y += 25;

            // 白方 AI
            GUI.Label(new Rect(10, y, 40, 22), "白方", labelStyle);
            for (int i = 0; i < 3; i++)
            {
                string label = _whiteAIDifficulty == Difficulties[i]
                    ? $"●{DifficultyNames[i]}"
                    : DifficultyNames[i];

                if (GUI.Button(new Rect(50 + i * 60, y, 55, 22), label, btnStyle))
                {
                    _whiteAIDifficulty = Difficulties[i];
                    gameManager.SetWhiteAIDifficulty(Difficulties[i]);
                }
            }

            y += 28;

            // 开始按钮
            if (GUI.Button(new Rect(10, y, 120, 25), "开始自对弈", new GUIStyle(GUI.skin.button) { fontSize = 14 }))
            {
                gameManager.SetBlackAIDifficulty(_blackAIDifficulty);
                gameManager.SetWhiteAIDifficulty(_whiteAIDifficulty);
                gameManager.StartNewGame();
                _showDialog = false;
            }
        }

        private void DrawSelfPlayControls()
        {
            if (gameManager == null || !gameManager.IsSelfPlaying) return;

            float y = 125;
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontSize = 13 };

            string pauseLabel = gameManager.IsSelfPlayPaused ? "继续" : "暂停";
            if (GUI.Button(new Rect(10, y, 60, 25), pauseLabel, style))
            {
                if (gameManager.IsSelfPlayPaused)
                    gameManager.ResumeSelfPlay();
                else
                    gameManager.PauseSelfPlay();
            }

            if (GUI.Button(new Rect(75, y, 60, 25), "重置", style))
            {
                gameManager.StartNewGame();
                _showDialog = false;
            }
        }

        private void DrawUndoButton()
        {
            bool canUndo = gameManager != null && gameManager.CanUndo;
            GUI.enabled = canUndo;
            GUIStyle style = new GUIStyle(GUI.skin.button) { fontSize = 14 };

            if (GUI.Button(new Rect(240, 40, 70, 25), "悔棋", style))
            {
                gameManager.Undo();
            }
            GUI.enabled = true;
        }

        private void DrawWinDialog()
        {
            bool isAIvsAI = gameManager != null && gameManager.GameMode == GameMode.AIvsAI;
            int height = isAIvsAI ? 260 : dialogHeight;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            Rect dialogRect = new Rect(
                centerX - dialogWidth / 2f,
                centerY - height / 2f,
                dialogWidth,
                height
            );

            GUI.Box(dialogRect, "");
            GUI.color = dialogBgColor;
            GUI.DrawTexture(dialogRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle borderStyle = new GUIStyle { normal = { background = Texture2D.whiteTexture, textColor = Color.white } };
            GUI.Box(dialogRect, "", borderStyle);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = winTextColor }
            };
            GUI.Label(new Rect(dialogRect.x, dialogRect.y + 20, dialogRect.width, 40), "游戏结束", titleStyle);

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(dialogRect.x, dialogRect.y + 60, dialogRect.width, 35), _dialogMessage, messageStyle);

            // AIvsAI 模式显示统计信息
            if (isAIvsAI && gameManager != null)
            {
                var stats = gameManager.GetGameStats();
                GUIStyle statsStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                };

                string statsText = $"黑方: {stats.BlackAIName}  vs  白方: {stats.WhiteAIName}\n"
                                 + $"总步数: {stats.TotalMoves}  耗时: {stats.ElapsedSeconds:F1}s";
                GUI.Label(new Rect(dialogRect.x, dialogRect.y + 100, dialogRect.width, 50), statsText, statsStyle);
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            if (GUI.Button(new Rect(centerX - 80, dialogRect.y + height - 55, 160, 40), "再来一局", buttonStyle))
            {
                RestartGame();
                _showDialog = false;
            }
        }

        public void RestartGame()
        {
            if (gameManager != null)
            {
                gameManager.StartNewGame();
                BoardView boardView = FindObjectOfType<BoardView>();
                if (boardView != null)
                    boardView.ClearBoard();
            }
        }

        public void SetPvPMode()
        {
            if (gameManager != null)
            {
                gameManager.SetGameMode(GameMode.PvP);
                gameManager.StartNewGame();
                _showDialog = false;
            }
        }

        public void SetPvAIMode()
        {
            if (gameManager != null)
            {
                gameManager.SetGameMode(GameMode.PvAI);
                gameManager.StartNewGame();
                _showDialog = false;
            }
        }
    }
}
