using UnityEngine;

[ExecuteInEditMode]
public class WaterfallParticleSystemCreator : MonoBehaviour
{
    [Header("Particle System Settings")]
    public bool createParticleSystem = false;
    public Transform particleParent;
    
    void Update()
    {
        if (createParticleSystem)
        {
            CreateWaterfallParticleSystem();
            createParticleSystem = false;
        }
    }
    
    /// <summary>
    /// Creates a waterfall particle system programmatically
    /// </summary>
    public void CreateWaterfallParticleSystem()
    {
        // Create parent object for particle system
        GameObject particleSystemObject = new GameObject("WaterfallParticleSystem");
        if (particleParent != null)
        {
            particleSystemObject.transform.SetParent(particleParent);
        }
        particleSystemObject.transform.localPosition = Vector3.zero;
        
        // Add particle system component
        ParticleSystem ps = particleSystemObject.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = ps.main;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.5f, 0.7f, 1.0f, 0.8f); // Light blue color
        main.startLifetime = 1.5f;
        main.gravityModifier = 0.5f;
        main.maxParticles = 1000;
        
        // Configure emission module
        var emission = ps.emission;
        emission.rateOverTime = 100;
        
        // Configure shape module
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;
        
        // Configure color over lifetime
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.5f, 0.7f, 1.0f, 0.8f), 0.0f), 
                new GradientColorKey(new Color(0.3f, 0.5f, 0.8f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0.3f, 0.6f, 0.0f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0.0f), 
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Configure size over lifetime
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(
            new Keyframe(0.0f, 0.5f),
            new Keyframe(0.5f, 1.0f),
            new Keyframe(1.0f, 0.2f)
        ));
        
        // Add renderer component settings
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        Debug.Log("Waterfall particle system created successfully!");
    }
}