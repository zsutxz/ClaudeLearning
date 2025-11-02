#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CoinAnimation.Editor
{
    /// <summary>
    /// CoinAnimationManager 编辑器扩展
    /// 提供预制体自动检测和配置功能
    /// </summary>
    [CustomEditor(typeof(CoinAnimation.Animation.CoinAnimationManager))]
    public class CoinAnimationManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty useObjectPoolingProp;
        private SerializedProperty poolConfigurationProp;
        private SerializedProperty coinPrefabProp;
        private SerializedProperty maxConcurrentCoinsProp;

        private bool showPrefabDetection = true;
        private bool showPoolConfiguration = true;

        private void OnEnable()
        {
            useObjectPoolingProp = serializedObject.FindProperty("useObjectPooling");
            poolConfigurationProp = serializedObject.FindProperty("poolConfiguration");
            coinPrefabProp = serializedObject.FindProperty("coinPrefab");
            maxConcurrentCoinsProp = serializedObject.FindProperty("maxConcurrentCoins");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var manager = target as CoinAnimation.Animation.CoinAnimationManager;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Coin Animation Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Enhanced with Auto Prefab Detection", EditorStyles.miniLabel);

            EditorGUILayout.Space(5);

            // 对象池配置
            showPoolConfiguration = EditorGUILayout.Foldout(showPoolConfiguration, "Pool Configuration", true);
            if (showPoolConfiguration)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(useObjectPoolingProp);

                if (useObjectPoolingProp.boolValue)
                {
                    EditorGUILayout.PropertyField(poolConfigurationProp);
                    EditorGUILayout.PropertyField(coinPrefabProp);

                    // 预制体检测状态
                    DrawPrefabDetectionStatus(manager);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);

            // 性能设置
            EditorGUILayout.LabelField("Performance Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(maxConcurrentCoinsProp);

            // 显示当前状态
            EditorGUILayout.LabelField($"Active Coins: {manager.ActiveCoinCount}");
            EditorGUILayout.LabelField($"Is At Capacity: {manager.IsAtCapacity}");
            EditorGUILayout.LabelField($"Pool Initialized: {manager.IsPoolInitialized}");
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);

            // 测试按钮
            EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (GUILayout.Button("Test Prefab Detection"))
            {
                TestPrefabDetection(manager);
            }

            if (GUILayout.Button("Test Pool"))
            {
                TestPool(manager);
            }

            if (GUILayout.Button("Test Animation Session"))
            {
                TestAnimationSession(manager);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);

            // 快速修复按钮
            if (coinPrefabProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No coin prefab assigned! Click 'Auto-Find Prefab' to automatically detect coin prefabs.", MessageType.Warning);

                if (GUILayout.Button("Auto-Find Prefab"))
                {
                    AutoFindPrefab(manager);
                }
            }

            if (EditorApplication.isPlaying && useObjectPoolingProp.boolValue && !manager.IsPoolInitialized)
            {
                EditorGUILayout.HelpBox("Object pooling is enabled but pool is not initialized. Check console for errors.", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPrefabDetectionStatus(CoinAnimation.Animation.CoinAnimationManager manager)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Prefab Detection Status:", EditorStyles.boldLabel);

            if (coinPrefabProp.objectReferenceValue != null)
            {
                var prefab = coinPrefabProp.objectReferenceValue as GameObject;
                EditorGUILayout.LabelField($"✅ Current Prefab: {prefab.name}");

                // 检查预制体是否包含动画控制器
                if (HasAnyCoinAnimationController(prefab))
                {
                    EditorGUILayout.LabelField("✅ Has Coin Animation Controller", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⚠️ No Coin Animation Controller Found", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("⚠️ No Prefab Assigned", EditorStyles.miniLabel);
            }

            // 显示可用的预制体选项
            var availablePrefabs = FindAvailableCoinPrefabs();
            if (availablePrefabs.Count > 0)
            {
                EditorGUILayout.LabelField("Available Prefabs:", EditorStyles.miniLabel);
                foreach (var prefab in availablePrefabs)
                {
                    if (GUILayout.Button($"Use: {prefab.name}", EditorStyles.miniButton))
                    {
                        coinPrefabProp.objectReferenceValue = prefab;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        private bool HasAnyCoinAnimationController(GameObject prefab)
        {
            if (prefab == null) return false;

            var controllers = prefab.GetComponentsInChildren<MonoBehaviour>();
            foreach (var controller in controllers)
            {
                if (controller != null)
                {
                    string typeName = controller.GetType().Name;
                    if (typeName.Contains("CoinAnimation") && typeName.Contains("Controller"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private System.Collections.Generic.List<GameObject> FindAvailableCoinPrefabs()
        {
            var prefabs = new System.Collections.Generic.List<GameObject>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab UGUICoin");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && HasAnyCoinAnimationController(prefab))
                {
                    prefabs.Add(prefab);
                }
            }

            return prefabs;
        }

        private void AutoFindPrefab(CoinAnimation.Animation.CoinAnimationManager manager)
        {
            var availablePrefabs = FindAvailableCoinPrefabs();
            if (availablePrefabs.Count > 0)
            {
                coinPrefabProp.objectReferenceValue = availablePrefabs[0];
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"[CoinAnimationManager] Auto-assigned prefab: {availablePrefabs[0].name}");
            }
            else
            {
                EditorUtility.DisplayDialog("No Prefabs Found",
                    "Could not find any coin prefabs with animation controllers. Please create or assign a coin prefab manually.",
                    "OK");
            }
        }

        private void TestPrefabDetection(CoinAnimation.Animation.CoinAnimationManager manager)
        {
            Debug.Log("[CoinAnimationManager] Testing prefab detection...");

            var availablePrefabs = FindAvailableCoinPrefabs();
            Debug.Log($"Found {availablePrefabs.Count} available coin prefabs");

            foreach (var prefab in availablePrefabs)
            {
                Debug.Log($"  - {prefab.name}");
            }
        }

        private void TestPool(CoinAnimation.Animation.CoinAnimationManager manager)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required",
                    "Pool testing requires the game to be in Play mode.", "OK");
                return;
            }

            Debug.Log("[CoinAnimationManager] Testing pool...");

            if (manager.IsPoolInitialized)
            {
                var coin = manager.GetCoinFromPool();
                if (coin != null)
                {
                    Debug.Log($"✅ Successfully retrieved coin: {coin.name}");
                    manager.ReturnCoinToPool(coin);
                    Debug.Log("✅ Successfully returned coin to pool");
                }
                else
                {
                    Debug.LogError("❌ Failed to get coin from pool");
                }
            }
            else
            {
                Debug.LogError("❌ Pool is not initialized");
            }
        }

        private void TestAnimationSession(CoinAnimation.Animation.CoinAnimationManager manager)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required",
                    "Animation testing requires the game to be in Play mode.", "OK");
                return;
            }

            Debug.Log("[CoinAnimationManager] Testing animation session...");

            // 创建测试目标
            GameObject target = new GameObject("TestTarget");
            target.transform.position = Vector3.zero;

            try
            {
                var sessionId = manager.StartCoinAnimation(target.transform, 3);
                Debug.Log($"✅ Animation session started: {sessionId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Animation session failed: {e.Message}");
            }
            finally
            {
                DestroyImmediate(target);
            }
        }
    }
}
#endif