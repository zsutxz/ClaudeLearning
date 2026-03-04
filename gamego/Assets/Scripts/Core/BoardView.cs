using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 棋盘视图 - 负责棋盘的渲染和交互
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float pieceScale = 0.4f;

        [Header("Visual References")]
        [SerializeField] private Material blackPieceMaterial;
        [SerializeField] private Material whitePieceMaterial;
        [SerializeField] private Material boardMaterial;
        [SerializeField] private Transform boardRoot;

        [Header("Prefabs")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject piecePrefab;

        [Header("References")]
        [SerializeField] private GameManager gameManager;

        private CellView[,] _cells;
        private (int x, int y)? _lastMove;

        private void Awake()
        {
            // 自动查找资源
            AutoFindResources();
        }

        private void Start()
        {
            // 再次确保 GameManager 引用正确
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (gameManager == null)
            {
                Debug.LogError("BoardView: 未找到 GameManager！请确保场景中有 GameManager 组件。");
                return;
            }

            InitializeBoard();

            // 订阅 GameManager 事件
            gameManager.OnPiecePlaced.AddListener(OnPiecePlaced);
            gameManager.OnGameEnded.AddListener(OnGameEnded);
            gameManager.OnGameReset.AddListener(OnGameReset);

            Debug.Log("BoardView 初始化完成，已订阅 GameManager 事件");
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (gameManager != null)
            {
                gameManager.OnPiecePlaced.RemoveListener(OnPiecePlaced);
                gameManager.OnGameEnded.RemoveListener(OnGameEnded);
                gameManager.OnGameReset.RemoveListener(OnGameReset);
            }
        }

        /// <summary>
        /// 棋子放置事件处理
        /// </summary>
        private void OnPiecePlaced(int x, int y, PieceType piece)
        {
            ShowPiece(x, y, piece);
        }

        /// <summary>
        /// 游戏结束事件处理
        /// </summary>
        private void OnGameEnded(GameState state)
        {
            // 如果有获胜者，高亮显示获胜连线
            if (state == GameState.BlackWin || state == GameState.WhiteWin)
            {
                // 查找最后落子位置并获取获胜连线
                if (_lastMove.HasValue)
                {
                    if (WinChecker.TryGetWinningLine(gameManager.Board, _lastMove.Value.x, _lastMove.Value.y, out var positions))
                    {
                        HighlightWinningLine(positions);
                    }
                }
            }
        }

        /// <summary>
        /// 游戏重置事件处理
        /// </summary>
        private void OnGameReset()
        {
            ClearBoard();
        }

        /// <summary>
        /// 自动查找需要的资源
        /// </summary>
        private void AutoFindResources()
        {
            // 查找 GameManager
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            // 查找 BoardRoot
            if (boardRoot == null)
            {
                boardRoot = transform;
            }

            // 查找 Prefabs
            if (cellPrefab == null)
            {
                cellPrefab = Resources.Load<GameObject>("Cell");
                if (cellPrefab == null)
                {
                    // 尝试从 Assets/Prefabs 加载
                    cellPrefab = LoadPrefabFromAssets("Assets/Prefabs/Cell.prefab");
                }
            }

            if (piecePrefab == null)
            {
                piecePrefab = Resources.Load<GameObject>("Piece");
                if (piecePrefab == null)
                {
                    piecePrefab = LoadPrefabFromAssets("Assets/Prefabs/Piece.prefab");
                }
            }

            // 查找材质
            if (blackPieceMaterial == null)
            {
                blackPieceMaterial = LoadMaterialFromAssets("Assets/Materials/BlackPiece.mat");
            }

            if (whitePieceMaterial == null)
            {
                whitePieceMaterial = LoadMaterialFromAssets("Assets/Materials/WhitePiece.mat");
            }

            // 如果还是没有 Prefab，动态创建
            if (cellPrefab == null)
            {
                cellPrefab = CreateCellPrefab();
                Debug.Log("动态创建了 Cell Prefab");
            }

            if (piecePrefab == null)
            {
                piecePrefab = CreatePiecePrefab();
                Debug.Log("动态创建了 Piece Prefab");
            }
        }

#if UNITY_EDITOR
        private GameObject LoadPrefabFromAssets(string path)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private Material LoadMaterialFromAssets(string path)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
        }
#else
        private GameObject LoadPrefabFromAssets(string path) => null;
        private Material LoadMaterialFromAssets(string path) => null;
#endif

        /// <summary>
        /// 动态创建 Cell Prefab
        /// </summary>
        private GameObject CreateCellPrefab()
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.name = "Cell";
            cell.transform.rotation = Quaternion.Euler(90, 0, 0);
            cell.transform.localScale = Vector3.one * 0.95f;
            cell.AddComponent<CellView>();

            // 设置材质颜色
            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.8f, 0.6f);
            }

            return cell;
        }

        /// <summary>
        /// 动态创建 Piece Prefab
        /// </summary>
        private GameObject CreatePiecePrefab()
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.name = "Piece";
            piece.transform.localScale = Vector3.one * 0.4f;
            piece.AddComponent<PieceAnimation>();
            return piece;
        }

        /// <summary>
        /// 初始化棋盘
        /// </summary>
        public void InitializeBoard()
        {
            if (cellPrefab == null)
            {
                Debug.LogError("Cell Prefab 未找到！");
                return;
            }

            if (_cells != null)
            {
                // 清理旧的格子
                foreach (var cell in _cells)
                {
                    if (cell != null)
                        Destroy(cell.gameObject);
                }
            }

            _cells = new CellView[Board.BOARD_SIZE, Board.BOARD_SIZE];

            float offsetX = (Board.BOARD_SIZE - 1) * cellSize / 2f;
            float offsetY = (Board.BOARD_SIZE - 1) * cellSize / 2f;

            for (int x = 0; x < Board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Board.BOARD_SIZE; y++)
                {
                    Vector3 position = new Vector3(
                        x * cellSize - offsetX,
                        0,
                        y * cellSize - offsetY
                    );

                    GameObject cellObj = Instantiate(cellPrefab, boardRoot);
                    cellObj.transform.localPosition = position;
                    cellObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    cellObj.name = $"Cell_{x}_{y}";

                    CellView cellView = cellObj.GetComponent<CellView>();
                    if (cellView == null)
                    {
                        cellView = cellObj.AddComponent<CellView>();
                    }

                    cellView.Initialize(x, y, this);
                    _cells[x, y] = cellView;
                }
            }

            Debug.Log($"棋盘初始化完成: {Board.BOARD_SIZE}x{Board.BOARD_SIZE}");
        }

        /// <summary>
        /// 处理格子点击
        /// </summary>
        public void OnCellClicked(int x, int y)
        {
            if (gameManager == null)
            {
                Debug.LogWarning("BoardView: GameManager 引用丢失，无法落子");
                return;
            }
            gameManager.TryPlacePiece(x, y);
        }

        /// <summary>
        /// 在指定位置显示棋子
        /// </summary>
        public void ShowPiece(int x, int y, PieceType piece)
        {
            if (!_cells[x, y].IsEmpty) return;

            GameObject pieceObj = Instantiate(piecePrefab, _cells[x, y].transform);
            pieceObj.transform.localPosition = Vector3.up * 0.1f;
            pieceObj.transform.localScale = Vector3.one * pieceScale;

            Renderer renderer = pieceObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 使用配置的材质，如果没有则使用颜色
                if (piece == PieceType.Black)
                {
                    if (blackPieceMaterial != null)
                        renderer.material = blackPieceMaterial;
                    else
                        renderer.material.color = new Color(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    if (whitePieceMaterial != null)
                        renderer.material = whitePieceMaterial;
                    else
                        renderer.material.color = new Color(0.95f, 0.95f, 0.95f);
                }
            }

            _cells[x, y].SetPiece(piece, pieceObj);

            // 更新最后落子位置
            if (_lastMove.HasValue)
            {
                _cells[_lastMove.Value.x, _lastMove.Value.y].SetLastMove(false);
            }
            _lastMove = (x, y);
            _cells[x, y].SetLastMove(true);
        }

        /// <summary>
        /// 高亮获胜连线
        /// </summary>
        public void HighlightWinningLine(System.Collections.Generic.List<(int x, int y)> positions)
        {
            if (positions == null) return;

            foreach (var (x, y) in positions)
            {
                _cells[x, y].SetWinning(true);
            }
        }

        /// <summary>
        /// 清空棋盘显示
        /// </summary>
        public void ClearBoard()
        {
            if (_cells == null) return;

            foreach (var cell in _cells)
            {
                if (cell != null)
                {
                    cell.ClearPiece();
                }
            }
            _lastMove = null;
        }
    }
}
