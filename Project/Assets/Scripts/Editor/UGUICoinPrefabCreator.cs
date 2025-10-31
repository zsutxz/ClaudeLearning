using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace CoinAnimation.Editor
{
    /// <summary>
    /// Editor utility for creating standardized UGUI coin prefabs
    /// Follows the specifications in CLAUDE.md for Story 1.2 - Task 5
    /// </summary>
    public static class UGUICoinPrefabCreator
    {
        [MenuItem("Coin Animation/Create UGUI Coin Prefab", priority = 1)]
        public static void CreateUGUICoinPrefab()
        {
            // Check if Canvas exists, if not create one with Screen Space - Overlay
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add CanvasScaler for proper UI scaling
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Add GraphicRaycaster for UI interaction
                canvasObject.AddComponent<GraphicRaycaster>();

                Debug.Log("Created new Canvas with Screen Space - Overlay rendering mode");
            }
            else
            {
                Debug.Log("Using existing Canvas in the scene");
            }

            // Create coin GameObject as child of Canvas
            GameObject coinObject = new GameObject("UGUICoin");
            coinObject.transform.SetParent(canvas.transform, false);

            // Add Image component for visual representation
            Image image = coinObject.AddComponent<Image>();

            // Try to find a coin sprite in the project
            Sprite coinSprite = FindCoinSprite();
            if (coinSprite != null)
            {
                image.sprite = coinSprite;
                // Set native size based on sprite
                image.SetNativeSize();
                Debug.Log("Assigned coin sprite from project");
            }
            else
            {
                // Create a default circle sprite if no coin sprite is found
                Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {
                        float distance = Vector2.Distance(
                            new Vector2(x, y),
                            new Vector2(texture.width / 2, texture.height / 2)
                        );
                        if (distance < texture.width / 2 - 2)
                        {
                            texture.SetPixel(x, y, new Color(1f, 0.8f, 0f)); // Gold color
                        }
                        else if (distance < texture.width / 2)
                        {
                            texture.SetPixel(x, y, Color.black);
                        }
                        else
                        {
                            texture.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                texture.Apply();

                Sprite defaultSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                // Make the texture readable so it can be used in builds
                string assetPath = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(assetPath))
                {
                    assetPath = "Assets/Sprites/DefaultCoinSprite.png";
                    // Ensure directory exists
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(assetPath));
                    System.IO.File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                    AssetDatabase.ImportAsset(assetPath);
                    assetPath = AssetDatabase.GetAssetPath(texture); // Get the actual asset path after import
                }

                image.sprite = defaultSprite;
                image.SetNativeSize();
                Debug.Log("Created and assigned default coin sprite");
            }

            // Add UGUICoinAnimationController component
            CoinAnimation.Animation.UGUICoinAnimationController controller =
                coinObject.AddComponent<CoinAnimation.Animation.UGUICoinAnimationController>();

            // Configure animation settings
            controller.animationSpeed = 1f;
            controller.rotationSpeed = 360f;

            // Set up RectTransform
            RectTransform rectTransform = coinObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(100, 100); // Default size

            // Create the prefab
            string prefabPath = "Assets/Prefabs/UI/UGUICoin.prefab";
            PrefabUtility.SaveAsPrefabAsset(coinObject, prefabPath);

            // Clean up the temporary instance
            Object.DestroyImmediate(coinObject);

            Debug.Log($"UGUI Coin Prefab created at {prefabPath}");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Find a coin sprite in the project
        /// </summary>
        private static Sprite FindCoinSprite()
        {
            string[] guids = AssetDatabase.FindAssets("coin t:sprite", new[] { "Assets" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
                if (obj != null)
                {
                    return (Sprite)obj;
                }
            }

            return null;
        }
    }
}