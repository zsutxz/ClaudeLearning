using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(Camera))]
public class RayTracingController : MonoBehaviour
{
    [Header("Ray Tracing Settings")]
    [SerializeField] private RayTracingShader rayTracingShader;
    [SerializeField] private Light directionalLight;
    [SerializeField] private int rayCount = 1;
    
    [Header("Performance")]
    [SerializeField] private bool enableDenoiser = true;
    [SerializeField] private int maxDepth = 4;
    
    private RayTracingAccelerationStructure accelerationStructure;
    private Camera mainCamera;
    private RenderTexture targetTexture;
    private RenderTexture denoisedTexture;
    
    private int frameCount = 0;
    
    void Start()
    {
        // Check for ray tracing support
        if (!SystemInfo.supportsRayTracing)
        {
            Debug.LogError("Ray tracing is not supported on this hardware.");
            enabled = false;
            return;
        }
        
        // Get main camera component
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("RayTracingController requires a Camera component.");
            enabled = false;
            return;
        }
        
        // Find directional light if not assigned
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }
        
        // Initialize acceleration structure
        accelerationStructure = new RayTracingAccelerationStructure();
        BuildAccelerationStructure();
        
        // Create render textures
        CreateRenderTextures();
    }
    
    void Update()
    {
        // Rebuild acceleration structure if needed
        if (accelerationStructure != null)
        {
            accelerationStructure.Update();
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Check if we have everything needed for ray tracing
        if (rayTracingShader == null || accelerationStructure == null || targetTexture == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Set shader parameters
        SetShaderParameters();
        
        // Execute ray tracing
        rayTracingShader.Execute(accelerationStructure, targetTexture);
        
        // Apply denoising if enabled
        if (enableDenoiser && denoisedTexture != null)
        {
            // Simple temporal accumulation for denoising
            Graphics.Blit(targetTexture, denoisedTexture);
            Graphics.Blit(denoisedTexture, destination);
        }
        else
        {
            Graphics.Blit(targetTexture, destination);
        }
        
        frameCount++;
    }
    
    void SetShaderParameters()
    {
        // Set acceleration structure
        rayTracingShader.SetAccelerationStructure("_AccelerationStructure", accelerationStructure);
        
        // Set camera parameters
        rayTracingShader.SetFloat("_CameraFarDistance", mainCamera.farClipPlane);
        rayTracingShader.SetVector("_WorldSpaceCameraPos", mainCamera.transform.position);
        
        // Set view-projection matrices
        Matrix4x4 viewMatrix = mainCamera.worldToCameraMatrix;
        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false);
        Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
        Matrix4x4 invViewProjMatrix = viewProjMatrix.inverse;
        
        rayTracingShader.SetMatrix("_InvViewProjectionMatrix", invViewProjMatrix);
        
        // Set light parameters
        if (directionalLight != null)
        {
            rayTracingShader.SetVector("_DirectionalLightDirection", directionalLight.transform.forward);
            rayTracingShader.SetVector("_DirectionalLightColor", directionalLight.color * directionalLight.intensity);
        }
        
        // Set ray count
        rayTracingShader.SetInt("_RayCount", rayCount);
        
        // Set frame count for temporal effects
        rayTracingShader.SetInt("_FrameCount", frameCount);
    }
    
    void BuildAccelerationStructure()
    {
        if (accelerationStructure == null) return;
        
        // Add all renderers in the scene to the acceleration structure
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Skip the camera's renderer if it has one
            if (renderer.GetComponent<Camera>() != null) continue;
            
            // Add renderer to acceleration structure
            accelerationStructure.AddInstance(renderer, renderer.transform.localToWorldMatrix);
        }
        
        // Build the acceleration structure
        accelerationStructure.Build();
    }
    
    void CreateRenderTextures()
    {
        // Clean up existing textures
        if (targetTexture != null)
        {
            targetTexture.Release();
            Destroy(targetTexture);
        }
        
        if (denoisedTexture != null)
        {
            denoisedTexture.Release();
            Destroy(denoisedTexture);
        }
        
        // Create new textures
        targetTexture = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
        targetTexture.enableRandomWrite = true;
        targetTexture.Create();
        
        if (enableDenoiser)
        {
            denoisedTexture = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            denoisedTexture.enableRandomWrite = true;
            denoisedTexture.Create();
        }
    }
    
    void OnDisable()
    {
        // Clean up resources
        if (accelerationStructure != null)
        {
            accelerationStructure.Dispose();
            accelerationStructure = null;
        }
        
        if (targetTexture != null)
        {
            targetTexture.Release();
            Destroy(targetTexture);
            targetTexture = null;
        }
        
        if (denoisedTexture != null)
        {
            denoisedTexture.Release();
            Destroy(denoisedTexture);
            denoisedTexture = null;
        }
    }
    
    void OnDestroy()
    {
        // Ensure resources are cleaned up
        OnDisable();
    }
}