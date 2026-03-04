using UnityEngine;
using UnityEngine.Events;

namespace Gomoku
{
    /// <summary>
    /// UI 管理器 - 包含获胜弹框功能
    /// </summary>
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

        // 事件 - 可用于自定义 UI
        public UnityEvent<string> OnStatusChanged;
        public UnityEvent<GameState> OnGameEnded;

        // 弹框状态
        private bool _showDialog = false;
        private string _dialogMessage = "";
        private GameState _currentGameState;

        // 当前回合提示
        private string _currentTurnMessage = "黑棋回合";

        private void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

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
            Debug.Log(_currentTurnMessage);
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
            Debug.Log($"游戏结束: {_dialogMessage}");
        }

        private void OnGUI()
        {
            // 显示当前回合（左上角）
            if (!_showDialog)
            {
                GUI.Label(new Rect(10, 10, 150, 30), _currentTurnMessage);
            }

            // 显示获胜弹框
            if (_showDialog)
            {
                DrawWinDialog();
            }
        }

        /// <summary>
        /// 绘制获胜弹框
        /// </summary>
        private void DrawWinDialog()
        {
            // 计算居中位置
            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;
            Rect dialogRect = new Rect(
                centerX - dialogWidth / 2f,
                centerY - dialogHeight / 2f,
                dialogWidth,
                dialogHeight
            );

            // 绘制背景
            GUI.Box(dialogRect, "");

            // 绘制半透明背景
            GUI.color = dialogBgColor;
            GUI.DrawTexture(dialogRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 绘制边框
            GUIStyle borderStyle = new GUIStyle();
            borderStyle.normal.background = Texture2D.whiteTexture;
            borderStyle.normal.textColor = Color.white;
            GUI.Box(dialogRect, "", borderStyle);

            // 绘制标题
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = winTextColor }
            };
            Rect titleRect = new Rect(dialogRect.x, dialogRect.y + 30, dialogRect.width, 50);
            GUI.Label(titleRect, "游戏结束", titleStyle);

            // 绘制获胜消息
            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            Rect messageRect = new Rect(dialogRect.x, dialogRect.y + 80, dialogRect.width, 40);
            GUI.Label(messageRect, _dialogMessage, messageStyle);

            // 绘制按钮
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            Rect buttonRect = new Rect(
                centerX - 80,
                dialogRect.y + dialogHeight - 60,
                160,
                40
            );

            if (GUI.Button(buttonRect, "再来一局", buttonStyle))
            {
                RestartGame();
                _showDialog = false;
            }
        }

        /// <summary>
        /// 重新开始游戏（可由 UI 按钮调用）
        /// </summary>
        public void RestartGame()
        {
            if (gameManager != null)
            {
                gameManager.StartNewGame();
                // 同时清空棋盘显示
                BoardView boardView = FindObjectOfType<BoardView>();
                if (boardView != null)
                {
                    boardView.ClearBoard();
                }
            }
        }

        /// <summary>
        /// 设置游戏模式为本地双人
        /// </summary>
        public void SetPvPMode()
        {
            if (gameManager != null)
            {
                gameManager.SetGameMode(GameMode.PvP);
                gameManager.StartNewGame();
                _showDialog = false;
            }
        }

        /// <summary>
        /// 设置游戏模式为人机对战
        /// </summary>
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
