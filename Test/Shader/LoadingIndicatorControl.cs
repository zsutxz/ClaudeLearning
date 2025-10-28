using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode] // This allows the script to run in the editor
public class LoadingIndicatorControl : MonoBehaviour
{
    public enum IndicatorType { Radial, Bar }
    public IndicatorType type = IndicatorType.Radial;

    [Header("Shared Properties")]
    [Range(0, 1)]
    public float progress = 1.0f;
    public Color tintColor = Color.white;

    [Header("Bar Properties")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    public bool indeterminateBar = false;

    private Image image;
    private Material materialInstance;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image != null && image.material != null)
        {
            // Create an instance to avoid changing the asset directly
            materialInstance = new Material(image.material);
            image.material = materialInstance;
        }
    }

    void Update()
    {
        if (materialInstance == null)
        {
            if (image != null && image.material != null)
            {
                 materialInstance = new Material(image.material);
                 image.material = materialInstance;
            }
            else
            {
                return;
            }
        }

        // Set properties based on the selected type
        materialInstance.SetColor("_Color", tintColor);
        materialInstance.SetFloat("_Progress", progress);

        if (type == IndicatorType.Bar)
        {
            materialInstance.SetColor("_BackgroundColor", backgroundColor);
            if (indeterminateBar)
            {
                materialInstance.EnableKeyword("INDETERMINATE_MODE");
            }
            else
            {
                materialInstance.DisableKeyword("INDETERMINATE_MODE");
            }
        }
    }
}
