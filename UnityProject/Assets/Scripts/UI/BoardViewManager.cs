using UnityEngine;
using System.Collections.Generic;
using GomokuGame.Core;
using GomokuGame.Themes;

namespace GomokuGame.UI
{
    public class BoardViewManager : MonoBehaviour
    {
        [Header("Board Settings")]
        public float cellSize = 1.0f;
        public Material lineMaterial;
        public Material blackPieceMaterial;
        public Material whitePieceMaterial;
        //Shared materials cache
        private Material sharedLineMaterial;
        private Material sharedBlackPieceMaterial;
        private Material sharedWhitePieceMaterial;
        [Header("Board Visualization")]
        public GameObject boardContainer;
        public GomokuGame.Core.BoardManager coreBoardManager;

        // Property to get board size from core BoardManager
        public int boardSize => coreBoardManager != null ? coreBoardManager.BoardSize : 15;
        private GameObject[,] pieceVisuals;
        private GameObject intersectionPrototype;
        private Stack<GameObject> intersectionPool = new Stack<GameObject>();

        private Camera mainCamera;
        private bool isBoardCreated = false;
        private float lastScreenWidth;
        private float lastScreenHeight;

        void Awake()
        {
            // Ensure we don't destroy this object when loading new scenes
            DontDestroyOnLoad(gameObject);

            // Store initial screen dimensions
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            // Initialize piece visuals array (will be resized when board is created)
            pieceVisuals = new GameObject[15, 15]; // Default size, will be updated in InitializeBoard
        }

        void Update()
        {
            // Check if screen resolution has changed
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                AdjustScaleForResolution();
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }

            // Render instanced meshes for pieces if enabled
            RenderInstancedPieces();
        }

        private void RenderInstancedPieces()
        {
  
        } 

        void Start()
        {
            mainCamera = Camera.main;
            if (!isBoardCreated)
            {
                InitializeBoard();
            }

            // Subscribe to piece placement events
            if (coreBoardManager != null)
            {
                coreBoardManager.OnPiecePlaced += OnPiecePlaced;
                coreBoardManager.OnCaptureMade += OnCaptureMade;
            }
        }

        /// <summary>
        /// Public method to initialize the board
        /// </summary>
        public void InitializeBoard()
        {
            // Clear existing board if it exists
            if (boardContainer != null)
            {
                Destroy(boardContainer);
            }

            // Resize piece visuals array to match current board size
            pieceVisuals = new GameObject[boardSize, boardSize];

            // Create new board
            CreateBoard();
            CenterBoard();
            AdjustScaleForResolution();
            isBoardCreated = true;
        }

        /// <summary>
        /// Creates the visual representation of the Gomoku board
        /// </summary>
        void CreateBoard()
        {
            // Create a container for all board lines
            boardContainer = new GameObject("BoardContainer");
            boardContainer.transform.SetParent(transform);
            boardContainer.transform.localPosition = Vector3.zero;

            // Create grid lines
            CreateGridLines();
        }

        /// <summary>
        /// Creates the grid lines for the board
        /// </summary>
        void CreateGridLines()
        {
            float boardHalfSize = (boardSize - 1) * cellSize * 0.5f;

            // Create horizontal lines
            for (int i = 0; i < boardSize; i++)
            {
                CreateLine(
                    new Vector3(-boardHalfSize, 0, -boardHalfSize + i * cellSize),
                    new Vector3(boardHalfSize, 0, -boardHalfSize + i * cellSize),
                    $"HorizontalLine_{i}"
                );
            }

            // Create vertical lines
            for (int i = 0; i < boardSize; i++)
            {
                CreateLine(
                    new Vector3(-boardHalfSize + i * cellSize, 0, -boardHalfSize),
                    new Vector3(-boardHalfSize + i * cellSize, 0, boardHalfSize),
                    $"VerticalLine_{i}"
                );
            }

            // Create intersection points
            CreateIntersectionPoints();
            // Prepare shared materials (ensure cache initialized)
            // if (sharedLineMaterial == null && lineMaterial != null)
            // sharedLineMaterial = lineMaterial;
            // if (sharedBlackPieceMaterial == null && blackPieceMaterial != null) sharedBlackPieceMaterial = blackPieceMaterial;
        }

        /// <summary>
        /// Creates a single line between two points
        /// </summary>
        /// <param name="start">Start position of the line</param>
        /// <param name="end">End position of the line</param>
        /// <param name="name">Name of the line GameObject</param>
        void CreateLine(Vector3 start, Vector3 end, string name)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(boardContainer.transform);

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // Configure line renderer properties
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            
            // Apply theme colors if ThemeManager exists
            ThemeSettings themeSettings = null;
            if (ThemeManager.Instance != null)
            {
                themeSettings = ThemeManager.Instance.GetCurrentThemeSettings();
            }
            
            if (themeSettings != null && themeSettings.boardLineMaterial != null)
            {
                lineRenderer.sharedMaterial = themeSettings.boardLineMaterial;
            }
            else
            {
                lineRenderer.sharedMaterial = sharedLineMaterial ? sharedLineMaterial : (sharedLineMaterial = (lineMaterial ? lineMaterial : new Material(Shader.Find("Standard"))));
            }
            
            if (themeSettings != null)
            {
                lineRenderer.startColor = themeSettings.boardLineColor;
                lineRenderer.endColor = themeSettings.boardLineColor;
            }
            else
            {
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = Color.black;
            }
        }

        /// <summary>
        /// Creates intersection points to make them clearly visible
        /// </summary>
        void CreateIntersectionPoints()
        {
            float boardHalfSize = (boardSize - 1) * cellSize * 0.5f;
            // Ensure prototype exists
            if (intersectionPrototype == null)
            {
                intersectionPrototype = CreateIntersectionPointPrototype();
            }
        }

        /// <summary>
        /// Creates a single intersection point
        /// </summary>
        /// <param name="position">Position of the intersection point</param>
        /// <param name="name">Name of the intersection point GameObject</param>
        void CreateIntersectionPoint(Vector3 position, string name)
        {
            GameObject pointObject = Instantiate(intersectionPrototype);
            pointObject.name = name;
            pointObject.transform.SetParent(boardContainer.transform);
            pointObject.transform.position = position;
            pointObject.transform.localScale = Vector3.one * 0.1f;
        }

        /// <summary>
        /// Centers the board in the camera view
        /// </summary>
        public void CenterBoard()
        {
            if (mainCamera != null)
            {
                // Position the board at the center of the camera view
                Vector3 cameraPosition = mainCamera.transform.position;
                transform.position = new Vector3(cameraPosition.x, 0, cameraPosition.z);
            }
        }

        /// <summary>
        /// Adjusts the board scale based on screen resolution
        /// </summary>
        public void AdjustScaleForResolution()
        {
            if (mainCamera != null)
            {
                // Get screen dimensions
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                // If the camera is perspective, we don't need to adjust scale
                if (mainCamera.orthographic)
                {
                    // Simple scaling based on camera orthographic size
                    float scale = mainCamera.orthographicSize / 10.0f;
                    transform.localScale = Vector3.one * Mathf.Max(0.5f, scale);
                }
                else
                {
                    // For perspective camera, adjust based on screen aspect ratio
                    float aspectRatio = screenWidth / screenHeight;
                    float scale = Mathf.Min(screenWidth, screenHeight) / 1000f;
                    transform.localScale = Vector3.one * Mathf.Max(0.5f, scale);
                }
            }
        }

        /// <summary>
        /// Handles piece placement events from the core BoardViewManager
        /// </summary>
        /// <param name="x">X coordinate of the placed piece</param>
        /// <param name="y">Y coordinate of the placed piece</param>
        /// <param name="player">Player who placed the piece</param>
        private void OnPiecePlaced(int x, int y, GameManager.Player player)
        {
            CreatePieceVisual(x, y, player);
        }

        /// <summary>
        /// Creates a visual representation of a placed piece
        /// </summary>
        /// <param name="x">X coordinate of the piece</param>
        /// <param name="y">Y coordinate of the piece</param>
        /// <param name="player">Player who placed the piece</param>
        private void CreatePieceVisual(int x, int y, GameManager.Player player)
        {
            // Calculate world position for the piece
            float boardHalfSize = (boardSize - 1) * cellSize * 0.5f;
            Vector3 position = new Vector3(
                -boardHalfSize + x * cellSize,
                0.1f, // Slightly above the board
                -boardHalfSize + y * cellSize
            );

            // Fallback: instantiate a prefab or primitive as before
            GameObject pieceObject = null;
            pieceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pieceObject.name = $"Piece_{x}_{y}";
            pieceObject.transform.SetParent(boardContainer.transform);
            pieceObject.transform.position = position;
            pieceObject.transform.localScale = Vector3.one * cellSize * 0.8f;

            // Set the material based on player and theme
            Renderer renderer = pieceObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                ThemeSettings themeSettings = null;
                if (ThemeManager.Instance != null)
                {
                    themeSettings = ThemeManager.Instance.GetCurrentThemeSettings();
                }
                
                if (player == GameManager.Player.Black)
                {
                    if (themeSettings != null && themeSettings.blackPieceMaterial != null)
                    {
                        renderer.sharedMaterial = themeSettings.blackPieceMaterial;
                    }
                    else if (sharedBlackPieceMaterial != null || blackPieceMaterial != null)
                    {
                        renderer.sharedMaterial = sharedBlackPieceMaterial ? sharedBlackPieceMaterial : (sharedBlackPieceMaterial = blackPieceMaterial);
                    }
                    else
                    {
                        renderer.material.color = (themeSettings != null) ? themeSettings.blackPieceColor : Color.black;
                    }
                }
                else
                {
                    if (themeSettings != null && themeSettings.whitePieceMaterial != null)
                    {
                        renderer.sharedMaterial = themeSettings.whitePieceMaterial;
                    }
                    else if (sharedWhitePieceMaterial != null || whitePieceMaterial != null)
                    {
                        renderer.sharedMaterial = sharedWhitePieceMaterial ? sharedWhitePieceMaterial : (sharedWhitePieceMaterial = whitePieceMaterial);
                    }
                    else
                    {
                        renderer.material.color = (themeSettings != null) ? themeSettings.whitePieceColor : Color.white;
                    }
                }
            }

            // Remove collider to avoid physics interactions
            Destroy(pieceObject.GetComponent<Collider>());

            // Store reference to the piece visual
            if (pieceVisuals != null)
                pieceVisuals[x, y] = pieceObject;
        }

        /// <summary>
        /// Handles capture events from the core BoardManager
        /// </summary>
        /// <param name="player">Player who made the capture</param>
        /// <param name="captureCount">Number of pieces captured</param>
        private void OnCaptureMade(GameManager.Player player, int captureCount)
        {
            // When a capture occurs, we need to remove the captured pieces from the board
            // The BoardManager has already cleared the board state, so we need to sync the visuals
            SyncVisualsWithBoardState();
        }

        /// <summary>
        /// Syncs the visual pieces with the current board state
        /// </summary>
        private void SyncVisualsWithBoardState()
        {
            if (coreBoardManager == null || pieceVisuals == null) return;

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    GameManager.Player piece = coreBoardManager.GetPieceAt(x, y);

                    // If there's no piece at this position but we have a visual, remove it
                    if (piece == GameManager.Player.None && pieceVisuals[x, y] != null)
                    {
                        Destroy(pieceVisuals[x, y]);
                        pieceVisuals[x, y] = null;
                    }
                    // If there's a piece at this position but no visual, create it
                    else if (piece != GameManager.Player.None && pieceVisuals[x, y] == null)
                    {
                        CreatePieceVisual(x, y, piece);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all piece visuals from the board
        /// </summary>
        public void ClearVisuals()
        {
            if (pieceVisuals != null)
            {
                for (int x = 0; x < pieceVisuals.GetLength(0); x++)
                {
                    for (int y = 0; y < pieceVisuals.GetLength(1); y++)
                    {
                        if (pieceVisuals[x, y] != null)
                        {
                            Destroy(pieceVisuals[x, y]);
                            pieceVisuals[x, y] = null;
                        }
                    }
                }
            }

            // Clean up intersection prototype
            if (intersectionPrototype != null)
            {
                Destroy(intersectionPrototype);
                intersectionPrototype = null;
            }
        }

        /// <summary>
        /// Creates a prototype for intersection points with proper theme settings
        /// </summary>
        /// <returns>Prototype GameObject for intersection points</returns>
        private GameObject CreateIntersectionPointPrototype()
        {
            GameObject prototype = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prototype.transform.localScale = Vector3.one * 0.1f;
            
            // Set the material to be visible based on theme
            Renderer renderer = prototype.GetComponent<Renderer>();
            if (renderer != null)
            {
                ThemeSettings themeSettings = null;
                if (ThemeManager.Instance != null)
                {
                    themeSettings = ThemeManager.Instance.GetCurrentThemeSettings();
                }
                
                if (themeSettings != null && themeSettings.boardPointMaterial != null)
                {
                    renderer.sharedMaterial = themeSettings.boardPointMaterial;
                }
                else if (themeSettings != null)
                {
                    renderer.material.color = themeSettings.boardPointColor;
                }
                else
                {
                    renderer.material.color = Color.black;
                }
            }
            
            // Remove collider to avoid physics interactions
            Destroy(prototype.GetComponent<Collider>());
            
            return prototype;
        }
    }
}