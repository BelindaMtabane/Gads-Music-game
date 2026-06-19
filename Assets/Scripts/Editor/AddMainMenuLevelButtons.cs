using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds LEVEL 2 / LEVEL 3 shortcut buttons to StartScene main menu.
/// </summary>
public static class AddMainMenuLevelButtons
{
    static readonly (string name, string label, Vector2 min, Vector2 max)[] ButtonLayout =
    {
        ("PlayButton",      "PLAY",      new Vector2(0.28f, 0.647f), new Vector2(0.72f, 0.740f)),
        ("PlayLevel2Button", "LEVEL 2",  new Vector2(0.28f, 0.554f), new Vector2(0.72f, 0.647f)),
        ("PlayLevel3Button", "LEVEL 3",  new Vector2(0.28f, 0.461f), new Vector2(0.72f, 0.554f)),
        ("PauseButton",     "PAUSE",     new Vector2(0.28f, 0.368f), new Vector2(0.72f, 0.461f)),
        ("NarrativeButton", "NARRATIVE", new Vector2(0.28f, 0.275f), new Vector2(0.72f, 0.368f)),
        ("SettingsButton",  "SETTINGS",  new Vector2(0.28f, 0.182f), new Vector2(0.72f, 0.275f)),
    };

    [MenuItem("Tools/Add Main Menu Level Buttons")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/StartScene.unity", OpenSceneMode.Single);
        var menuPanel = FindInactiveByName("MenuPanel");
        if (menuPanel == null)
        {
            Debug.LogError("[AddMainMenuLevelButtons] MenuPanel not found.");
            return;
        }

        var play = menuPanel.Find("PlayButton");
        if (play == null)
        {
            Debug.LogError("[AddMainMenuLevelButtons] PlayButton not found.");
            return;
        }

        EnsureButton(menuPanel, play, "PlayLevel2Button");
        EnsureButton(menuPanel, play, "PlayLevel3Button");

        Button playBtn = null;
        Button level2Btn = null;
        Button level3Btn = null;
        Button pauseBtn = null;
        Button narrativeBtn = null;
        Button settingsBtn = null;

        for (int i = 0; i < ButtonLayout.Length; i++)
        {
            var (name, label, min, max) = ButtonLayout[i];
            var btnTr = menuPanel.Find(name);
            if (btnTr == null)
            {
                Debug.LogWarning($"[AddMainMenuLevelButtons] '{name}' not found — skipping.");
                continue;
            }

            SetAnchors(btnTr.gameObject, min, max);
            btnTr.SetSiblingIndex(i);
            SetLabel(btnTr, label);

            var btn = btnTr.GetComponent<Button>();
            switch (name)
            {
                case "PlayButton": playBtn = btn; break;
                case "PlayLevel2Button": level2Btn = btn; break;
                case "PlayLevel3Button": level3Btn = btn; break;
                case "PauseButton": pauseBtn = btn; break;
                case "NarrativeButton": narrativeBtn = btn; break;
                case "SettingsButton": settingsBtn = btn; break;
            }
        }

        var canvas = GameObject.Find("MainCanvas");
        var ui = canvas != null ? canvas.GetComponent<StartSceneUI>() : null;
        if (ui != null)
        {
            ui.menuPanel = menuPanel.gameObject;
            ui.playButton = playBtn;
            ui.level2Button = level2Btn;
            ui.level3Button = level3Btn;
            ui.pauseButton = pauseBtn;
            ui.narrativeButton = narrativeBtn;
            ui.settingsButton = settingsBtn;
            EditorUtility.SetDirty(ui);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AddMainMenuLevelButtons] LEVEL 2 and LEVEL 3 buttons added to StartScene.");
    }

    static void EnsureButton(Transform menuPanel, Transform template, string name)
    {
        if (menuPanel.Find(name) != null) return;
        var dup = Object.Instantiate(template.gameObject, menuPanel);
        dup.name = name;
    }

    static void SetAnchors(GameObject go, Vector2 min, Vector2 max)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        EditorUtility.SetDirty(go);
    }

    static void SetLabel(Transform btn, string text)
    {
        var label = btn.Find("Label");
        if (label == null) return;
        var tmp = label.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.text = text;
        tmp.fontSize = 40;
        EditorUtility.SetDirty(tmp);
    }

    static Transform FindInactiveByName(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go.transform;
        return null;
    }
}
