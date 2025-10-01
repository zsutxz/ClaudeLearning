using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ImageFader : MonoBehaviour
{
    public float fadeDuration = 1.0f;

    private Material materialInstance;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        Image image = GetComponent<Image>();
        if (image.material != null && image.material.shader.name == "UI/ImageFade")
        {
            // It's important to create an instance of the material
            // otherwise, we would be changing the material asset itself.
            materialInstance = new Material(image.material);
            image.material = materialInstance;
        }
        else
        {
            Debug.LogError("Please assign a material with the UI/ImageFade shader to this Image component.");
        }
    }

    /// <summary>
    /// Starts the fade-in animation.
    /// </summary>
    public void FadeIn()
    {
        if (materialInstance == null) return;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(AnimateFade(0f, 1f));
    }

    /// <summary>
    /// Starts the fade-out animation.
    /// </summary>
    public void FadeOut()
    {
        if (materialInstance == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(AnimateFade(1f, 0f));
    }

    private IEnumerator AnimateFade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            materialInstance.SetFloat("_Alpha", newAlpha);
            yield return null;
        }

        // Ensure the final alpha value is set correctly
        materialInstance.SetFloat("_Alpha", endAlpha);
    }

    // --- Example Usage ---
    // You can call FadeIn() and FadeOut() from other scripts or UI events.
    void OnEnable()
    {
        // Example: Automatically fade in when the object becomes active.
        // Set initial alpha to 0 to ensure it's invisible before fading in.
        if (materialInstance != null)
        {
            materialInstance.SetFloat("_Alpha", 0f);
            FadeIn();
        }
    }

    // Example: You could have a button that calls this method to fade the image out.
    public void TriggerFadeOut()
    {
        FadeOut();
    }
}
