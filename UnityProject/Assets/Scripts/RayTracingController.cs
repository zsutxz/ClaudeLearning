using UnityEngine;
using UnityEngine.UI;

public class RayTracingController : MonoBehaviour
{
    [Header("Ray Tracing Parameters")]
    public float reflectionIntensity = 0.5f;
    public float shadowSoftness = 0.5f;
    public int rayDepth = 3;
    
    [Header("UI Elements")]
    public Slider reflectionSlider;
    public Slider shadowSlider;
    public Material rayTracingMaterial;
    
    void Start()
    {
        // Initialize UI controls
        if (reflectionSlider != null)
        {
            reflectionSlider.onValueChanged.AddListener(OnReflectionIntensityChanged);
            reflectionSlider.value = reflectionIntensity;
        }
        
        if (shadowSlider != null)
        {
            shadowSlider.onValueChanged.AddListener(OnShadowSoftnessChanged);
            shadowSlider.value = shadowSoftness;
        }
    }
    
    void OnReflectionIntensityChanged(float value)
    {
        reflectionIntensity = value;
        if (rayTracingMaterial != null)
        {
            rayTracingMaterial.SetFloat("_ReflectionIntensity", value);
        }
    }
    
    void OnShadowSoftnessChanged(float value)
    {
        shadowSoftness = value;
        if (rayTracingMaterial != null)
        {
            rayTracingMaterial.SetFloat("_ShadowSoftness", value);
        }
    }
    
    // Additional methods for controlling ray tracing parameters
}