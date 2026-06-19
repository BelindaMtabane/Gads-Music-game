using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the Rhythm Raiders splash for a fixed duration, then loads StartScene.
/// </summary>
public class SplashScreenController : MonoBehaviour
{
    [Header("Timing")]
    public float displayDuration = 3f;

    [Header("Scene")]
    public string nextSceneName = "StartScene";

    [Header("UI")]
    public RectTransform splashRoot;
    public Image splashImage;

    [Header("Animation")]
    public float pulseScale = 1.02f;
    public float pulseSpeed = 2f;

    private void Awake()
    {
        EnsureCanvasLayout();
    }

    private void Start()
    {
        UICursor.UnlockForMenu();
        Time.timeScale = 1f;
        EnsureCanvasLayout();
        StartCoroutine(RunSplash());
    }

    private void EnsureCanvasLayout()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private IEnumerator RunSplash()
    {
        if (splashRoot != null)
            splashRoot.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            AnimatePulse(elapsed);
            yield return null;
        }

        SceneFader.LoadScene(nextSceneName);
    }

    private void AnimatePulse(float time)
    {
        if (splashRoot == null) return;
        float t = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(1f, pulseScale, t);
        splashRoot.localScale = new Vector3(scale, scale, 1f);
    }
}
