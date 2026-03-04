#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Gomoku.Editor
{
    /// <summary>
    /// 五子棋场景快速设置工具
    /// 使用方法：菜单 Tools > Gomoku > Setup Scene
    /// </summary>
    public static class GomokuSceneSetup
    {
        private const string CELL_PREFAB_PATH = "Assets/Prefabs/Cell.prefab";
        private const string PIECE_PREFAB_PATH = "Assets/Prefabs/Piece.prefab";

        [MenuItem("Tools/Gomoku/Setup Scene", false, 1)]
        public static void SetupScene()
        {
            // 创建必要目录
            CreateDirectories();

            // 创建游戏配置
            CreateGameConfig();

            // 创建摄像机
            SetupCamera();

            // 创建光照
            SetupLighting();

            // 创建 GameManager
            CreateGameManager();

            // 创建音效管理器
            CreateAudioManager();

            Debug.Log("场景基础设置完成！请继续创建 Cell 和 Piece Prefab。");
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Audio"))
                AssetDatabase.CreateFolder("Assets", "Audio");
        }

        private static void CreateGameConfig()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/GameConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(config, "Assets/GameConfig.asset");
                AssetDatabase.SaveAssets();
                Debug.Log("已创建 GameConfig.asset");
            }
        }

        private static void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                cameraObj.tag = "MainCamera";
                mainCamera = cameraObj.AddComponent<Camera>();
            }

            mainCamera.transform.position = new Vector3(0, 15, -8);
            mainCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
            mainCamera.orthographic = false;
            mainCamera.fieldOfView = 60;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
        }

        private static void SetupLighting()
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            bool hasDirectionalLight = false;
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    hasDirectionalLight = true;
                    break;
                }
            }

            if (!hasDirectionalLight)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                light.color = Color.white;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
        }

        private static void CreateGameManager()
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            GameManager gameManager = gameManagerObj.AddComponent<GameManager>();

            // 创建 Board 子对象
            GameObject boardObj = new GameObject("Board");
            boardObj.transform.parent = gameManagerObj.transform;

            BoardView boardView = boardObj.AddComponent<BoardView>();

            // 创建棋盘底座
            GameObject boardBase = GameObject.CreatePrimitive(PrimitiveType.Quad);
            boardBase.name = "BoardBase";
            boardBase.transform.parent = boardObj.transform;
            boardBase.transform.localRotation = Quaternion.Euler(90, 0, 0);
            boardBase.transform.localScale = new Vector3(16f, 16f, 1f);
            boardBase.transform.localPosition = Vector3.down * 0.01f;

            // 设置引用
            SerializedObject boardSo = new SerializedObject(boardView);
            boardSo.FindProperty("cellSize").floatValue = 1f;
            boardSo.FindProperty("pieceScale").floatValue = 0.4f;
            boardSo.FindProperty("boardRoot").objectReferenceValue = boardObj.transform;
            boardSo.ApplyModifiedProperties();

            // 关联 GameManager 和 BoardView
            SerializedObject gmSo = new SerializedObject(gameManager);
            gmSo.FindProperty("boardView").objectReferenceValue = boardView;
            gmSo.ApplyModifiedProperties();
        }

        private static void CreateAudioManager()
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        [MenuItem("Tools/Gomoku/Create Cell Prefab", false, 2)]
        public static void CreateCellPrefab()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cellObj.name = "Cell";
            cellObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            cellObj.transform.localScale = Vector3.one * 0.95f;

            cellObj.AddComponent<CellView>();

            // 创建预制体
            string localPath = CELL_PREFAB_PATH;
            PrefabUtility.SaveAsPrefabAsset(cellObj, localPath);
            Object.DestroyImmediate(cellObj);

            Debug.Log("已创建 Cell.prefab 在 Assets/Prefabs/ 目录");
        }

        [MenuItem("Tools/Gomoku/Create Piece Prefab", false, 3)]
        public static void CreatePiecePrefab()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            GameObject pieceObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pieceObj.name = "Piece";
            pieceObj.transform.localScale = Vector3.one * 0.4f;

            pieceObj.AddComponent<PieceAnimation>();

            // 创建预制体
            string localPath = PIECE_PREFAB_PATH;
            PrefabUtility.SaveAsPrefabAsset(pieceObj, localPath);
            Object.DestroyImmediate(pieceObj);

            Debug.Log("已创建 Piece.prefab 在 Assets/Prefabs/ 目录");
        }

        [MenuItem("Tools/Gomoku/Create Materials", false, 4)]
        public static void CreateMaterials()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");

            // 黑棋材质
            Material blackMat = new Material(Shader.Find("Standard"));
            blackMat.color = new Color(0.1f, 0.1f, 0.1f);
            blackMat.SetFloat("_Metallic", 0.3f);
            blackMat.SetFloat("_Glossiness", 0.8f);
            AssetDatabase.CreateAsset(blackMat, "Assets/Materials/BlackPiece.mat");

            // 白棋材质
            Material whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.color = new Color(0.95f, 0.95f, 0.95f);
            whiteMat.SetFloat("_Metallic", 0.1f);
            whiteMat.SetFloat("_Glossiness", 0.6f);
            AssetDatabase.CreateAsset(whiteMat, "Assets/Materials/WhitePiece.mat");

            // 棋盘材质
            Material boardMat = new Material(Shader.Find("Standard"));
            boardMat.color = new Color(0.9f, 0.8f, 0.6f);
            boardMat.SetFloat("_Metallic", 0f);
            boardMat.SetFloat("_Glossiness", 0.3f);
            AssetDatabase.CreateAsset(boardMat, "Assets/Materials/Board.mat");

            AssetDatabase.SaveAssets();

            Debug.Log("已创建材质：BlackPiece.mat, WhitePiece.mat, Board.mat");
        }
    }
}
#endif
