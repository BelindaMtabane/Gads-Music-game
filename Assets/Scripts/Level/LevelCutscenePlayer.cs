using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Full-screen UI cutscenes per level theme:
/// L1 Opera — red curtains part
/// L2 Museum — security slide doors open
/// L3 Club — disco ball drops and spins
/// </summary>
public static class LevelCutscenePlayer
{
    const int SortOrder = 500;

    public static IEnumerator PlayAndWait(LevelCutsceneType type)
    {
        if (type == LevelCutsceneType.None)
            yield break;

        var root = CreateOverlayRoot();
        PlayCutsceneAudio(type, 0);

        switch (type)
        {
            case LevelCutsceneType.OperaCurtain:
                yield return PlayOperaCurtain(root);
                break;
            case LevelCutsceneType.MuseumSecurityDoor:
                yield return PlayMuseumSecurityDoor(root);
                break;
            case LevelCutsceneType.ClubDiscoBall:
                yield return PlayClubDiscoBall(root);
                break;
        }

        Object.Destroy(root);
    }

    static GameObject CreateOverlayRoot()
    {
        var go = new GameObject("LevelCutsceneOverlay");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortOrder;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        var blocker = CreatePanel(go.transform, "Blocker", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 1f));
        blocker.transform.SetAsFirstSibling();
        return go;
    }

    static IEnumerator PlayOperaCurtain(GameObject root)
    {
        // Try to load the real curtain texture (copied to Resources by SetupCurtainResource editor script)
        var curtainTex = Resources.Load<Texture2D>("curtain_texture");

        // Left curtain — slides in from the left
        var left  = CreateCurtainPanel(root.transform, "CurtainLeft",  curtainTex, flipH: false);
        var right = CreateCurtainPanel(root.transform, "CurtainRight", curtainTex, flipH: true);

        var leftRt  = left.GetComponent<RectTransform>();
        var rightRt = right.GetComponent<RectTransform>();

        // Start off-screen
        leftRt.anchorMin  = new Vector2(-0.52f, 0f); leftRt.anchorMax  = new Vector2(-0.02f, 1f);
        rightRt.anchorMin = new Vector2(1.02f,  0f); rightRt.anchorMax = new Vector2(1.52f,  1f);

        // Slide in to cover screen
        yield return AnimateAnchors(leftRt,  leftRt.anchorMin,  new Vector2(0f,   0f), 0.75f);
        yield return AnimateAnchors(rightRt, rightRt.anchorMin, new Vector2(0.5f, 0f), 0.75f);
        rightRt.anchorMax = new Vector2(1f, 1f);

        PlayCutsceneAudio(LevelCutsceneType.OperaCurtain, 1);

        // Show "Level 1 — Opera House" title over the closed curtains
        var title = CreateTitleCard(root.transform, "LEVEL 1", "OPERA HOUSE");
        yield return FadeInGraphic(title.GetComponentsInChildren<Graphic>(), 0.35f);
        yield return new WaitForSecondsRealtime(1.1f);
        yield return FadeOutGraphics(title.GetComponentsInChildren<Graphic>(), 0.25f);

        PlayCutsceneAudio(LevelCutsceneType.OperaCurtain, 2);

        // Curtains part — slide off screen
        yield return AnimateAnchors(leftRt,  new Vector2(0f,   0f), new Vector2(-0.52f, 0f), 0.8f);
        yield return AnimateAnchors(rightRt, new Vector2(0.5f, 0f), new Vector2(1.02f,  0f), 0.8f);
        yield return FadeOut(root, 0.4f);
    }

    // Creates a curtain panel using RawImage so the Texture2D can be applied directly.
    // flipH mirrors the UV so left and right curtains look like a matching pair.
    static GameObject CreateCurtainPanel(Transform parent, string name, Texture2D tex, bool flipH)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        if (tex != null)
        {
            var raw = go.AddComponent<RawImage>();
            raw.texture = tex;
            // Single UV rect (no tiling) — avoids the visible seam mid-curtain.
            // Right panel is horizontally flipped so both sides mirror each other.
            raw.uvRect  = flipH ? new Rect(1f, 0f, -1f, 1f) : new Rect(0f, 0f, 1f, 1f);
            raw.raycastTarget = false;
        }
        else
        {
            // Fallback: plain dark red if texture not found
            var img = go.AddComponent<Image>();
            img.color = new Color(0.5f, 0.05f, 0.08f, 1f);
            img.raycastTarget = false;
        }
        return go;
    }

    // Creates a centred title card with a large level number and subtitle.
    static GameObject CreateTitleCard(Transform parent, string levelLine, string subtitleLine)
    {
        var card = new GameObject("TitleCard");
        card.transform.SetParent(parent, false);
        var cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.15f, 0.35f);
        cardRt.anchorMax = new Vector2(0.85f, 0.65f);
        cardRt.offsetMin = cardRt.offsetMax = Vector2.zero;

        // Semi-transparent dark band behind the text
        var bg = card.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        bg.raycastTarget = false;

        AddTitleText(card.transform, levelLine,   fontSize: 80, yAnchorMin: 0.52f, yAnchorMax: 0.95f,
                     color: new Color(1f, 0.92f, 0.55f, 1f));  // gold
        AddTitleText(card.transform, subtitleLine, fontSize: 44, yAnchorMin: 0.05f, yAnchorMax: 0.48f,
                     color: new Color(1f, 1f, 1f, 1f));
        return card;
    }

    static void AddTitleText(Transform parent, string text, int fontSize,
                             float yAnchorMin, float yAnchorMax, Color color)
    {
        var go = new GameObject("TitleText");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, yAnchorMin);
        rt.anchorMax = new Vector2(0.95f, yAnchorMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
    }

    static IEnumerator FadeInGraphic(Graphic[] graphics, float duration)
    {
        float t = 0f;
        foreach (var g in graphics) { var c = g.color; c.a = 0f; g.color = c; }
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            foreach (var g in graphics) { var c = g.color; c.a = p; g.color = c; }
            yield return null;
        }
    }

    static IEnumerator FadeOutGraphics(Graphic[] graphics, float duration)
    {
        float t = 0f;
        var startAlphas = new float[graphics.Length];
        for (int i = 0; i < graphics.Length; i++) startAlphas[i] = graphics[i].color.a;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            for (int i = 0; i < graphics.Length; i++)
            {
                var c = graphics[i].color;
                c.a = Mathf.Lerp(startAlphas[i], 0f, p);
                graphics[i].color = c;
            }
            yield return null;
        }
    }

    static IEnumerator PlayMuseumSecurityDoor(GameObject root)
    {
        var top = CreatePanel(root.transform, "DoorTop",
            new Vector2(0f, 0.5f), new Vector2(1f, 1.05f), new Color(0.35f, 0.38f, 0.42f, 1f));
        var bottom = CreatePanel(root.transform, "DoorBottom",
            new Vector2(0f, -0.05f), new Vector2(1f, 0.5f), new Color(0.28f, 0.31f, 0.35f, 1f));

        AddStripe(top.transform, new Color(0.9f, 0.75f, 0.2f, 0.85f));
        AddStripe(bottom.transform, new Color(0.9f, 0.75f, 0.2f, 0.85f));

        var topRt = top.GetComponent<RectTransform>();
        var botRt = bottom.GetComponent<RectTransform>();

        yield return AnimateAnchors(topRt, new Vector2(0f, 1.05f), new Vector2(1f, 0.5f), 0.85f);
        yield return AnimateAnchors(botRt, new Vector2(0f, -0.05f), new Vector2(1f, 0.5f), 0.85f);
        PlayCutsceneAudio(LevelCutsceneType.MuseumSecurityDoor, 1);
        yield return new WaitForSecondsRealtime(0.25f);

        PlayCutsceneAudio(LevelCutsceneType.MuseumSecurityDoor, 2);

        yield return AnimateAnchors(topRt, new Vector2(0f, 0.5f), new Vector2(1f, 1.08f), 0.7f);
        yield return AnimateAnchors(botRt, new Vector2(0f, 0.5f), new Vector2(1f, -0.08f), 0.7f);
        yield return FadeOut(root, 0.35f);
    }

    static IEnumerator PlayClubDiscoBall(GameObject root)
    {
        var ball = CreatePanel(root.transform, "DiscoBall",
            new Vector2(0.42f, 0.72f), new Vector2(0.58f, 0.88f), new Color(0.85f, 0.9f, 1f, 1f));
        var ballRt = ball.GetComponent<RectTransform>();
        PlayCutsceneAudio(LevelCutsceneType.ClubDiscoBall, 1);

        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI * 2f / 6f;
            var spark = CreatePanel(ball.transform, $"Spark{i}",
                new Vector2(0.45f, 0.45f), new Vector2(0.55f, 0.55f),
                Color.HSVToRGB((i * 0.17f) % 1f, 0.8f, 1f));
            var srt = spark.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(Mathf.Cos(a) * 80f, Mathf.Sin(a) * 80f);
        }

        float t = 0f;
        const float dropDuration = 1.4f;
        while (t < dropDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / dropDuration);
            ballRt.anchorMin = new Vector2(0.42f, Mathf.Lerp(1.05f, 0.38f, p));
            ballRt.anchorMax = new Vector2(0.58f, Mathf.Lerp(1.21f, 0.54f, p));
            ballRt.localRotation = Quaternion.Euler(0f, 0f, p * 720f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.4f);
        PlayCutsceneAudio(LevelCutsceneType.ClubDiscoBall, 2);

        t = 0f;
        while (t < 0.6f)
        {
            t += Time.unscaledDeltaTime;
            ballRt.localScale = Vector3.one * (1f + t * 0.5f);
            ballRt.localRotation = Quaternion.Euler(0f, 0f, 720f + t * 360f);
            yield return null;
        }

        yield return FadeOut(root, 0.45f);
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    static void AddStripe(Transform door, Color stripeColor)
    {
        var stripe = CreatePanel(door, "WarningStripe",
            new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f), stripeColor);
        stripe.transform.SetAsLastSibling();
    }

    static IEnumerator AnimateAnchors(RectTransform rt, Vector2 fromMin, Vector2 toMin, float duration)
    {
        Vector2 fromMax = rt.anchorMax;
        Vector2 toMax = new Vector2(toMin.x + (fromMax.x - fromMin.x), toMin.y + (fromMax.y - fromMin.y));
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            rt.anchorMin = Vector2.Lerp(fromMin, toMin, p);
            rt.anchorMax = Vector2.Lerp(fromMax, toMax, p);
            yield return null;
        }
        rt.anchorMin = toMin;
        rt.anchorMax = toMax;
    }

    static IEnumerator FadeOut(GameObject root, float duration)
    {
        var images = root.GetComponentsInChildren<Image>();
        float t = 0f;
        var startAlphas = new float[images.Length];
        for (int i = 0; i < images.Length; i++)
            startAlphas[i] = images[i].color.a;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            for (int i = 0; i < images.Length; i++)
            {
                var c = images[i].color;
                c.a = Mathf.Lerp(startAlphas[i], 0f, p);
                images[i].color = c;
            }
            yield return null;
        }
    }

    static void PlayCutsceneAudio(LevelCutsceneType type, int phase)
    {
        AudioManager.Instance?.PlayCutsceneStinger(type, phase);
    }
}
