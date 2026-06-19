using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates standalone Level 2 and Level 3 intro preview scenes (not in build flow yet).
/// Run via Tools → Setup Level Intro Screens.
/// </summary>
public static class SetupLevelIntroScreens
{
    public const string Lvl2BgPath   = "Assets/UI/LevelIntro/lvl2_intro_background.png";
    public const string Lvl3BgPath   = "Assets/UI/LevelIntro/lvl3_intro_background.png";
    public const string Lvl2ScenePath = "Assets/Scenes/Level2IntroScene.unity";
    public const string Lvl3ScenePath = "Assets/Scenes/Level3IntroScene.unity";

    // Orange content panels on the art (1920×1080 reference).
    private static readonly Vector2 Lvl2ContentMin = new Vector2(0.14f, 0.22f);
    private static readonly Vector2 Lvl2ContentMax = new Vector2(0.86f, 0.58f);
    private static readonly Vector2 Lvl3ContentMin = new Vector2(0.10f, 0.08f);
    private static readonly Vector2 Lvl3ContentMax = new Vector2(0.90f, 0.52f);

    [MenuItem("Tools/Setup Level Intro Screens")]
    public static void Setup()
    {
        EnsureTextureImport(Lvl2BgPath);
        EnsureTextureImport(Lvl3BgPath);

        BuildScene(Lvl2ScenePath, "Level2IntroScene", Lvl2BgPath, 2, Lvl2ContentMin, Lvl2ContentMax);
        BuildScene(Lvl3ScenePath, "Level3IntroScene", Lvl3BgPath, 3, Lvl3ContentMin, Lvl3ContentMax);

        Debug.Log("[SetupLevelIntro] Level 2 and Level 3 intro preview scenes created.\n" +
                  "  Open Assets/Scenes/Level2IntroScene.unity or Level3IntroScene.unity to preview.\n" +
                  "  Scenes are not added to Build Settings yet.");
    }

    private static void BuildScene(string scenePath, string sceneName, string bgPath,
                                   int levelNumber, Vector2 contentMin, Vector2 contentMax)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = sceneName;

        EnsureCamera();

        var canvas = CreateCanvas("MainCanvas");
        EnsureFader(canvas);

        var screenRoot = CreatePanel(canvas, "LevelIntroScreen", Vector2.zero, Vector2.one);
        screenRoot.layer = 5;

        var bg = CreatePanel(screenRoot, "Background", Vector2.zero, Vector2.one);
        bg.layer = 5;
        bg.transform.SetAsFirstSibling();
        ApplyBackground(bg, bgPath);

        var content = CreatePanel(screenRoot, "ContentPanel", contentMin, contentMax);
        content.layer = 5;
        var contentImg = content.GetComponent<Image>() ?? content.AddComponent<Image>();
        contentImg.color = new Color(1f, 1f, 1f, 0.01f);
        contentImg.raycastTarget = false;

        var hud = canvas.AddComponent<LevelIntroScreenHUD>();
        hud.levelNumber = levelNumber;

        EditorSceneManager.SaveScene(scene, scenePath);

        var canvasRt = canvas.GetComponent<RectTransform>();
        if (canvasRt != null)
        {
            canvasRt.localScale = Vector3.one;
            EditorUtility.SetDirty(canvasRt);
        }

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SetupLevelIntro] Saved {scenePath}");
    }

    private static void ApplyBackground(GameObject go, string bgPath)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = false;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }
        else
            Debug.LogWarning($"[SetupLevelIntro] Missing sprite: {bgPath}");
    }

    private static GameObject CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        return go;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        SetAnchors(go, min, max);
        return go;
    }

    private static void EnsureCamera()
    {
        var existing = GameObject.Find("Main Camera");
        if (existing == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camGo.AddComponent<AudioListener>();
            return;
        }

        var cam = existing.GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
    }

    private static void EnsureFader(GameObject canvas)
    {
        if (canvas.transform.Find("SceneFader") != null) return;
        var faderGO = new GameObject("SceneFader");
        faderGO.transform.SetParent(canvas.transform, false);
        faderGO.AddComponent<SceneFader>();
    }

    private static void EnsureTextureImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[SetupLevelIntro] Texture missing: {path}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
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
