using UnityEngine;

/// <summary>
/// Prevents UI canvas scale (0,0,0) from hiding HUD, menus, and end screens.
/// Runs very early and again each frame until scale is valid.
/// </summary>
[DefaultExecutionOrder(1000)]
public class GameplayCanvasGuard : MonoBehaviour
{
    private void Awake() => EnsureCanvasScale(gameObject);
    private void OnEnable() => EnsureCanvasScale(gameObject);
    private void LateUpdate() => EnsureCanvasScale(gameObject);

    public static void EnsureCanvasScale(GameObject canvasRoot)
    {
        if (canvasRoot == null) return;

        var rt = canvasRoot.GetComponent<RectTransform>();
        if (rt != null && rt.localScale.sqrMagnitude < 0.001f)
            rt.localScale = Vector3.one;
    }

    public static void FixAllCanvasesInScene()
    {
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            EnsureCanvasScale(canvas.gameObject);
    }
}
