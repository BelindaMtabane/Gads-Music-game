using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Persistent full-screen game over message. Survives scene Canvas scale (0,0,0) bugs.
/// </summary>
public class GameOverOverlay : MonoBehaviour
{
    public static GameOverOverlay Instance { get; private set; }

    const int SortOrder = 850;

    GameObject _root;
    TextMeshProUGUI _label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() => EnsureInstance();

    public static void EnsureInstance()
    {
        if (Instance != null) return;

        var go = new GameObject("_GameOverOverlay");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GameOverOverlay>();
        Instance.Build();
    }

    void Build()
    {
        _root = new GameObject("Canvas");
        _root.transform.SetParent(transform, false);

        var canvas = _root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortOrder;

        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(_root.transform, false);
        var dimRt = dim.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.5f);
        dimImg.raycastTarget = false;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(_root.transform, false);
        var rt = textGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.15f);
        rt.anchorMax = new Vector2(0.95f, 0.85f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _label = textGo.AddComponent<TextMeshProUGUI>();
        TmpUiUtility.EnsureFont(_label);
        _label.alignment = TextAlignmentOptions.Center;
        _label.fontSize = 48f;
        _label.color = Color.white;
        _label.raycastTarget = false;

        _root.SetActive(false);
    }

    public static void Show(string body)
    {
        EnsureInstance();
        if (Instance._label == null) return;

        TmpUiUtility.SetSafeText(Instance._label, body);
        Instance._root.SetActive(true);
    }

    public static void Hide()
    {
        if (Instance == null || Instance._root == null) return;
        Instance._root.SetActive(false);
    }
}
