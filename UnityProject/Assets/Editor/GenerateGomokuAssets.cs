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
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(baseFolder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Gomoku");

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

        // Create materials
        Shader std = Shader.Find("Standard");
        Material blackMat = new Material(std) { name = "Gomoku_Black_Mat" };
        blackMat.color = Color.black;
        blackMat.enableInstancing = true;
        string blackMatPath = Path.Combine(baseFolder, blackMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(blackMat, blackMatPath);

        Material whiteMat = new Material(std) { name = "Gomoku_White_Mat" };
        whiteMat.color = Color.white;
        whiteMat.enableInstancing = true;
        string whiteMatPath = Path.Combine(baseFolder, whiteMat.name + ".mat").Replace("\\","/");
        AssetDatabase.CreateAsset(whiteMat, whiteMatPath);

        // Create a prefab for the piece
        GameObject piece = new GameObject("Gomoku_Piece_Prefab");
        MeshFilter pmf = piece.AddComponent<MeshFilter>();
        MeshRenderer pmr = piece.AddComponent<MeshRenderer>();
        if (mesh != null) pmf.sharedMesh = mesh;
        pmr.sharedMaterial = blackMat; // default material; can be swapped per instance

        // Remove collider if present (none added)

        string prefabPath = Path.Combine(baseFolder, piece.name + ".prefab").Replace("\\","/");
        PrefabUtility.SaveAsPrefabAsset(piece, prefabPath);
        Object.DestroyImmediate(piece);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Gomoku sample assets created in Assets/Prefabs/Gomoku.\n- Mesh: " + (mesh!=null?mesh.name:"(none)") + "\n- Materials: " + blackMatPath + ", " + whiteMatPath + "\n- Prefab: " + prefabPath);
    }
}
