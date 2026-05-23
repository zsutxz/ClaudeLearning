using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace Gomoku
{
    public struct GameStats
    {
        public int TotalMoves;
        public float ElapsedSeconds;
        public string BlackAIName;
        public string WhiteAIName;
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Game Settings")]
        [SerializeField] private GameMode gameMode = GameMode.PvAI;
        [SerializeField] private bool aiFirst = false;
        [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Simple;

        [Header("AI vs AI Settings")]
        [SerializeField] private AIDifficulty blackAIDifficulty = AIDifficulty.Simple;
        [SerializeField] private AIDifficulty whiteAIDifficulty = AIDifficulty.Hard;
        [SerializeField] private float selfPlayStepDelay = 0.5f;

        [Header("References")]
        [SerializeField] private BoardView boardView;

        // 游戏状态
        private Board _board;
        private PieceType _currentPlayer;
        private GameState _gameState;
        private IAIPlayer _aiPlayer;
        private IAIPlayer _blackAI;
        private IAIPlayer _whiteAI;
        private Stack<(int x, int y, PieceType piece)> _moveHistory;
        private Coroutine _selfPlayCoroutine;
        private bool _selfPlayPaused;
        private Stopwatch _gameStopwatch;
        private int _totalMoveCount;

        // 事件
        public UnityEvent<PieceType> OnTurnChanged;
        public UnityEvent<GameState> OnGameEnded;
        public UnityEvent<int, int, PieceType> OnPiecePlaced;
        public UnityEvent OnGameReset;

        // 属性
        public PieceType CurrentPlayer => _currentPlayer;
        public GameState GameState => _gameState;
        public Board Board => _board;
        public GameMode GameMode => gameMode;
        public AIDifficulty AIDifficulty => aiDifficulty;
        public AIDifficulty BlackAIDifficulty => blackAIDifficulty;
        public AIDifficulty WhiteAIDifficulty => whiteAIDifficulty;
        public bool CanUndo => _moveHistory != null && _moveHistory.Count > 0 && _gameState == GameState.Playing;
        public bool IsSelfPlaying => gameMode == GameMode.AIvsAI && _selfPlayCoroutine != null;
        public bool IsSelfPlayPaused => _selfPlayPaused;
        public int TotalMoves => _totalMoveCount;
        public float ElapsedSeconds => _gameStopwatch?.IsRunning == true
            ? (float)_gameStopwatch.Elapsed.TotalSeconds : 0f;

        public GameStats GetGameStats()
        {
            return new GameStats
            {
                TotalMoves = _totalMoveCount,
                ElapsedSeconds = _gameStopwatch != null ? (float)_gameStopwatch.Elapsed.TotalSeconds : 0f,
                BlackAIName = DifficultyName(blackAIDifficulty),
                WhiteAIName = DifficultyName(whiteAIDifficulty)
            };
        }

        private static string DifficultyName(AIDifficulty d) => d switch
        {
            AIDifficulty.Medium => "Minimax(中)",
            AIDifficulty.Hard => "Minimax(难)",
            _ => "SimpleAI"
        };

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

        public void StartNewGame()
        {
            // 停止之前的自对弈协程
            if (_selfPlayCoroutine != null)
            {
                StopCoroutine(_selfPlayCoroutine);
                _selfPlayCoroutine = null;
            }
            _selfPlayPaused = false;

            _board.Reset();
            _currentPlayer = PieceType.Black;
            _gameState = GameState.Playing;
            _moveHistory = new Stack<(int x, int y, PieceType piece)>();
            _totalMoveCount = 0;
            _gameStopwatch = Stopwatch.StartNew();

            if (gameMode == GameMode.PvAI)
            {
                _aiPlayer = CreateAI(aiDifficulty);
            }
            else if (gameMode == GameMode.AIvsAI)
            {
                _blackAI = CreateAI(blackAIDifficulty);
                _whiteAI = CreateAI(whiteAIDifficulty);
            }

            OnGameReset?.Invoke();
            OnTurnChanged?.Invoke(_currentPlayer);

            if (gameMode == GameMode.PvAI && aiFirst)
            {
                MakeAIMove();
            }
            else if (gameMode == GameMode.AIvsAI)
            {
                _selfPlayCoroutine = StartCoroutine(RunAIvsAI());
            }
        }

        private IAIPlayer CreateAI(AIDifficulty difficulty)
        {
            return difficulty switch
            {
                AIDifficulty.Medium => new MinimaxAI(1),
                AIDifficulty.Hard => new MinimaxAI(6, useIterativeDeepening: true, timeLimitMs: 2000),
                _ => new SimpleAI()
            };
        }

        public bool TryPlacePiece(int x, int y)
        {
            if (_gameState != GameState.Playing)
                return false;

            if (gameMode == GameMode.PvAI && _currentPlayer == GetAIPiece())
                return false;

            if (gameMode == GameMode.AIvsAI)
                return false;

            if (!_board.PlacePiece(x, y, _currentPlayer))
                return false;

            _moveHistory.Push((x, y, _currentPlayer));
            _totalMoveCount++;
            OnPiecePlaced?.Invoke(x, y, _currentPlayer);

            _gameState = WinChecker.CheckGameState(_board, x, y);

            if (_gameState != GameState.Playing)
            {
                _gameStopwatch?.Stop();
                OnGameEnded?.Invoke(_gameState);
                return true;
            }

            SwitchPlayer();

            if (gameMode == GameMode.PvAI && _currentPlayer == GetAIPiece())
            {
                MakeAIMove();
            }

            return true;
        }

        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PieceType.Black ? PieceType.White : PieceType.Black;
            OnTurnChanged?.Invoke(_currentPlayer);
        }

        private PieceType GetAIPiece()
        {
            return aiFirst ? PieceType.Black : PieceType.White;
        }

        private void MakeAIMove()
        {
            if (_aiPlayer == null) return;

            var (x, y) = _aiPlayer.GetMove(_board, GetAIPiece());

            if (_board.PlacePiece(x, y, _currentPlayer))
            {
                _moveHistory.Push((x, y, _currentPlayer));
                _totalMoveCount++;
                OnPiecePlaced?.Invoke(x, y, _currentPlayer);

                _gameState = WinChecker.CheckGameState(_board, x, y);

                if (_gameState != GameState.Playing)
                {
                    _gameStopwatch?.Stop();
                    OnGameEnded?.Invoke(_gameState);
                    return;
                }

                SwitchPlayer();
            }
        }

        private IEnumerator RunAIvsAI()
        {
            while (_gameState == GameState.Playing)
            {
                while (_selfPlayPaused)
                    yield return null;

                yield return new WaitForSeconds(selfPlayStepDelay);

                if (_gameState != GameState.Playing) break;

                IAIPlayer currentAI = _currentPlayer == PieceType.Black ? _blackAI : _whiteAI;
                var (x, y) = currentAI.GetMove(_board, _currentPlayer);

                if (_board.PlacePiece(x, y, _currentPlayer))
                {
                    _moveHistory.Push((x, y, _currentPlayer));
                    _totalMoveCount++;
                    OnPiecePlaced?.Invoke(x, y, _currentPlayer);

                    _gameState = WinChecker.CheckGameState(_board, x, y);

                    if (_gameState != GameState.Playing)
                    {
                        _gameStopwatch?.Stop();
                        OnGameEnded?.Invoke(_gameState);
                        break;
                    }

                    SwitchPlayer();
                }
            }

            _selfPlayCoroutine = null;
            _gameStopwatch?.Stop();
        }

        public void PauseSelfPlay()
        {
            _selfPlayPaused = true;
        }

        public void ResumeSelfPlay()
        {
            _selfPlayPaused = false;
        }

        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;
        }

        public void SetAIFirst(bool first)
        {
            aiFirst = first;
        }

        public void SetAIDifficulty(AIDifficulty difficulty)
        {
            aiDifficulty = difficulty;
        }

        public void SetBlackAIDifficulty(AIDifficulty difficulty)
        {
            blackAIDifficulty = difficulty;
        }

        public void SetWhiteAIDifficulty(AIDifficulty difficulty)
        {
            whiteAIDifficulty = difficulty;
        }

        public void SetSelfPlayStepDelay(float delay)
        {
            selfPlayStepDelay = delay;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            if (gameMode == GameMode.AIvsAI) return false;

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
