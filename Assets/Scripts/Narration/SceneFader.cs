using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Smooth black-screen fade when loading scenes.
/// Add a SceneFader GameObject (with a full-screen black Image) to every scene,
/// or use the SetupNarrationScenes editor tool which creates it automatically.
///
/// Usage from any script:
///   SceneFader.LoadScene("MainGameL1");
///   SceneFader.LoadScene(1);           // by build index
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Tooltip("The full-screen black Image used for fading.")]
    public Image fadeImage;

    [Tooltip("Seconds for fade-in and fade-out.")]
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully black then fade in
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.raycastTarget = true;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    // ── Static convenience ────────────────────────────────────────────────────

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeOutThenLoad(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int buildIndex)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeOutThenLoad(buildIndex.ToString(), buildIndex));
        else
            SceneManager.LoadScene(buildIndex);
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
        if (fadeImage != null) fadeImage.raycastTarget = false;
    }

    private IEnumerator FadeOutThenLoad(string sceneName, int buildIndex = -1)
    {
        if (fadeImage != null) fadeImage.raycastTarget = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);

        if (buildIndex >= 0)
            SceneManager.LoadScene(buildIndex);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
