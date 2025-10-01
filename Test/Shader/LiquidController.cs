using UnityEngine;

[ExecuteInEditMode]
public class LiquidController : MonoBehaviour
{
    private const int MAX_PARTICLES = 64;

    [Tooltip("用于渲染液体的材质")]
    public Material liquidMaterial;

    [Tooltip("粒子列表")]
    public Transform[] particles;

    private Vector4[] particlePositions = new Vector4[MAX_PARTICLES];

    void Update()
    {
        // --- 调试代码开始 ---
        if (liquidMaterial == null)
        {
            Debug.LogError("LiquidController: 液体材质 (Liquid Material) 未设置!", this.gameObject);
            return; // 如果没有材质，就停止执行
        }

        if (particles == null || particles.Length == 0)
        {
            Debug.LogWarning("LiquidController: 粒子列表 (Particles) 为空。", this.gameObject);
            // 即使列表为空，我们也要告诉Shader粒子数量为0
            liquidMaterial.SetInt("_ParticlesCount", 0);
            return;
        }
        // --- 调试代码结束 ---

        int count = Mathf.Min(particles.Length, MAX_PARTICLES);
        int validParticleCount = 0;
        
        for (int i = 0; i < count; i++)
        {
            if (particles[i] != null)
            {
                particlePositions[validParticleCount] = particles[i].position;
                validParticleCount++;
            }
        }

        liquidMaterial.SetInt("_ParticlesCount", validParticleCount);
        if (validParticleCount > 0)
        {
            liquidMaterial.SetVectorArray("_Particles", particlePositions);
        }

        // --- 调试代码开始 ---
        // 每 100 帧打印一次日志，避免刷屏
        if (Time.frameCount % 100 == 0) 
        {
            Debug.Log($"LiquidController: 正在发送 {validParticleCount} 个粒子数据到材质 '{liquidMaterial.name}'。第一个粒子的位置: { (validParticleCount > 0 ? particlePositions[0].ToString() : "N/A") }", this.gameObject);
        }
        // --- 调试代码结束 ---
    }
}