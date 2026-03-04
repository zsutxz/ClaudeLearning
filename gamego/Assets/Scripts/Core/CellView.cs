using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 棋盘格子视图 - 处理单个格子的显示和交互
    /// </summary>
    public class CellView : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = new Color(0.9f, 0.8f, 0.6f);  // 木质色
        [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.7f);
        [SerializeField] private Color lastMoveColor = new Color(1f, 0.85f, 0.5f);
        [SerializeField] private Color winningColor = new Color(1f, 0.6f, 0.4f);

        private int _x;
        private int _y;
        private BoardView _boardView;
        private PieceType _piece = PieceType.None;
        private GameObject _pieceObject;
        private Renderer _renderer;
        private bool _isHovered;
        private bool _isLastMove;
        private bool _isWinning;

        public int X => _x;
        public int Y => _y;
        public PieceType Piece => _piece;
        public bool IsEmpty => _piece == PieceType.None;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        /// <summary>
        /// 初始化格子
        /// </summary>
        public void Initialize(int x, int y, BoardView boardView)
        {
            _x = x;
            _y = y;
            _boardView = boardView;
            _piece = PieceType.None;

            UpdateVisual();
        }

        /// <summary>
        /// 设置棋子
        /// </summary>
        public void SetPiece(PieceType piece, GameObject pieceObject)
        {
            _piece = piece;
            _pieceObject = pieceObject;
        }

        /// <summary>
        /// 清除棋子
        /// </summary>
        public void ClearPiece()
        {
            _piece = PieceType.None;
            if (_pieceObject != null)
            {
                Destroy(_pieceObject);
                _pieceObject = null;
            }
            _isLastMove = false;
            _isWinning = false;
            UpdateVisual();
        }

        /// <summary>
        /// 设置是否为最后落子位置
        /// </summary>
        public void SetLastMove(bool isLast)
        {
            _isLastMove = isLast;
            UpdateVisual();
        }

        /// <summary>
        /// 设置是否为获胜连线
        /// </summary>
        public void SetWinning(bool isWinning)
        {
            _isWinning = isWinning;
            UpdateVisual();
        }

        /// <summary>
        /// 更新视觉显示
        /// </summary>
        private void UpdateVisual()
        {
            if (_renderer == null) return;

            if (_isWinning)
            {
                _renderer.material.color = winningColor;
            }
            else if (_isLastMove)
            {
                _renderer.material.color = lastMoveColor;
            }
            else if (_isHovered)
            {
                _renderer.material.color = hoverColor;
            }
            else
            {
                _renderer.material.color = normalColor;
            }
        }

        private void OnMouseEnter()
        {
            if (!IsEmpty) return;
            _isHovered = true;
            UpdateVisual();
        }

        private void OnMouseExit()
        {
            _isHovered = false;
            UpdateVisual();
        }

        private void OnMouseDown()
        {
            if (!IsEmpty) return;
            _boardView.OnCellClicked(_x, _y);
        }
    }
}
