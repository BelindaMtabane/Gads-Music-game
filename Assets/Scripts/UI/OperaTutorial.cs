using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Level 1 only. For the first 10 seconds after GO, cycles short play tips
/// without pausing the run.
/// </summary>
public class OperaTutorial : MonoBehaviour
{
    const float Duration = 10f;

    static readonly string[] Tips =
    {
        "You keep running.  A / D or the arrows change lanes.  Space jumps.  G ducks.",
        "Shoes speed you up.  Musical notes help you.  Headphones are your shields.",
        "Vibe rises the farther you run on this level.  Good pickups add score points on top.",
        "Each piano row has 2, 3, or 4 white lanes.  Walk any white one.  A red lane makes the guard faster.",
        "Collect 2 instruments and stay ahead of the guard.  A hit spends a headphone."
    };

    float _elapsed;
    TextMeshProUGUI _label;
    int _shown = -1;

    public static void Show()
    {
        if (Object.FindAnyObjectByType<OperaTutorial>() != null) return;

        var canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>()
                  ?? Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("OperaTutorial");
        go.transform.SetParent(canvas.transform, false);
        go.AddComponent<OperaTutorial>().Build(go);
    }

    void Build(GameObject go)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 28f);
        rt.sizeDelta = new Vector2(980f, 96f);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.02f, 0.04f, 0.82f);
        bg.raycastTarget = false;

        var textGo = new GameObject("Tip");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(18f, 8f);
        textRt.offsetMax = new Vector2(-18f, -8f);

        _label = textGo.AddComponent<TextMeshProUGUI>();
        _label.fontSize = 26f;
        _label.fontStyle = FontStyles.Bold;
        _label.alignment = TextAlignmentOptions.Center;
        _label.color = new Color(1f, 0.92f, 0.75f, 1f);
        _label.enableWordWrapping = true;
        _label.raycastTarget = false;
        TmpUiUtility.EnsureFont(_label);
        ApplyTip(0);
    }

    void Update()
    {
        if (!GameManager.GameStarted)
            return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= Duration)
        {
            Destroy(gameObject);
            return;
        }

        int index = Mathf.Clamp(Mathf.FloorToInt(_elapsed / Duration * Tips.Length), 0, Tips.Length - 1);
        ApplyTip(index);
    }

    void ApplyTip(int index)
    {
        if (_label == null || index == _shown) return;
        _shown = index;
        _label.text = Tips[index];
    }
}
