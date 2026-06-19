using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Applies the Rhythm Raiders narrative background and text layout to all narration scenes.
/// Run via Tools → Setup Narration Background.
/// </summary>
public class SetupNarrationBackground
{
    public const string NARRATION_BG_PATH = "Assets/UI/Narration/narration_background.png";

    // White text box region on the narrative art (1920×1080 reference).
    private static readonly Vector2 TextAreaMin = new Vector2(0.11f, 0.33f);
    private static readonly Vector2 TextAreaMax = new Vector2(0.89f, 0.61f);

    [MenuItem("Tools/Setup Start Scene Narration")]
    public static void SetupStartSceneOnly()
    {
        EnsureTextureImport();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/StartScene.unity", OpenSceneMode.Single);
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogError("[SetupNarrationBackground] MainCanvas not found in StartScene.");
            return;
        }

        FixCanvasScale(canvas);
        var existing = canvas.transform.Find("NarrationPanel");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var panel = CreateNarrationPanel(canvas);
        ApplyPanelLayout(panel);

        var nm = canvas.GetComponent<NarrationManager>() ?? canvas.AddComponent<NarrationManager>();
        nm.narrationPanel = panel;
        nm.narrationText  = panel.transform.Find("NarrationText")?.GetComponent<TextMeshProUGUI>();
        nm.speakerText    = null;
        nm.autoAdvance    = true;
        nm.autoDelay      = 2.5f;
        nm.charDelay      = 0.035f;
        nm.advanceButton  = EnsureNarrationNextButton(panel.transform);

        var ui = canvas.GetComponent<StartSceneUI>();
        if (ui != null)
            ui.nextButton = nm.advanceButton;

        var menu = canvas.transform.Find("MenuPanel");
        if (menu != null)
            menu.gameObject.SetActive(false);

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupNarrationBackground] StartScene narration restored.");
    }

    [MenuItem("Tools/Setup Narration Background")]
    public static void Setup()
    {
        EnsureTextureImport();

        string current = EditorSceneManager.GetActiveScene().path;
        ApplyToScene("Assets/Scenes/StartScene.unity");
        ApplyToScene("Assets/Scenes/DeathScene.unity");
        ApplyToScene("Assets/Scenes/VictoryScene.unity");
        ApplyToScene(GameplaySceneNames.L1Path);

        if (!string.IsNullOrEmpty(current))
            EditorSceneManager.OpenScene(current);

        Debug.Log("[SetupNarrationBackground] Narration visuals applied to all scenes.");
    }

    private static void ApplyToScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var canvas = GameObject.Find("MainCanvas") ?? GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning($"[SetupNarrationBackground] No canvas in {path}");
            return;
        }

        FixCanvasScale(canvas);

        var panel = canvas.transform.Find("NarrationPanel");
        if (panel == null)
        {
            if (!path.Contains("StartScene"))
            {
                Debug.LogWarning($"[SetupNarrationBackground] No NarrationPanel in {path}");
                return;
            }
            panel = CreateNarrationPanel(canvas).transform;
        }

        ApplyPanelLayout(panel.gameObject);

        var nm = canvas.GetComponent<NarrationManager>() ?? canvas.AddComponent<NarrationManager>();
        nm.narrationPanel = panel.gameObject;
        nm.narrationText  = panel.Find("NarrationText")?.GetComponent<TextMeshProUGUI>();
        nm.speakerText    = panel.Find("SpeakerText")?.GetComponent<TextMeshProUGUI>();
        nm.autoAdvance    = true;
        nm.autoDelay      = 2.5f;
        nm.charDelay      = 0.035f;
        EditorUtility.SetDirty(nm);

        var next = EnsureNarrationNextButton(panel.transform);
        nm.advanceButton = next;

        if (path.Contains("StartScene"))
        {
            var menu = canvas.transform.Find("MenuPanel");
            if (menu != null)
                menu.gameObject.SetActive(false);

            var ui = canvas.GetComponent<StartSceneUI>();
            if (ui != null)
                ui.nextButton = next;
        }

        // Old full-canvas backgrounds are no longer used for narration overlays.
        var legacyBg = canvas.transform.Find("Background");
        if (legacyBg != null && path.Contains("StartScene"))
            legacyBg.gameObject.SetActive(true);

        var overlay = canvas.transform.Find("Overlay");
        if (overlay != null)
            overlay.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[SetupNarrationBackground] Updated {path}");
    }

    private static void ApplyPanelLayout(GameObject panel)
    {
        SetAnchors(panel, Vector2.zero, Vector2.one);

        var img = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = false;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(NARRATION_BG_PATH);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }

        var speaker = panel.transform.Find("SpeakerText");
        if (speaker != null)
            Object.DestroyImmediate(speaker.gameObject);

        var textTr = panel.transform.Find("NarrationText");
        GameObject textGo;
        if (textTr != null)
            textGo = textTr.gameObject;
        else
        {
            textGo = new GameObject("NarrationText");
            textGo.transform.SetParent(panel.transform, false);
            textGo.AddComponent<TextMeshProUGUI>();
        }

        SetAnchors(textGo, TextAreaMin, TextAreaMax);

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = "";
            tmp.fontSize = 30;
            tmp.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.raycastTarget = false;
            ApplyFont(tmp);
        }

        panel.SetActive(false);
        EditorUtility.SetDirty(panel);
    }

    private static readonly Vector2 NextButtonMin = new Vector2(0.70f, 0.04f);
    private static readonly Vector2 NextButtonMax = new Vector2(0.94f, 0.18f);

    public static Button EnsureNarrationNextButton(Transform panel)
    {
        var existing = panel.Find("NextButton");
        GameObject go;

        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject("NextButton");
            go.transform.SetParent(panel, false);
            go.AddComponent<Image>();
            go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.AddComponent<TextMeshProUGUI>();
        }

        SetAnchors(go, NextButtonMin, NextButtonMax);

        var btn = go.GetComponent<Button>();
        if (go.GetComponent<NarrationAdvanceButton>() == null)
            go.AddComponent<NarrationAdvanceButton>();

        var img = go.GetComponent<Image>();
        img.sprite = null;
        img.type = Image.Type.Simple;
        img.color = new Color(0.96f, 0.92f, 0.86f, 1f);
        img.raycastTarget = true;
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1f, 0.98f, 0.94f, 1f);
        colors.pressedColor     = new Color(0.88f, 0.82f, 0.74f, 1f);
        colors.selectedColor    = Color.white;
        btn.colors = colors;

        var label = go.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = "NEXT";
            label.fontSize = 28;
            label.fontStyle = FontStyles.Bold;
            label.color = new Color(0.25f, 0.18f, 0.12f, 1f);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            ApplyFont(label);

            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        }

        go.SetActive(false);
        return btn;
    }

    private static GameObject CreateNarrationPanel(GameObject canvas)
    {
        var panel = new GameObject("NarrationPanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<Image>();
        return panel;
    }

    private static void EnsureTextureImport()
    {
        var importer = AssetImporter.GetAtPath(NARRATION_BG_PATH) as TextureImporter;
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
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) return;
        tmp.font = font;
        if (font.material != null)
            tmp.fontSharedMaterial = font.material;
    }

    private static void FixCanvasScale(GameObject canvas)
    {
        var rt = canvas.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.localScale = Vector3.one;
        EditorUtility.SetDirty(rt);
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
