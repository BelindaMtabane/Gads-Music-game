using UnityEngine;

/// <summary>
/// Attach to HUD_Root. Drag the Scale slider in the Inspector to
/// resize the entire gameplay HUD without re-running any tool.
/// </summary>
public class HUDScaler : MonoBehaviour
{
    [Header("HUD Scale (1 = default)")]
    [Range(0.5f, 2.0f)]
    public float scale = 1.0f;

    [Header("Icon Size Override (0 = use default)")]
    [Range(0f, 80f)]
    public float iconSize = 0f;   // 0 means "don't override"

    void OnValidate()
    {
        Apply();
    }

    void Start()
    {
        Apply();
    }

    void Apply()
    {
        transform.localScale = Vector3.one * scale;

        if (iconSize > 0f)
        {
            // Resize every Icon child inside the HUD
            foreach (var img in GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (img.gameObject.name == "Icon")
                {
                    var rt = img.rectTransform;
                    rt.sizeDelta = new Vector2(iconSize, iconSize);
                }
            }
        }
    }
}
