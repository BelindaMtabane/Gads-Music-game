using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Scoreboard overlay shown from the Game Over screen (EXIT button).
/// Displays artifact count and total money value from the last run.
/// </summary>
public class ScoreboardHUD : MonoBehaviour
{
    [Header("UI")]
    public GameObject      panel;
    public TextMeshProUGUI scoreText;
    public Button          backButton;

    public void Show()
    {
        EnsurePanelExists();

        if (scoreText != null)
            scoreText.text = RunStats.FormatScoreSummary();

        if (panel != null)
            panel.SetActive(true);

        if (backButton != null)
        {
            UIButtonRaycastFix.Apply(backButton);
            UIButtonRaycastFix.BringToFront(backButton);
        }

        UIInputFix.EnsureEventSystem();
        UICursor.UnlockForMenu();
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void EnsurePanelExists()
    {
        if (panel != null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var panelGo = new GameObject("ScoreboardPanel");
        panelGo.transform.SetParent(canvas.transform, false);
        var rt = panelGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = panelGo.AddComponent<Image>();
        img.color = new Color(0.12f, 0.08f, 0.06f, 0.92f);
        img.raycastTarget = true;

        var textGo = new GameObject("ScoreText");
        textGo.transform.SetParent(panelGo.transform, false);
        var trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.18f, 0.30f);
        trt.anchorMax = new Vector2(0.82f, 0.68f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        scoreText = textGo.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 34;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.color = new Color(0.95f, 0.92f, 0.88f, 1f);
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.verticalAlignment = VerticalAlignmentOptions.Middle;
        scoreText.raycastTarget = false;

        var btnGo = new GameObject("BackButton");
        btnGo.transform.SetParent(panelGo.transform, false);
        var brt = btnGo.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.35f, 0.08f);
        brt.anchorMax = new Vector2(0.65f, 0.18f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.96f, 0.92f, 0.86f, 1f);
        backButton = btnGo.AddComponent<Button>();
        backButton.targetGraphic = btnImg;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);
        var lrt = labelGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var lbl = labelGo.AddComponent<TextMeshProUGUI>();
        lbl.text = "BACK";
        lbl.fontSize = 28;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = new Color(0.25f, 0.18f, 0.12f, 1f);
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.raycastTarget = false;

        panel = panelGo;
        panel.SetActive(false);
    }
}
