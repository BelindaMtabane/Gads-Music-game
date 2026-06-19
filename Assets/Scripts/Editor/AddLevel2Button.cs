using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds PLAY AGAIN, PLAY LEVEL 2, and MAIN MENU buttons to the VictoryScene.
/// Run via  Tools → Add Level 2 Button
/// </summary>
public static class AddLevel2Button
{
    [MenuItem("Tools/Add Level 2 Button")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().name != "VictoryScene")
        {
            Debug.LogWarning("[AddLevel2Button] Open VictoryScene first.");
            return;
        }

        // ── Find MainCanvas ────────────────────────────────────────────────────
        var canvasGO = GameObject.Find("MainCanvas");
        if (canvasGO == null)
        {
            // Fall back to any Canvas in scene
            var c = Object.FindAnyObjectByType<Canvas>();
            canvasGO = c != null ? c.gameObject : null;
        }
        if (canvasGO == null)
        {
            Debug.LogError("[AddLevel2Button] No Canvas found in VictoryScene.");
            return;
        }

        // ── Remove existing victory buttons if re-running ──────────────────────
        foreach (var name in new[] { "PlayAgainButton", "PlayLevel2Button", "MainMenuButton" })
        {
            var old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);
        }

        // ── Create a ButtonRow container ───────────────────────────────────────
        var rowGO = new GameObject("VictoryButtonRow");
        rowGO.transform.SetParent(canvasGO.transform, false);
        Undo.RegisterCreatedObjectUndo(rowGO, "Add Victory Buttons");

        var rowRT = rowGO.AddComponent<RectTransform>();
        // Anchor to center of screen, grow to fit children
        rowRT.anchorMin        = new Vector2(0.5f, 0.5f);
        rowRT.anchorMax        = new Vector2(0.5f, 0.5f);
        rowRT.pivot            = new Vector2(0.5f, 0.5f);
        rowRT.anchoredPosition = new Vector2(0f, -120f); // slightly below centre
        rowRT.sizeDelta        = new Vector2(340f, 310f); // wide enough for buttons + spacing

        var vLayout = rowGO.AddComponent<VerticalLayoutGroup>();
        vLayout.childAlignment        = TextAnchor.MiddleCenter;
        vLayout.spacing               = 20f;
        vLayout.childControlWidth     = true;
        vLayout.childControlHeight    = false;
        vLayout.childForceExpandWidth = true;
        vLayout.padding               = new RectOffset(0, 0, 0, 0);

        // ── Create the three buttons ───────────────────────────────────────────
        var playAgain  = MakeButton(rowGO.transform, "PlayAgainButton",  "PLAY AGAIN",  new Color(0.18f, 0.72f, 0.18f));
        var playLevel2 = MakeButton(rowGO.transform, "PlayLevel2Button", "PLAY LEVEL 2", new Color(0.95f, 0.72f, 0.05f));
        var mainMenu   = MakeButton(rowGO.transform, "MainMenuButton",   "MAIN MENU",   new Color(0.29f, 0.29f, 0.82f));

        // ── Wire onClick listeners via persistent handler components ───────────
        WirePlayAgain(playAgain);
        WirePlayLevel2(playLevel2);
        WireMainMenu(mainMenu);

        // ── Wire to VictoryHUD if present ──────────────────────────────────────
        var hud = Object.FindAnyObjectByType<VictoryHUD>();
        if (hud != null)
        {
            Undo.RecordObject(hud, "Wire VictoryHUD buttons");
            hud.playAgainButton = playAgain;
            hud.nextButton      = playLevel2;
            hud.mainMenuButton  = mainMenu;
            EditorUtility.SetDirty(hud);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[AddLevel2Button] Done — PLAY AGAIN / PLAY LEVEL 2 / MAIN MENU buttons added and scene saved.");
    }

    // ── Helper: create a styled TMP button ────────────────────────────────────
    static Button MakeButton(Transform parent, string goName, string label, Color color)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280f, 80f);

        var img = go.AddComponent<Image>();
        img.color = color;

        var btn = go.AddComponent<Button>();

        // Normal/highlighted/pressed colours
        var colors     = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor     = color * 0.75f;
        colors.selectedColor    = color;
        btn.colors = colors;

        // Text child
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);

        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin        = Vector2.zero;
        txtRT.anchorMax        = Vector2.one;
        txtRT.offsetMin        = Vector2.zero;
        txtRT.offsetMax        = Vector2.zero;

        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 28f;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    // ── Wire helpers ──────────────────────────────────────────────────────────
    static void WirePlayAgain(Button btn)
    {
        var h = btn.gameObject.GetComponent<VictoryPlayAgainHandler>()
             ?? btn.gameObject.AddComponent<VictoryPlayAgainHandler>();
        btn.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, h.OnClick);
        EditorUtility.SetDirty(btn);
    }

    static void WirePlayLevel2(Button btn)
    {
        var h = btn.gameObject.GetComponent<Level2ButtonHandler>()
             ?? btn.gameObject.AddComponent<Level2ButtonHandler>();
        btn.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, h.GoToLevel2);
        EditorUtility.SetDirty(btn);
    }

    static void WireMainMenu(Button btn)
    {
        var h = btn.gameObject.GetComponent<VictoryMainMenuHandler>()
             ?? btn.gameObject.AddComponent<VictoryMainMenuHandler>();
        btn.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, h.OnClick);
        EditorUtility.SetDirty(btn);
    }
}
