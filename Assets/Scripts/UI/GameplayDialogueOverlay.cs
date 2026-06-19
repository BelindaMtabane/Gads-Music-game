using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight top-center gameplay subtitles. Does not open the narration panel.
/// </summary>
public class GameplayDialogueOverlay : MonoBehaviour
{
    public static GameplayDialogueOverlay Instance { get; private set; }

    const int SortOrder = 420;
    const float DefaultDuration = 2.8f;

    GameObject _root;
    TextMeshProUGUI _speaker;
    TextMeshProUGUI _line;
    Coroutine _hideRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() => EnsureInstance();

    public static void EnsureInstance()
    {
        if (Instance != null) return;

        var go = new GameObject("_GameplayDialogueOverlay");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<GameplayDialogueOverlay>();
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

        _root.AddComponent<GraphicRaycaster>();

        var bar = new GameObject("Bar");
        bar.transform.SetParent(_root.transform, false);
        var barRt = bar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0.08f, 0.90f);
        barRt.anchorMax = new Vector2(0.92f, 0.98f);
        barRt.offsetMin = barRt.offsetMax = Vector2.zero;

        var bg = bar.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.08f, 0.58f);
        bg.raycastTarget = false;

        var speakerGo = new GameObject("Speaker");
        speakerGo.transform.SetParent(bar.transform, false);
        var speakerRt = speakerGo.AddComponent<RectTransform>();
        speakerRt.anchorMin = new Vector2(0.03f, 0.58f);
        speakerRt.anchorMax = new Vector2(0.97f, 0.96f);
        speakerRt.offsetMin = speakerRt.offsetMax = Vector2.zero;

        _speaker = speakerGo.AddComponent<TextMeshProUGUI>();
        TmpUiUtility.EnsureFont(_speaker);
        _speaker.fontSize = 22f;
        _speaker.fontStyle = FontStyles.Bold;
        _speaker.color = new Color(1f, 0.86f, 0.35f, 1f);
        _speaker.alignment = TextAlignmentOptions.Top;
        _speaker.raycastTarget = false;

        var lineGo = new GameObject("Line");
        lineGo.transform.SetParent(bar.transform, false);
        var lineRt = lineGo.AddComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0.03f, 0.08f);
        lineRt.anchorMax = new Vector2(0.97f, 0.62f);
        lineRt.offsetMin = lineRt.offsetMax = Vector2.zero;

        _line = lineGo.AddComponent<TextMeshProUGUI>();
        TmpUiUtility.EnsureFont(_line);
        _line.fontSize = 28f;
        _line.color = Color.white;
        _line.alignment = TextAlignmentOptions.Center;
        _line.textWrappingMode = TextWrappingModes.Normal;
        _line.raycastTarget = false;

        _root.SetActive(false);
    }

    public static void Show(string text, string speaker, float duration = DefaultDuration)
    {
        EnsureInstance();
        Instance.ShowInternal(text, speaker, duration);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.HideInternal();
    }

    void ShowInternal(string text, string speaker, float duration)
    {
        if (_line == null) return;

        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        bool hasSpeaker = !string.IsNullOrWhiteSpace(speaker);
        if (_speaker != null)
        {
            _speaker.gameObject.SetActive(hasSpeaker);
            if (hasSpeaker)
                TmpUiUtility.SetSafeText(_speaker, speaker);
        }

        TmpUiUtility.SetSafeText(_line, text ?? string.Empty);
        _root.SetActive(true);

        _hideRoutine = StartCoroutine(HideAfter(Mathf.Max(1.2f, duration)));
    }

    void HideInternal()
    {
        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        if (_root != null)
            _root.SetActive(false);
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        HideInternal();
    }
}
