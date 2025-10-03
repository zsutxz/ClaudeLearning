using UnityEngine;
using GomokuGame.Core;
namespace GomokuGame.UI {\r\n\npublic class BoardViewManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int boardSize = 15;
    public float cellSize = 1.0f;
    public Material lineMaterial;
    public Material blackPieceMaterial;
    public Material whitePieceMaterial;\r\n\r\n    // Shared materials cache\r\n    private Material sharedLineMaterial;\r\n    private Material sharedBlackPieceMaterial;\r\n    private Material sharedWhitePieceMaterial;\r\n\r\n    [Header("Board Visualization")]
    public GameObject boardContainer;
    public GomokuGame.Core.BoardManager coreBoardManager;
    private GameObject[,] pieceVisuals;\r\n        private GameObject intersectionPrototype;\r\n        private Stack<GameObject> intersectionPool = new Stack<GameObject>();

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
        
        // Initialize piece visuals array
        pieceVisuals = new GameObject[boardSize, boardSize];
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
        }
    }

    /// <summary>
    /// Public method to initialize the board
    /// </summary>
    public void InitializeBoard()
    {
        if (!isBoardCreated)
        {
            CreateBoard();
            CenterBoard();
            AdjustScaleForResolution();
            isBoardCreated = true;
        }
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
        CreateIntersectionPoints();\r\n            // Prepare shared materials (ensure cache initialized)\r\n            if (sharedLineMaterial == null && lineMaterial != null) sharedLineMaterial = lineMaterial;\r\n            if (sharedBlackPieceMaterial == null && blackPieceMaterial != null) sharedBlackPieceMaterial = blackPieceMaterial;\r\n            if (sharedWhitePieceMaterial == null && whitePieceMaterial != null) sharedWhitePieceMaterial = whitePieceMaterial;
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
        lineRenderer.sharedMaterial = sharedLineMaterial ? sharedLineMaterial : (sharedLineMaterial = (lineMaterial ? lineMaterial : new Material(Shader.Find("Standard"))));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    /// <summary>
    /// Creates intersection points to make them clearly visible
    /// </summary>
    void CreateIntersectionPoints()\r\n    {\r\n        float boardHalfSize = (boardSize - 1) * cellSize * 0.5f;\r\n\r\n        // Ensure prototype exists\r\n        if (intersectionPrototype == null)\r\n        {\r\n            intersectionPrototype = GameObject.CreatePrimitive(PrimitiveType.Sphere);\r\n            intersectionPrototype.transform.localScale = Vector3.one * 0.1f;\r\n            var r = intersectionPrototype.GetComponent<Renderer>();\r\n            if (r != null) r.sharedMaterial = sharedLineMaterial ? sharedLineMaterial : CreateDefaultMaterial(Color.black);\r\n            Destroy(intersectionPrototype.GetComponent<Collider>());\r\r\n            // hide prototype in editor runtime\r\n            intersectionPrototype.SetActive(false);\r\n            intersectionPrototype.name = "IntersectionPrototype";\r\n            intersectionPrototype.transform.SetParent(boardContainer.transform);\r\n        }\r\n\r\n        // Create or reuse intersection points\r\n        for (int x = 0; x < boardSize; x++)\r\n        {\r\n            for (int z = 0; z < boardSize; z++)\r\n            {\r\n                GameObject pointObject = null;\r\n                if (intersectionPool.Count > 0) pointObject = intersectionPool.Pop();\r\n                else pointObject = Instantiate(intersectionPrototype);\r\n\r\n                float hx = -boardHalfSize + x * cellSize;\r\n                float hz = -boardHalfSize + z * cellSize;\r\n                pointObject.transform.localPosition = new Vector3(hx, 0.01f, hz);\r\n                pointObject.name = $"Intersection_{x}_{z}";\r\n                pointObject.SetActive(true);\r\n            }\r\n        }\r\n    }
        }
    }

    /// <summary>
    /// Creates a single intersection point
    /// </summary>
    /// <param name="position">Position of the intersection point</param>
    /// <param name="name">Name of the intersection point GameObject</param>
    void CreateIntersectionPoint(Vector3 position, string name)
    {
        GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointObject.name = name;
        pointObject.transform.SetParent(boardContainer.transform);
        pointObject.transform.position = position;
        pointObject.transform.localScale = Vector3.one * 0.1f;
        
        // Set the material to be visible
        Renderer renderer = pointObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.black;
        }
        
        // Remove collider to avoid physics interactions
        Destroy(pointObject.GetComponent<Collider>());
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
        
        // Create the piece visual
        GameObject pieceObject = null;\r\n            if (boardPiecePrefab != null) { pieceObject = Instantiate(boardPiecePrefab); } else { pieceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); }
        pieceObject.name = $"Piece_{x}_{y}";
        pieceObject.transform.SetParent(boardContainer.transform);
        pieceObject.transform.position = position;
        pieceObject.transform.localScale = Vector3.one * cellSize * 0.8f;
        
        // Set the material based on player
        Renderer renderer = pieceObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (player == GameManager.Player.Black) { renderer.sharedMaterial = sharedBlackPieceMaterial ? sharedBlackPieceMaterial : (sharedBlackPieceMaterial = (blackPieceMaterial ? blackPieceMaterial : CreateDefaultMaterial(Color.black))); } else { renderer.sharedMaterial = sharedWhitePieceMaterial ? sharedWhitePieceMaterial : (sharedWhitePieceMaterial = (whitePieceMaterial ? whitePieceMaterial : CreateDefaultMaterial(Color.white))); }
        }
        
        // Remove collider to avoid physics interactions
        Destroy(pieceObject.GetComponent<Collider>());
        
        // Store reference to the piece visual
        pieceVisuals[x, y] = pieceObject;
    }
    
    /// <summary>
    /// Creates a default material with the specified color
    /// </summary>
    /// <param name="color">Color for the material</param>
    /// <returns>New material with the specified color</returns>
    private Material CreateDefaultMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }
}





}





