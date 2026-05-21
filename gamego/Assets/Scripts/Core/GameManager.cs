using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gomoku
{
    /// <summary>
    /// 游戏管理器 - 控制游戏流程
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Game Settings")]
        [SerializeField] private GameMode gameMode = GameMode.PvAI;
        [SerializeField] private bool aiFirst = false;
        [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Simple;

        [Header("References")]
        [SerializeField] private BoardView boardView;

        // 游戏状态
        private Board _board;
        private PieceType _currentPlayer;
        private GameState _gameState;
        private IAIPlayer _aiPlayer;
        private Stack<(int x, int y, PieceType piece)> _moveHistory;

        // 事件
        public UnityEvent<PieceType> OnTurnChanged;
        public UnityEvent<GameState> OnGameEnded;
        public UnityEvent<int, int, PieceType> OnPiecePlaced;
        public UnityEvent OnGameReset;  // 游戏重置事件

        // 属性
        public PieceType CurrentPlayer => _currentPlayer;
        public GameState GameState => _gameState;
        public Board Board => _board;
        public GameMode GameMode => gameMode;
        public AIDifficulty AIDifficulty => aiDifficulty;
        public bool CanUndo => _moveHistory != null && _moveHistory.Count > 0 && _gameState == GameState.Playing;

        private void Awake()
        {
            _board = new Board();
            ApplyGameConfig();
        }

        private void ApplyGameConfig()
        {
            if (gameConfig == null) return;
            gameMode = gameConfig.defaultGameMode;
            aiFirst = gameConfig.aiFirst;
            aiDifficulty = gameConfig.defaultAIDifficulty;
        }

        private void Start()
        {
            StartNewGame();
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            _board.Reset();
            _currentPlayer = PieceType.Black;
            _gameState = GameState.Playing;
            _moveHistory = new Stack<(int x, int y, PieceType piece)>();

            // 初始化 AI
            if (gameMode == GameMode.PvAI)
            {
                _aiPlayer = aiDifficulty switch
                {
                    AIDifficulty.Medium => new MinimaxAI(1),
                    AIDifficulty.Hard => new MinimaxAI(3),
                    _ => new SimpleAI()
                };
            }

            // 触发游戏重置事件（清空棋盘显示）
            OnGameReset?.Invoke();

            OnTurnChanged?.Invoke(_currentPlayer);

            // 如果 AI 先手
            if (gameMode == GameMode.PvAI && aiFirst)
            {
                MakeAIMove();
            }
        }

        /// <summary>
        /// 尝试在指定位置落子
        /// </summary>
        public bool TryPlacePiece(int x, int y)
        {
            // 检查游戏是否结束
            if (_gameState != GameState.Playing)
                return false;

            // 检查是否是玩家回合（人机模式下）
            if (gameMode == GameMode.PvAI && _currentPlayer == GetAIPiece())
                return false;

            // 尝试落子
            if (!_board.PlacePiece(x, y, _currentPlayer))
                return false;

            _moveHistory.Push((x, y, _currentPlayer));
            OnPiecePlaced?.Invoke(x, y, _currentPlayer);

            _gameState = WinChecker.CheckGameState(_board, x, y);

            if (_gameState != GameState.Playing)
            {
                OnGameEnded?.Invoke(_gameState);
                return true;
            }

            // 切换玩家
            SwitchPlayer();

            // AI 回合
            if (gameMode == GameMode.PvAI && _currentPlayer == GetAIPiece())
            {
                MakeAIMove();
            }

            return true;
        }

        /// <summary>
        /// 切换当前玩家
        /// </summary>
        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PieceType.Black ? PieceType.White : PieceType.Black;
            OnTurnChanged?.Invoke(_currentPlayer);
        }

        /// <summary>
        /// 获取 AI 的棋子颜色
        /// </summary>
        private PieceType GetAIPiece()
        {
            return aiFirst ? PieceType.Black : PieceType.White;
        }

        /// <summary>
        /// AI 落子
        /// </summary>
        private void MakeAIMove()
        {
            if (_aiPlayer == null) return;

            var (x, y) = _aiPlayer.GetMove(_board, GetAIPiece());

            if (_board.PlacePiece(x, y, _currentPlayer))
            {
                _moveHistory.Push((x, y, _currentPlayer));
                OnPiecePlaced?.Invoke(x, y, _currentPlayer);

                _gameState = WinChecker.CheckGameState(_board, x, y);

                if (_gameState != GameState.Playing)
                {
                    OnGameEnded?.Invoke(_gameState);
                    return;
                }

                SwitchPlayer();
            }
        }

        /// <summary>
        /// 设置游戏模式
        /// </summary>
        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;
        }

        /// <summary>
        /// 设置 AI 是否先手
        /// </summary>
        public void SetAIFirst(bool first)
        {
            aiFirst = first;
        }

        public void SetAIDifficulty(AIDifficulty difficulty)
        {
            aiDifficulty = difficulty;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;

            int steps = gameMode == GameMode.PvAI ? 2 : 1;

            for (int i = 0; i < steps && _moveHistory.Count > 0; i++)
            {
                var (x, y, _) = _moveHistory.Pop();
                _board.RemovePiece(x, y);
                boardView?.RemovePieceAt(x, y);
            }

            _currentPlayer = _moveHistory.Count > 0
                ? (_moveHistory.Peek().piece == PieceType.Black ? PieceType.White : PieceType.Black)
                : PieceType.Black;

            OnTurnChanged?.Invoke(_currentPlayer);

            boardView?.RefreshLastMoveMarker(_moveHistory.Count > 0
                ? (_moveHistory.Peek().x, _moveHistory.Peek().y)
                : (-1, -1));

            return true;
        }
    }
}
