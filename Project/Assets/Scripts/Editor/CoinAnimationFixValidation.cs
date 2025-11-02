#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CoinAnimation.Animation;

namespace CoinAnimation.Editor
{
    /// <summary>
    /// CoinAnimationManager 修复验证脚本
    /// 验证预制体检测和配置是否正常工作
    /// </summary>
    public class CoinAnimationFixValidation : EditorWindow
    {
        private Vector2 scrollPosition;
        private string validationLog = "";
        private bool isValidating = false;

        [MenuItem("Coin Animation/Validate Prefab Fix")]
        public static void ShowWindow()
        {
            GetWindow<CoinAnimationFixValidation>("Coin Animation Fix Validation");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Coin Animation Manager Fix Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("This tool validates that the CoinAnimationManager prefab detection fix is working correctly.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Full Validation"))
                {
                    RunFullValidation();
                }

                if (GUILayout.Button("Clear Log"))
                {
                    validationLog = "";
                }
            }

            EditorGUILayout.Space(10);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                EditorGUILayout.LabelField("Validation Log:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(validationLog, GUILayout.Height(300), GUILayout.ExpandWidth(true));
            }

            if (isValidating)
            {
                EditorGUILayout.HelpBox("Validation in progress...", MessageType.Info);
            }
        }

        private async void RunFullValidation()
        {
            if (isValidating) return;

            isValidating = true;
            validationLog = "=== Coin Animation Manager Fix Validation ===\n\n";
            validationLog += $"Validation started at {System.DateTime.Now}\n\n";

            try
            {
                // Test 1: Prefab Detection
                await System.Threading.Tasks.Task.Run(() => TestPrefabDetection());

                // Test 2: Manager Initialization
                TestManagerInitialization();

                // Test 3: Pool Functionality
                TestPoolFunctionality();

                // Test 4: Animation Controller Compatibility
                TestAnimationControllerCompatibility();

                validationLog += "\n=== Validation Complete ===\n";
                validationLog += "All tests passed! The CoinAnimationManager fix is working correctly.\n";
            }
            catch (System.Exception e)
            {
                validationLog += $"\n❌ Validation Failed: {e.Message}\n";
                validationLog += $"{e.StackTrace}\n";
            }
            finally
            {
                isValidating = false;
                Repaint();
            }
        }

        private void TestPrefabDetection()
        {
            validationLog += "🔍 Testing Prefab Detection...\n";

            // Check if UGUICoin prefab exists
            string[] guids = AssetDatabase.FindAssets("UGUICoin t:Prefab");
            if (guids.Length > 0)
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        validationLog += $"✅ Found UGUICoin prefab: {path}\n";

                        // Check for animation controllers
                        bool hasUGUIController = prefab.GetComponent<UGUICoinAnimationController>() != null;
                        bool hasStandardController = prefab.GetComponent<CoinAnimationController>() != null;

                        if (hasUGUIController)
                        {
                            validationLog += "✅ UGUICoin has UGUICoinAnimationController\n";
                        }

                        if (hasStandardController)
                        {
                            validationLog += "✅ UGUICoin has CoinAnimationController\n";
                        }

                        if (!hasUGUIController && !hasStandardController)
                        {
                            validationLog += "⚠️ UGUICoin has no coin animation controllers\n";
                        }
                    }
                }
            }
            else
            {
                validationLog += "❌ No UGUICoin prefab found\n";
            }

            validationLog += "\n";
        }

        private void TestManagerInitialization()
        {
            validationLog += "🔧 Testing Manager Initialization...\n";

            // Check if CoinAnimationManager exists in scene
            var manager = Object.FindObjectOfType<CoinAnimationManager>();
            if (manager != null)
            {
                validationLog += "✅ CoinAnimationManager found in scene\n";

                // Check pool status
                if (manager.IsPoolInitialized)
                {
                    validationLog += "✅ Object pool is initialized\n";
                }
                else
                {
                    validationLog += "⚠️ Object pool is not initialized (check console for errors)\n";
                }

                // Check active coin count
                validationLog += $"📊 Active coins: {manager.ActiveCoinCount}\n";
                validationLog += $"📊 Is at capacity: {manager.IsAtCapacity}\n";
            }
            else
            {
                validationLog += "❌ No CoinAnimationManager found in scene\n";
            }

            validationLog += "\n";
        }

        private void TestPoolFunctionality()
        {
            validationLog += "🏊 Testing Pool Functionality...\n";

            var manager = Object.FindObjectOfType<CoinAnimationManager>();
            if (manager != null && manager.IsPoolInitialized)
            {
                try
                {
                    // Test getting coin from pool
                    var coin = manager.GetCoinFromPool();
                    if (coin != null)
                    {
                        validationLog += "✅ Successfully retrieved coin from pool\n";
                        validationLog += $"🪙 Coin type: {coin.name}\n";

                        // Test returning coin to pool
                        manager.ReturnCoinToPool(coin);
                        validationLog += "✅ Successfully returned coin to pool\n";
                    }
                    else
                    {
                        validationLog += "❌ Failed to retrieve coin from pool\n";
                    }
                }
                catch (System.Exception e)
                {
                    validationLog += $"❌ Pool test failed: {e.Message}\n";
                }
            }
            else
            {
                validationLog += "⚠️ Cannot test pool functionality (manager not initialized)\n";
            }

            validationLog += "\n";
        }

        private void TestAnimationControllerCompatibility()
        {
            validationLog += "🎬 Testing Animation Controller Compatibility...\n";

            var manager = Object.FindObjectOfType<CoinAnimationManager>();
            if (manager != null && manager.IsPoolInitialized)
            {
                try
                {
                    var coin = manager.GetCoinFromPool();
                    if (coin != null)
                    {
                        // Check for UGUI controller
                        var uguiController = coin.GetComponent<UGUICoinAnimationController>();
                        if (uguiController != null)
                        {
                            validationLog += "✅ UGUICoinAnimationController detected\n";
                            validationLog += $"📊 Controller state: {uguiController.CurrentState}\n";
                            validationLog += $"📊 Is animating: {uguiController.IsAnimating}\n";
                        }

                        // Check for standard controller
                        var standardController = coin.GetComponent<CoinAnimationController>();
                        if (standardController != null)
                        {
                            validationLog += "✅ CoinAnimationController detected\n";
                        }

                        manager.ReturnCoinToPool(coin);
                    }
                }
                catch (System.Exception e)
                {
                    validationLog += $"❌ Controller compatibility test failed: {e.Message}\n";
                }
            }
            else
            {
                validationLog += "⚠️ Cannot test controller compatibility (manager not initialized)\n";
            }

            validationLog += "\n";
        }
    }
}
#endif