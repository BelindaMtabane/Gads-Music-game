using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Smooth black-screen fade when loading scenes.
/// Self-creates its own persistent Canvas + Image so the overlay survives
/// every scene transition without losing its reference.
///
/// Usage from any script:
///   SceneFader.LoadScene("MainGameL1");
///   SceneFader.LoadScene(1);   // by build index
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Tooltip("Seconds for fade-in and fade-out.")]
    public float fadeDuration = 0.5f;

    // Created at runtime — survives scene transitions as a child of this GO
    private GameObject _fadeCanvasRoot;
    private Image _fadeImage;
    private CanvasGroup _fadeGroup;
    private Coroutine _fadeInRoutine;

    /// <summary>True while the black overlay is visible (blocks the scene underneath).</summary>
    public bool IsBlocking { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Must be a root object for DontDestroyOnLoad to work
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Build a self-contained Canvas + Image under this GameObject.
        // Because this GO is DontDestroyOnLoad, the canvas and image persist too.
        BuildFadeCanvas();

        DisableLegacySceneFadeOverlays();

        bool isSplash = SceneManager.GetActiveScene().name == "SplashScene";
        SetAlpha(isSplash ? 0f : 1f);
        if (!isSplash)
            _fadeImage.raycastTarget = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "SplashScene")
            return;

        BeginFadeIn();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UICursor.ApplyForScene(scene.name);
        Time.timeScale = 1f;
        UIInputFix.EnsureEventSystem();
        DisableLegacySceneFadeOverlays();

        if (scene.name == "SplashScene")
        {
            SetAlpha(0f);
            return;
        }

        if (scene.name == "DeathScene" || scene.name == "VictoryScene")
            GameOverOverlay.Hide();

        BeginFadeIn();
    }

    private void BeginFadeIn()
    {
        if (_fadeImage == null) return;
        if (_fadeInRoutine != null)
            StopCoroutine(_fadeInRoutine);
        _fadeInRoutine = StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Old setup scripts left a full-screen "FadeOverlay" Image on each Canvas.
    /// SceneFader owns fading now — disable those so they don't block the Game view.
    /// </summary>
    private static void DisableLegacySceneFadeOverlays()
    {
        var overlays = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var img in overlays)
        {
            if (img != null && img.gameObject.name == "FadeOverlay")
                img.gameObject.SetActive(false);
        }
    }

    // ── Canvas builder ────────────────────────────────────────────────────────

    private void BuildFadeCanvas()
    {
        // Canvas
        _fadeCanvasRoot = new GameObject("_SceneFadeCanvas");
        _fadeCanvasRoot.transform.SetParent(transform);

        var canvas = _fadeCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;          // always on top

        _fadeCanvasRoot.AddComponent<CanvasScaler>();
        _fadeCanvasRoot.AddComponent<GraphicRaycaster>();

        _fadeGroup = _fadeCanvasRoot.AddComponent<CanvasGroup>();

        // Full-screen black Image
        var imgGO = new GameObject("_FadeImage");
        imgGO.transform.SetParent(_fadeCanvasRoot.transform, false);

        // Add Image first — this auto-adds RectTransform on the GameObject
        _fadeImage = imgGO.AddComponent<Image>();

        // Now RectTransform exists, configure it to fill the screen
        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        _fadeImage.color       = Color.black;
        _fadeImage.raycastTarget = true;
    }

    // ── Static convenience ────────────────────────────────────────────────────

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeOutThenLoad(sceneName, -1));
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int buildIndex)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeOutThenLoad(null, buildIndex));
        else
            SceneManager.LoadScene(buildIndex);
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        if (_fadeCanvasRoot != null)
            _fadeCanvasRoot.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        _fadeInRoutine = null;
    }

    /// <summary>Clears a stuck black overlay so end-scene UI can show.</summary>
    public static void ForceClearFade()
    {
        if (Instance == null) return;
        if (Instance._fadeInRoutine != null)
        {
            Instance.StopCoroutine(Instance._fadeInRoutine);
            Instance._fadeInRoutine = null;
        }
        Instance.SetAlpha(0f);
    }

    private IEnumerator FadeOutThenLoad(string sceneName, int buildIndex)
    {
        if (_fadeInRoutine != null)
        {
            StopCoroutine(_fadeInRoutine);
            _fadeInRoutine = null;
        }

        if (_fadeCanvasRoot != null)
            _fadeCanvasRoot.SetActive(true);

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
        if (_fadeImage == null) return;
        Color c = _fadeImage.color;
        c.a = a;
        _fadeImage.color = c;

        bool blocks = a > 0.01f;
        IsBlocking = blocks;
        _fadeImage.raycastTarget = blocks;
        if (_fadeGroup != null)
        {
            _fadeGroup.blocksRaycasts = blocks;
            _fadeGroup.interactable   = blocks;
        }

        // Fully transparent — hide overlay so narration/UI underneath is visible
        if (_fadeCanvasRoot != null)
            _fadeCanvasRoot.SetActive(blocks);
    }
}
