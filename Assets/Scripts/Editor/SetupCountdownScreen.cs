using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds the LVL1 theater countdown background and positions countdown text in the orange panel.
/// Run via Tools → Setup Countdown Screen.
/// </summary>
public class SetupCountdownScreen
{
    public const string CountdownBgPath = "Assets/UI/Countdown/lvl1_countdown_background.png";

    // Orange countdown panel on the art.
    private static readonly Vector2 CountdownTextMin = new Vector2(0.20f, 0.40f);
    private static readonly Vector2 CountdownTextMax = new Vector2(0.80f, 0.52f);

    [MenuItem("Tools/Setup Countdown Screen")]
    public static void Setup()
    {
        EnsureTextureImport();

        var scene = EditorSceneManager.OpenScene(GameplaySceneNames.L1Path, OpenSceneMode.Single);
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[SetupCountdown] Canvas not found in MainGameL1.");
            return;
        }

        var overlay = EnsureOverlay(canvas);
        var countdownText = EnsureCountdownText(overlay);

        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.countdownOverlay = overlay;
            gm.countdownText = countdownText;
            EditorUtility.SetDirty(gm);
        }

        overlay.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupCountdown] LVL1 countdown screen wired in MainGameL1.");
    }

    private static GameObject EnsureOverlay(GameObject canvas)
    {
        var existing = canvas.transform.Find("CountdownOverlay");
        GameObject overlay;

        if (existing != null)
        {
            overlay = existing.gameObject;
        }
        else
        {
            overlay = new GameObject("CountdownOverlay");
            overlay.transform.SetParent(canvas.transform, false);
            overlay.AddComponent<Image>();
        }

        SetAnchors(overlay, Vector2.zero, Vector2.one);

        var img = overlay.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = false;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CountdownBgPath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
        }

        overlay.transform.SetAsLastSibling();
        return overlay;
    }

    private static TextMeshProUGUI EnsureCountdownText(GameObject overlay)
    {
        var existing = overlay.transform.Find("CountdownText");
        GameObject textGo;

        if (existing != null)
        {
            textGo = existing.gameObject;
            textGo.transform.SetParent(overlay.transform, false);
        }
        else
        {
            var sceneText = GameObject.Find("Canvas/CountdownText");
            if (sceneText != null)
            {
                textGo = sceneText;
                textGo.transform.SetParent(overlay.transform, false);
                textGo.name = "CountdownText";
            }
            else
            {
                textGo = new GameObject("CountdownText");
                textGo.transform.SetParent(overlay.transform, false);
                textGo.AddComponent<TextMeshProUGUI>();
            }
        }

        SetAnchors(textGo, CountdownTextMin, CountdownTextMax);

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "5";
        tmp.fontSize = 96;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.95f, 0.45f, 0.05f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = new Color(0.2f, 0.08f, 0f, 1f);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Unity UI Samples/Fonts/Jupiter/Jupiter SDF.asset");
        if (font != null) tmp.font = font;

        textGo.SetActive(true);
        return tmp;
    }

    private static void EnsureTextureImport()
    {
        var importer = AssetImporter.GetAtPath(CountdownBgPath) as TextureImporter;
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
