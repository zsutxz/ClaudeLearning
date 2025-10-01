using UnityEngine;
using System.Collections.Generic;

// 确保游戏对象上有关联的 MeshFilter 和 MeshRenderer 组件
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrailController : MonoBehaviour
{
    [Header("拖尾设置")]
    public float lifetime = 2.0f; // 拖尾上每个点持续的时间
    public float minimumVertexDistance = 0.1f; // 添加新点之前需要移动的最小距离
    public float startWidth = 1.0f; // 拖尾的初始宽度
    public float endWidth = 0.0f;   // 拖尾的结束宽度

    [Header("材质")]
    public Material trailMaterial; // 使用我们自定义拖尾着色器的材质

    private Mesh _mesh;
    private List<TrailPoint> _points = new List<TrailPoint>();
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    // 用于存储位置和创建时间的辅助类
    private class TrailPoint
    {
        public Vector3 position;
        public float creationTime;
    }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        _mesh = new Mesh();
        _mesh.name = "Trail Mesh";
        _meshFilter.mesh = _mesh;

        // 为渲染器指定材质
        _meshRenderer.material = trailMaterial;
    }

    void Update()
    {
        // 移除生命周期结束的旧点
        _points.RemoveAll(p => Time.time - p.creationTime > lifetime);

        // 如果物体移动了足够远的距离，就添加一个新点
        if (_points.Count == 0 || Vector3.Distance(_points[_points.Count - 1].position, transform.position) > minimumVertexDistance)
        {
            _points.Add(new TrailPoint { position = transform.position, creationTime = Time.time });
        }

        // 生成或更新网格
        GenerateMesh();
    }

    void GenerateMesh()
    {
        // 点数少于2个，无法构成一个面
        if (_points.Count < 2)
        {
            _mesh.Clear();
            return;
        }

        Vector3[] vertices = new Vector3[_points.Count * 2];
        Vector2[] uvs = new Vector2[_points.Count * 2];
        int[] triangles = new int[(_points.Count - 1) * 6];
        Color[] colors = new Color[_points.Count * 2];

        // 获取摄像机的 "right" 向量，用于将拖尾面向摄像机
        Vector3 cameraRight = Camera.main.transform.right;

        for (int i = 0; i < _points.Count; i++)
        {
            TrailPoint currentPoint = _points[i];
            float age = (Time.time - currentPoint.creationTime) / lifetime; // "年龄"比例 (0 = 新, 1 = 旧)

            // 根据“年龄”计算当前点的宽度
            float width = Mathf.Lerp(startWidth, endWidth, age);

            // 设置顶点位置，一个在左，一个在右
            vertices[i * 2] = currentPoint.position - cameraRight * width * 0.5f;
            vertices[i * 2 + 1] = currentPoint.position + cameraRight * width * 0.5f;

            // 设置 UV。U 坐标代表了顶点所处拖尾的“年龄”。
            // 着色器将使用这个值来让拖尾渐隐。
            uvs[i * 2] = new Vector2(age, 0);
            uvs[i * 2 + 1] = new Vector2(age, 1);
            
            // 设置顶点颜色（可选，但有助于调试或实现更复杂的效果）
            colors[i * 2] = colors[i * 2 + 1] = Color.white;

            // 当我们有至少两个点时，开始构建三角形
            if (i > 0)
            {
                // 每个分段由两个三角形（6个顶点索引）组成
                int baseIndex = (i - 1) * 6;
                int vertIndex = (i - 1) * 2;
                triangles[baseIndex + 0] = vertIndex + 0;
                triangles[baseIndex + 1] = vertIndex + 1;
                triangles[baseIndex + 2] = vertIndex + 2;

                triangles[baseIndex + 3] = vertIndex + 2;
                triangles[baseIndex + 4] = vertIndex + 1;
                triangles[baseIndex + 5] = vertIndex + 3;
            }
        }

        // 将计算好的数据应用到 Mesh 上
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        _mesh.colors = colors;
        _mesh.RecalculateBounds(); // 重新计算包围盒
    }
}
