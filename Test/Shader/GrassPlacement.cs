using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GrassPlacement : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public int instanceCount = 10000;
    public float placementArea = 100f;

    private List<Matrix4x4> matrices;

    void Start()
    {
        // Ensure the material has GPU instancing enabled
        if (grassMaterial != null)
        {
            grassMaterial.enableInstancing = true;
        }

        // Bounding box solution for culling
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Mesh boundsMesh = new Mesh();
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(placementArea, 10, placementArea));
        boundsMesh.bounds = bounds;
        filter.mesh = boundsMesh;
        renderer.enabled = false;

        // Generate matrices for the grass
        matrices = new List<Matrix4x4>(instanceCount);
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-placementArea / 2, placementArea / 2),
                0,
                Random.Range(-placementArea / 2, placementArea / 2)
            );
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float scale = Random.Range(0.8f, 1.2f);
            matrices.Add(Matrix4x4.TRS(position, rotation, new Vector3(scale, scale, scale)));
        }
    }

    void Update()
    {
        if (matrices == null || matrices.Count == 0 || grassMaterial == null) return;

        Graphics.DrawMeshInstanced(
            grassMesh,
            0,
            grassMaterial,
            matrices
        );
    }
}