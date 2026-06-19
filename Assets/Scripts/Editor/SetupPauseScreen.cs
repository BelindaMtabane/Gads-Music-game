using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Creates the pause menu UI with background art, buttons, and settings sliders.
/// Run via Tools → Setup Pause Screen.
/// </summary>
public static class SetupPauseScreen
{
    public const string PauseBgPath = "Assets/UI/Pause/pause_menu_background.png";

    private static readonly Color ButtonHitFill   = new Color(1f, 1f, 1f, 0.01f);
    private static readonly Color ButtonHighlight = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color ButtonPressed   = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color ButtonLabel     = new Color(0.02f, 0.52f, 0.35f, 1f);
    private static readonly Color SettingsFill    = new Color(0.96f, 0.92f, 0.86f, 1f);
    private static readonly Color SettingsLabel   = new Color(0.25f, 0.18f, 0.12f, 1f);

    // Full-screen art (1920×1080). Title is baked into the image.
    private static readonly Vector2 ResumeMin    = new Vector2(0.28f, 0.54f);
    private static readonly Vector2 ResumeMax    = new Vector2(0.72f, 0.66f);
    private static readonly Vector2 SettingsMin  = new Vector2(0.28f, 0.40f);
    private static readonly Vector2 SettingsMax  = new Vector2(0.72f, 0.52f);
    private static readonly Vector2 MainMenuMin  = new Vector2(0.28f, 0.26f);
    private static readonly Vector2 MainMenuMax  = new Vector2(0.72f, 0.38f);

    [MenuItem("Tools/Setup Pause Screen")]
    public static void Setup()
    {
        EnsureTextureImport();
        SetupScene(GameplaySceneNames.L1Path,  true,  true);
        SetupScene(GameplaySceneNames.L2Path,  true,  true);
        SetupScene(GameplaySceneNames.L3Path,  true,  true);
        SetupScene("Assets/Scenes/StartScene.unity", false, false);
        Debug.Log("[SetupPauseScreen] Pause UI applied to all gameplay scenes and StartScene.");
    }

    private static void SetupScene(string scenePath, bool requireGameStarted, bool showMainMenuButton)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogError($"[SetupPauseScreen] Canvas not found in {scenePath}");
            return;
        }

        if (scenePath.Contains("StartScene"))
            RemoveLegacyPauseUI(canvas);

        FixCanvasScale(canvas);
        FixCanvasScaler(canvas);
        EnsureGraphicRaycaster(canvas);
        EnsureEventSystem(scene);

        var pausePanel = EnsurePausePanel(canvas);
        EnsureBackground(pausePanel);

        var resumeBtn   = EnsureHitButton(pausePanel, "ResumeButton",   "RESUME",    ResumeMin, ResumeMax);
        var settingsBtn = EnsureHitButton(pausePanel, "SettingsButton", "SETTINGS",  SettingsMin, SettingsMax);
        var menuBtn     = EnsureHitButton(pausePanel, "MainMenuButton", "MAIN MENU", MainMenuMin, MainMenuMax);
        menuBtn.gameObject.SetActive(showMainMenuButton);

        var settingsPanel = EnsureSettingsPanel(canvas, out var closeBtn, out var musicSlider, out var sfxSlider);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        var hud = canvas.GetComponent<PauseMenuHUD>() ?? canvas.AddComponent<PauseMenuHUD>();
        hud.pausePanel = pausePanel;
        hud.resumeButton = resumeBtn;
        hud.settingsButton = settingsBtn;
        hud.mainMenuButton = menuBtn;
        hud.settingsPanel = settingsPanel;
        hud.settingsCloseButton = closeBtn;
        hud.musicVolumeSlider = musicSlider;
        hud.sfxVolumeSlider = sfxSlider;
        hud.pauseTimeOnOpen = true;
        hud.listenForEscape = true;
        hud.requireGameStarted = requireGameStarted;
        hud.showMainMenuButton = showMainMenuButton;

        var startUI = canvas.GetComponent<StartSceneUI>();
        if (startUI != null)
        {
            startUI.pauseMenuHud = hud;
            startUI.pausePanel = pausePanel;
            startUI.settingsPanel = settingsPanel;
            startUI.pauseResumeButton = resumeBtn;
            startUI.settingsCloseButton = closeBtn;
            startUI.musicVolumeSlider = musicSlider;
            startUI.sfxVolumeSlider = sfxSlider;
            EditorUtility.SetDirty(startUI);
        }

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject FindCanvas()
    {
        var canvas = GameObject.Find("MainCanvas") ?? GameObject.Find("Canvas");
        if (canvas != null) return canvas;

        var canvasComp = Object.FindAnyObjectByType<Canvas>();
        return canvasComp != null ? canvasComp.gameObject : null;
    }

    private static void RemoveLegacyPauseUI(GameObject canvas)
    {
        string[] legacy = { "PausePanel", "PauseMenuPanel", "SettingsPanel" };
        foreach (var name in legacy)
        {
            var t = canvas.transform.Find(name);
            if (t != null)
                Object.DestroyImmediate(t.gameObject);
        }

        var content = canvas.transform.Find("PauseMenuPanel/Content");
        if (content != null)
            Object.DestroyImmediate(content.parent.gameObject);
    }

    private static GameObject EnsurePausePanel(GameObject canvas)
    {
        var existing = canvas.transform.Find("PauseMenuPanel");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var panel = CreatePanel(canvas, "PauseMenuPanel", Vector2.zero, Vector2.one);
        panel.layer = 5;
        panel.transform.SetAsLastSibling();
        return panel;
    }

    private static void EnsureBackground(GameObject pausePanel)
    {
        var bg = pausePanel.transform.Find("Background");
        GameObject go;
        if (bg == null)
        {
            go = new GameObject("Background");
            go.transform.SetParent(pausePanel.transform, false);
        }
        else go = bg.gameObject;

        go.layer = 5;
        go.transform.SetAsFirstSibling();
        SetAnchors(go, Vector2.zero, Vector2.one);

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = false;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PauseBgPath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }
    }

    private static GameObject EnsureSettingsPanel(GameObject canvas, out Button closeBtn,
                                                  out Slider musicSlider, out Slider sfxSlider)
    {
        var existing = canvas.transform.Find("PauseSettingsPanel");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var panel = CreatePanel(canvas, "PauseSettingsPanel", Vector2.zero, Vector2.one);
        panel.layer = 5;
        var dim = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = true;

        var box = CreatePanel(panel, "SettingsBox", new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.74f));
        box.layer = 5;
        var boxImg = box.GetComponent<Image>() ?? box.AddComponent<Image>();
        boxImg.color = SettingsFill;

        var title = EnsureLabel(box, "Title", "SETTINGS", new Vector2(0.08f, 0.79f), new Vector2(0.92f, 0.94f), 46);
        title.color = SettingsLabel;

        var musicLabel = EnsureLabel(box, "MusicLabel", "Music Volume", new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.71f), 28);
        musicLabel.color = SettingsLabel;
        musicSlider = EnsureSlider(box, "MusicSlider", new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.60f));

        var sfxLabel = EnsureLabel(box, "SfxLabel", "SFX Volume", new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.45f), 28);
        sfxLabel.color = SettingsLabel;
        sfxSlider = EnsureSlider(box, "SfxSlider", new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.34f));

        closeBtn = EnsureSolidButton(box, "CloseButton", "CLOSE", new Vector2(0.30f, 0.07f), new Vector2(0.70f, 0.18f));
        return panel;
    }

    private static Button EnsureHitButton(GameObject parent, string name, string label, Vector2 min, Vector2 max)
    {
        var tr = parent.transform.Find(name);
        if (tr != null)
            Object.DestroyImmediate(tr.gameObject);

        var go = CreatePanel(parent, name, min, max);
        go.layer = 5;

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = ButtonHitFill;
        img.raycastTarget = true;

        var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = ButtonHitFill;
        colors.highlightedColor = ButtonHighlight;
        colors.pressedColor = ButtonPressed;
        colors.selectedColor = ButtonHitFill;
        btn.colors = colors;

        var labelGo = new GameObject("Label");
        labelGo.layer = 5;
        labelGo.transform.SetParent(go.transform, false);
        SetAnchors(labelGo, Vector2.zero, Vector2.one);

        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 40;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = ButtonLabel;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        ApplyFont(tmp);

        UIButtonRaycastFix.Apply(btn);
        return btn;
    }

    private static Button EnsureSolidButton(GameObject parent, string name, string label, Vector2 min, Vector2 max)
    {
        var go = CreatePanel(parent, name, min, max);
        go.layer = 5;

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = SettingsFill;
        img.raycastTarget = true;

        var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = SettingsFill;
        colors.highlightedColor = new Color(1f, 0.98f, 0.94f, 1f);
        colors.pressedColor = new Color(0.88f, 0.82f, 0.74f, 1f);
        btn.colors = colors;

        var labelGo = new GameObject("Label");
        labelGo.layer = 5;
        labelGo.transform.SetParent(go.transform, false);
        SetAnchors(labelGo, Vector2.zero, Vector2.one);

        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 34;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = SettingsLabel;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        ApplyFont(tmp);

        UIButtonRaycastFix.Apply(btn);
        return btn;
    }

    private static TextMeshProUGUI EnsureLabel(GameObject parent, string name, string text, Vector2 min, Vector2 max, int size)
    {
        var tr = parent.transform.Find(name);
        GameObject go;
        if (tr == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
        }
        else go = tr.gameObject;

        go.layer = 5;
        SetAnchors(go, min, max);
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        ApplyFont(tmp);
        return tmp;
    }

    private static Slider EnsureSlider(GameObject parent, string name, Vector2 min, Vector2 max)
    {
        var existing = parent.transform.Find(name);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var go = CreatePanel(parent, name, min, max);
        go.layer = 5;

        var bg = new GameObject("Background");
        bg.layer = 5;
        bg.transform.SetParent(go.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        var fillArea = new GameObject("Fill Area");
        fillArea.layer = 5;
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(8, 8);
        faRt.offsetMax = new Vector2(-8, -8);

        var fill = new GameObject("Fill");
        fill.layer = 5;
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = ButtonLabel;

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.layer = 5;
        handleArea.transform.SetParent(go.transform, false);
        var haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = haRt.offsetMax = Vector2.zero;

        var handle = new GameObject("Handle");
        handle.layer = 5;
        handle.transform.SetParent(handleArea.transform, false);
        var hRt = handle.AddComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(24, 24);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        return slider;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        SetAnchors(go, min, max);
        return go;
    }

    private static void EnsureTextureImport()
    {
        var importer = AssetImporter.GetAtPath(PauseBgPath) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    private static void ApplyFont(TextMeshProUGUI tmp)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Unity UI Samples/Fonts/Jupiter/Jupiter SDF.asset");
        if (font != null) tmp.font = font;
    }

    /// <summary>GraphicRaycaster is required for button clicks to register.</summary>
    private static void EnsureGraphicRaycaster(GameObject canvas)
    {
        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.AddComponent<GraphicRaycaster>();
            EditorUtility.SetDirty(canvas);
        }
    }

    /// <summary>EventSystem is required for any UI interaction.</summary>
    private static void EnsureEventSystem(UnityEngine.SceneManagement.Scene scene)
    {
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es != null) return;

        var go = new GameObject("EventSystem");
        UnityEditor.SceneManagement.EditorSceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        EditorUtility.SetDirty(go);
    }

    private static void FixCanvasScale(GameObject canvas)
    {
        var rt = canvas.GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;
    }

    private static void FixCanvasScaler(GameObject canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static void SetAnchors(GameObject go, Vector2 min, Vector2 max)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
