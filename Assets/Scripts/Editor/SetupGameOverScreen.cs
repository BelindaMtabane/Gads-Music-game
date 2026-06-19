using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Applies the Game Over background art and light RESUME / EXIT button colors on DeathScene.
/// Run via Tools → Setup Game Over Screen.
/// </summary>
public class SetupGameOverScreen
{
    public const string GameOverBgPath = "Assets/UI/GameOver/game_over_background.png";

    private static readonly Color ButtonFill      = new Color(0.96f, 0.92f, 0.86f, 1f);
    private static readonly Color ButtonHighlight = new Color(1f, 0.98f, 0.94f, 1f);
    private static readonly Color ButtonPressed   = new Color(0.88f, 0.82f, 0.74f, 1f);
    private static readonly Color ButtonLabel     = new Color(0.25f, 0.18f, 0.12f, 1f);

    [MenuItem("Tools/Setup Game Over Screen")]
    public static void Setup()
    {
        EnsureTextureImport();

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/DeathScene.unity", OpenSceneMode.Single);
        var canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogError("[SetupGameOver] MainCanvas not found in DeathScene.");
            return;
        }

        FixCanvasScale(canvas);
        if (canvas.GetComponent<GameplayCanvasGuard>() == null)
            canvas.AddComponent<GameplayCanvasGuard>();

        ApplyBackground(canvas);
        LayoutActionButton(canvas, "RetryButton", "RESUME", new Vector2(0.08f, 0.12f), new Vector2(0.46f, 0.24f));
        LayoutActionButton(canvas, "MainMenuButton", "EXIT", new Vector2(0.54f, 0.12f), new Vector2(0.92f, 0.24f));

        var hud = canvas.GetComponent<DeathHUD>();
        if (hud != null)
        {
            hud.useBakedArt = true;
            EditorUtility.SetDirty(hud);
        }

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupGameOver] Game Over screen applied to DeathScene.");
        SetupScoreboardScreen.Setup();
    }

    private static void FixCanvasScale(GameObject canvas)
    {
        var rt = canvas.GetComponent<RectTransform>();
        if (rt != null)
            rt.localScale = Vector3.one;
    }

    private static void ApplyBackground(GameObject canvas)
    {
        var bg = canvas.transform.Find("Background");
        if (bg == null)
        {
            var go = new GameObject("Background");
            go.transform.SetParent(canvas.transform, false);
            go.AddComponent<Image>();
            bg = go.transform;
        }

        bg.SetAsFirstSibling();
        SetAnchors(bg.gameObject, Vector2.zero, Vector2.one);

        var img = bg.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = false;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GameOverBgPath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }
    }

    private static void LayoutActionButton(GameObject canvas, string name, string label, Vector2 min, Vector2 max)
    {
        var tr = canvas.transform.Find(name);
        if (tr == null) return;

        SetAnchors(tr.gameObject, min, max);
        ApplyLightButtonColors(canvas, name, label);
    }

    private static void ApplyLightButtonColors(GameObject canvas, string name, string label)
    {
        var tr = canvas.transform.Find(name);
        if (tr == null) return;

        var img = tr.GetComponent<Image>();
        if (img != null)
            img.color = ButtonFill;

        var btn = tr.GetComponent<Button>();
        if (btn != null)
        {
            var colors = btn.colors;
            colors.normalColor      = ButtonFill;
            colors.highlightedColor = ButtonHighlight;
            colors.pressedColor     = ButtonPressed;
            colors.selectedColor    = ButtonFill;
            btn.colors = colors;
        }

        var labelTmp = tr.GetComponentInChildren<TextMeshProUGUI>();
        if (labelTmp != null)
        {
            labelTmp.text = label;
            labelTmp.color = ButtonLabel;
            labelTmp.fontStyle = FontStyles.Bold;
        }
    }

    private static void EnsureTextureImport()
    {
        var importer = AssetImporter.GetAtPath(GameOverBgPath) as TextureImporter;
        if (importer == null) return;

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
