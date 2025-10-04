using UnityEditor;
using UnityEngine;
using System.IO;

// Editor utility to generate sample piece mesh, materials and a prefab for Gomoku
public static class GenerateGomokuAssets
{
    [MenuItem("Gomoku/Create Sample Assets (Mesh/Prefab/Materials)")]
    public static void CreateAssets()
    {
        string baseFolder = "Assets/Prefabs/Gomoku";
        string materialsFolder = "Assets/Materials/Themes";
        
        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(baseFolder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Gomoku");
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(materialsFolder))
            AssetDatabase.CreateFolder("Assets/Materials", "Themes");

        // Create a low-poly sphere mesh by extracting from a primitive
        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        MeshFilter mf = tmp.GetComponent<MeshFilter>();
        Mesh mesh = null;
        if (mf != null && mf.sharedMesh != null)
        {
            mesh = Object.Instantiate(mf.sharedMesh);
            mesh.name = "Gomoku_Piece_Mesh";
            string meshPath = Path.Combine(baseFolder, mesh.name + ".asset").Replace("\\","/");
            AssetDatabase.CreateAsset(mesh, meshPath);
            Debug.Log($"Created mesh asset: {meshPath}");
        }
        Object.DestroyImmediate(tmp);

        // Create materials for all themes
        Shader std = Shader.Find("Standard");
        
        // Classic theme materials
        Material classicBlackMat = new Material(std) { name = "Classic_Black_Mat" };
        classicBlackMat.color = Color.black;
        classicBlackMat.enableInstancing = true;
        string classicBlackMatPath = Path.Combine(materialsFolder, classicBlackMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(classicBlackMat, classicBlackMatPath);

        Material classicWhiteMat = new Material(std) { name = "Classic_White_Mat" };
        classicWhiteMat.color = Color.white;
        classicWhiteMat.enableInstancing = true;
        string classicWhiteMatPath = Path.Combine(materialsFolder, classicWhiteMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(classicWhiteMat, classicWhiteMatPath);
        
        Material classicBoardMat = new Material(std) { name = "Classic_Board_Mat" };
        classicBoardMat.color = new Color(0.8f, 0.6f, 0.4f); // Wood-like color
        string classicBoardMatPath = Path.Combine(materialsFolder, classicBoardMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(classicBoardMat, classicBoardMatPath);
        
        Material classicPointMat = new Material(std) { name = "Classic_Point_Mat" };
        classicPointMat.color = Color.black;
        string classicPointMatPath = Path.Combine(materialsFolder, classicPointMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(classicPointMat, classicPointMatPath);

        // Modern theme materials
        Material modernBlackMat = new Material(std) { name = "Modern_Black_Mat" };
        modernBlackMat.color = new Color(0.1f, 0.1f, 0.1f); // Very dark gray
        modernBlackMat.enableInstancing = true;
        string modernBlackMatPath = Path.Combine(materialsFolder, modernBlackMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(modernBlackMat, modernBlackMatPath);

        Material modernWhiteMat = new Material(std) { name = "Modern_White_Mat" };
        modernWhiteMat.color = new Color(0.9f, 0.9f, 0.9f); // Light gray
        modernWhiteMat.enableInstancing = true;
        string modernWhiteMatPath = Path.Combine(materialsFolder, modernWhiteMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(modernWhiteMat, modernWhiteMatPath);
        
        Material modernBoardMat = new Material(std) { name = "Modern_Board_Mat" };
        modernBoardMat.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray
        string modernBoardMatPath = Path.Combine(materialsFolder, modernBoardMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(modernBoardMat, modernBoardMatPath);
        
        Material modernPointMat = new Material(std) { name = "Modern_Point_Mat" };
        modernPointMat.color = new Color(0.3f, 0.3f, 0.3f); // Medium gray
        string modernPointMatPath = Path.Combine(materialsFolder, modernPointMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(modernPointMat, modernPointMatPath);

        // Nature theme materials
        Material natureBlackMat = new Material(std) { name = "Nature_Black_Mat" };
        natureBlackMat.color = new Color(0.1f, 0.1f, 0.1f); // Dark
        natureBlackMat.enableInstancing = true;
        string natureBlackMatPath = Path.Combine(materialsFolder, natureBlackMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(natureBlackMat, natureBlackMatPath);

        Material natureWhiteMat = new Material(std) { name = "Nature_White_Mat" };
        natureWhiteMat.color = new Color(0.8f, 0.7f, 0.6f); // Light brown
        natureWhiteMat.enableInstancing = true;
        string natureWhiteMatPath = Path.Combine(materialsFolder, natureWhiteMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(natureWhiteMat, natureWhiteMatPath);
        
        Material natureBoardMat = new Material(std) { name = "Nature_Board_Mat" };
        natureBoardMat.color = new Color(0.4f, 0.2f, 0.1f); // Brown
        string natureBoardMatPath = Path.Combine(materialsFolder, natureBoardMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(natureBoardMat, natureBoardMatPath);
        
        Material naturePointMat = new Material(std) { name = "Nature_Point_Mat" };
        naturePointMat.color = new Color(0.3f, 0.6f, 0.3f); // Green
        string naturePointMatPath = Path.Combine(materialsFolder, naturePointMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(naturePointMat, naturePointMatPath);

        // Create a prefab for the piece
        GameObject piece = new GameObject("Gomoku_Piece_Prefab");
        MeshFilter pmf = piece.AddComponent<MeshFilter>();
        MeshRenderer pmr = piece.AddComponent<MeshRenderer>();
        if (mesh != null) pmf.sharedMesh = mesh;
        pmr.sharedMaterial = classicBlackMat; // default material; can be swapped per instance

        // Remove collider if present (none added)

        string prefabPath = Path.Combine(baseFolder, piece.name + ".prefab").Replace("\\","/");
        PrefabUtility.SaveAsPrefabAsset(piece, prefabPath);
        Object.DestroyImmediate(piece);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Gomoku sample assets created in Assets/Prefabs/Gomoku.\n- Mesh: " + (mesh!=null?mesh.name:"(none)") + "\n- Materials: " + classicBlackMatPath + ", " + classicWhiteMatPath + "\n- Prefab: " + prefabPath);
    }
}
