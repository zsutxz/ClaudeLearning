using UnityEngine;
using UnityEditor;
using System.IO;

public class ThemeMaterialGenerator : EditorWindow
{
    [MenuItem("Gomoku/Generate Theme Materials")]
    public static void ShowWindow()
    {
        GetWindow<ThemeMaterialGenerator>("Theme Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Theme Material Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate All Theme Materials"))
        {
            GenerateAllThemeMaterials();
        }
    }

    private void GenerateAllThemeMaterials()
    {
        // Create the Themes directory if it doesn't exist
        string themesPath = "Assets/Resources/Materials/Themes";
        if (!Directory.Exists(themesPath))
        {
            Directory.CreateDirectory(themesPath);
            AssetDatabase.Refresh();
        }

        // Generate materials for each theme
        GenerateClassicThemeMaterials(themesPath);
        GenerateModernThemeMaterials(themesPath);
        GenerateNatureThemeMaterials(themesPath);

        AssetDatabase.Refresh();
        Debug.Log("All theme materials generated successfully!");
    }

    private void GenerateClassicThemeMaterials(string basePath)
    {
        string themePath = Path.Combine(basePath, "Classic");
        if (!Directory.Exists(themePath))
        {
            Directory.CreateDirectory(themePath);
        }

        // Classic Black Piece Material
        Material blackPieceMat = new Material(Shader.Find("Standard"));
        blackPieceMat.color = Color.black;
        blackPieceMat.name = "Classic_Black_Mat";
        AssetDatabase.CreateAsset(blackPieceMat, Path.Combine(themePath, "Classic_Black_Mat.mat"));

        // Classic White Piece Material
        Material whitePieceMat = new Material(Shader.Find("Standard"));
        whitePieceMat.color = Color.white;
        whitePieceMat.name = "Classic_White_Mat";
        AssetDatabase.CreateAsset(whitePieceMat, Path.Combine(themePath, "Classic_White_Mat.mat"));

        // Classic Board Line Material
        Material boardLineMat = new Material(Shader.Find("Standard"));
        boardLineMat.color = Color.black;
        boardLineMat.name = "Classic_Board_Mat";
        AssetDatabase.CreateAsset(boardLineMat, Path.Combine(themePath, "Classic_Board_Mat.mat"));

        // Classic Board Point Material
        Material boardPointMat = new Material(Shader.Find("Standard"));
        boardPointMat.color = Color.black;
        boardPointMat.name = "Classic_Point_Mat";
        AssetDatabase.CreateAsset(boardPointMat, Path.Combine(themePath, "Classic_Point_Mat.mat"));
    }

    private void GenerateModernThemeMaterials(string basePath)
    {
        string themePath = Path.Combine(basePath, "Modern");
        if (!Directory.Exists(themePath))
        {
            Directory.CreateDirectory(themePath);
        }

        // Modern Black Piece Material
        Material blackPieceMat = new Material(Shader.Find("Standard"));
        blackPieceMat.color = new Color(0.1f, 0.1f, 0.1f); // Very dark gray
        blackPieceMat.name = "Modern_Black_Mat";
        AssetDatabase.CreateAsset(blackPieceMat, Path.Combine(themePath, "Modern_Black_Mat.mat"));

        // Modern White Piece Material
        Material whitePieceMat = new Material(Shader.Find("Standard"));
        whitePieceMat.color = new Color(0.9f, 0.9f, 0.9f); // Light gray
        whitePieceMat.name = "Modern_White_Mat";
        AssetDatabase.CreateAsset(whitePieceMat, Path.Combine(themePath, "Modern_White_Mat.mat"));

        // Modern Board Line Material
        Material boardLineMat = new Material(Shader.Find("Standard"));
        boardLineMat.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray
        boardLineMat.name = "Modern_Board_Mat";
        AssetDatabase.CreateAsset(boardLineMat, Path.Combine(themePath, "Modern_Board_Mat.mat"));

        // Modern Board Point Material
        Material boardPointMat = new Material(Shader.Find("Standard"));
        boardPointMat.color = new Color(0.3f, 0.3f, 0.3f); // Medium gray
        boardPointMat.name = "Modern_Point_Mat";
        AssetDatabase.CreateAsset(boardPointMat, Path.Combine(themePath, "Modern_Point_Mat.mat"));
    }

    private void GenerateNatureThemeMaterials(string basePath)
    {
        string themePath = Path.Combine(basePath, "Nature");
        if (!Directory.Exists(themePath))
        {
            Directory.CreateDirectory(themePath);
        }

        // Nature Black Piece Material
        Material blackPieceMat = new Material(Shader.Find("Standard"));
        blackPieceMat.color = new Color(0.1f, 0.1f, 0.1f); // Dark
        blackPieceMat.name = "Nature_Black_Mat";
        AssetDatabase.CreateAsset(blackPieceMat, Path.Combine(themePath, "Nature_Black_Mat.mat"));

        // Nature White Piece Material
        Material whitePieceMat = new Material(Shader.Find("Standard"));
        whitePieceMat.color = new Color(0.8f, 0.7f, 0.6f); // Light brown
        whitePieceMat.name = "Nature_White_Mat";
        AssetDatabase.CreateAsset(whitePieceMat, Path.Combine(themePath, "Nature_White_Mat.mat"));

        // Nature Board Line Material
        Material boardLineMat = new Material(Shader.Find("Standard"));
        boardLineMat.color = new Color(0.4f, 0.2f, 0.1f); // Brown
        boardLineMat.name = "Nature_Board_Mat";
        AssetDatabase.CreateAsset(boardLineMat, Path.Combine(themePath, "Nature_Board_Mat.mat"));

        // Nature Board Point Material
        Material boardPointMat = new Material(Shader.Find("Standard"));
        boardPointMat.color = new Color(0.3f, 0.6f, 0.3f); // Green
        boardPointMat.name = "Nature_Point_Mat";
        AssetDatabase.CreateAsset(boardPointMat, Path.Combine(themePath, "Nature_Point_Mat.mat"));
    }
}