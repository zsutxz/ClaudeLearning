using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CommandBufferBlur : MonoBehaviour
{
    [Tooltip("模糊着色器")]
    public Shader gaussianBlurShader;

    [Range(0, 10)]
    [Tooltip("模糊程度")]
    public int blurIterations = 2;

    [Range(1, 8)]
    [Tooltip("降采样率，数值越高，性能越好，但效果越模糊")]
    public int downSample = 2;

    private Material _material;
    private CommandBuffer _commandBuffer;
    private Camera _camera;

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("此脚本需要附加到相机上。");
            return;
        }

        if (gaussianBlurShader == null)
        {
            Debug.LogError("请分配模糊着色器（GaussianBlur.shader）。");
            enabled = false;
            return;
        }

        if (_material == null)
        {
            _material = new Material(gaussianBlurShader);
            _material.hideFlags = HideFlags.HideAndDontSave;
        }

        // 创建一个新的 Command Buffer
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Gaussian Blur";

        // 定义临时渲染纹理的ID
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        int blurTemp1ID = Shader.PropertyToID("_BlurTemp1");
        int blurTemp2ID = Shader.PropertyToID("_BlurTemp2");

        // 获取屏幕尺寸
        int width = _camera.pixelWidth / downSample;
        int height = _camera.pixelHeight / downSample;

        // 1. 创建一个降采样的临时渲染纹理来存储屏幕图像
        _commandBuffer.GetTemporaryRT(screenCopyID, width, height, 0, FilterMode.Bilinear);
        // 将屏幕内容复制到这个临时纹理中
        _commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, screenCopyID);

        // 2. 创建两个用于“乒乓”模糊的临时渲染纹理
        _commandBuffer.GetTemporaryRT(blurTemp1ID, width, height, 0, FilterMode.Bilinear);
        _commandBuffer.GetTemporaryRT(blurTemp2ID, width, height, 0, FilterMode.Bilinear);

        // 初始模糊，从 screenCopyID 到 blurTemp1ID
        _commandBuffer.Blit(screenCopyID, blurTemp1ID, _material, 0); // Pass 0: 垂直模糊

        // 3. 执行多次“乒乓”模糊
        for (int i = 0; i < blurIterations; i++)
        {
            // 水平模糊
            _commandBuffer.Blit(blurTemp1ID, blurTemp2ID, _material, 1); // Pass 1: 水平模糊
            // 垂直模糊
            _commandBuffer.Blit(blurTemp2ID, blurTemp1ID, _material, 0); // Pass 0: 垂直模糊
        }

        // 4. 将最终模糊结果渲染回屏幕
        _commandBuffer.Blit(blurTemp1ID, BuiltinRenderTextureType.CameraTarget);

        // 5. 释放所有临时渲染纹理
        _commandBuffer.ReleaseTemporaryRT(screenCopyID);
        _commandBuffer.ReleaseTemporaryRT(blurTemp1ID);
        _commandBuffer.ReleaseTemporaryRT(blurTemp2ID);

        // 将 Command Buffer 添加到相机的渲染管线中
        // AfterSkybox 事件表示在天空盒渲染之后，但在透明物体和UI渲染之前执行
        _camera.AddCommandBuffer(CameraEvent.AfterSkybox, _commandBuffer);
    }

    void OnDisable()
    {
        // 清理
        if (_camera != null && _commandBuffer != null)
        {
            _camera.RemoveCommandBuffer(CameraEvent.AfterSkybox, _commandBuffer);
            _commandBuffer.Dispose();
            _commandBuffer = null;
        }
        if (_material != null)
        {
            DestroyImmediate(_material);
            _material = null;
        }
    }
}
