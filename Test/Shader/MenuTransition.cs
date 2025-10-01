using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class MenuTransition : MonoBehaviour
{
    public enum TransitionType
    {
        Fade,
        Slide,
        Popup
    }

    public TransitionType transitionType = TransitionType.Fade;
    public float duration = 0.5f;
    public Vector2 slideDirection = new Vector2(-1, 0); // Default to slide from left

    private Material materialInstance;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        // Create a new instance of the material for this object
        Image image = GetComponent<Image>();
        if (image.material != null)
        {
            materialInstance = new Material(image.material);
            image.material = materialInstance;
        }
        else
        {
            Debug.LogError("Please assign a material with the UI/MenuTransition shader to this Image.");
        }
    }

    // Call this to start the transition in (e.g., show the menu)
    public void TransitionIn()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(Animate(0, 1));
    }

    // Call this to start the transition out (e.g., hide the menu)
    public void TransitionOut()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(Animate(1, 0));
    }

    private IEnumerator Animate(float from, float to)
    {
        // Enable the correct shader keyword based on the selected transition type
        EnableKeyword(transitionType.ToString().ToUpper() + "_ON");

        if (transitionType == TransitionType.Slide)
        {
            materialInstance.SetVector("_SlideDirection", slideDirection.normalized);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Lerp(from, to, elapsed / duration);
            materialInstance.SetFloat("_Progress", progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        materialInstance.SetFloat("_Progress", to);
    }

    private void EnableKeyword(string keyword)
    {
        materialInstance.DisableKeyword("FADE_ON");
        materialInstance.DisableKeyword("SLIDE_ON");
        materialInstance.DisableKeyword("POPUP_ON");
        materialInstance.EnableKeyword(keyword);
    }

    // Example usage (you can call these from buttons or other scripts)
    void OnEnable()
    {
        // Automatically transition in when the object is enabled
        TransitionIn();
    }

    // Example: Call this from a close button's OnClick event
    public void CloseMenu()
    {
        TransitionOut();
        // Optional: Deactivate the GameObject after the transition
        // StartCoroutine(DeactivateAfterDelay(duration));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
