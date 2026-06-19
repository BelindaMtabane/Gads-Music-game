using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes StartScene MenuPanel — four evenly spaced buttons inside the orange menu box.
/// Run via Tools → Fix Button Layout (StartScene must be open).
/// </summary>
public static class FixButtonLayout
{
    // Four equal-height rows inside the orange panel on the menu art.
    static readonly (string name, Vector2 min, Vector2 max)[] ButtonLayout =
    {
        ("PlayButton",       new Vector2(0.28f, 0.647f), new Vector2(0.72f, 0.740f)),
        ("PlayLevel2Button", new Vector2(0.28f, 0.554f), new Vector2(0.72f, 0.647f)),
        ("PlayLevel3Button", new Vector2(0.28f, 0.461f), new Vector2(0.72f, 0.554f)),
        ("PauseButton",      new Vector2(0.28f, 0.368f), new Vector2(0.72f, 0.461f)),
        ("NarrativeButton",  new Vector2(0.28f, 0.275f), new Vector2(0.72f, 0.368f)),
        ("SettingsButton",   new Vector2(0.28f, 0.182f), new Vector2(0.72f, 0.275f)),
    };

    [MenuItem("Tools/Fix Button Layout")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            Debug.LogWarning("[FixButtonLayout] Open StartScene first.");
            return;
        }

        Transform menuPanel = FindInactiveByName("MenuPanel");
        if (menuPanel == null)
        {
            Debug.LogError("[FixButtonLayout] MenuPanel not found.");
            return;
        }

        Transform play = menuPanel.Find("PlayButton");
        if (play == null)
        {
            Debug.LogError("[FixButtonLayout] PlayButton not found.");
            return;
        }

        // Create NarrativeButton from Play if missing.
        if (menuPanel.Find("NarrativeButton") == null)
        {
            var dup = Object.Instantiate(play.gameObject, menuPanel);
            dup.name = "NarrativeButton";
        }

        if (menuPanel.Find("PlayLevel2Button") == null)
        {
            var dup = Object.Instantiate(play.gameObject, menuPanel);
            dup.name = "PlayLevel2Button";
        }

        if (menuPanel.Find("PlayLevel3Button") == null)
        {
            var dup = Object.Instantiate(play.gameObject, menuPanel);
            dup.name = "PlayLevel3Button";
        }

        for (int i = 0; i < ButtonLayout.Length; i++)
        {
            var (name, min, max) = ButtonLayout[i];
            var btn = menuPanel.Find(name);
            if (btn == null)
            {
                Debug.LogWarning($"[FixButtonLayout] '{name}' not found — skipping.");
                continue;
            }

            SetAnchors(btn.gameObject, min, max);
            btn.SetSiblingIndex(i);
            SetLabel(btn, name switch
            {
                "PlayButton" => "PLAY",
                "PlayLevel2Button" => "LEVEL 2",
                "PlayLevel3Button" => "LEVEL 3",
                "PauseButton" => "PAUSE",
                "NarrativeButton" => "NARRATIVE",
                "SettingsButton" => "SETTINGS",
                _ => name
            });
        }

        var narrative = menuPanel.Find("NarrativeButton");
        var level2 = menuPanel.Find("PlayLevel2Button");
        var level3 = menuPanel.Find("PlayLevel3Button");
        var startSceneUI = FindInactive<StartSceneUI>();
        if (startSceneUI != null)
        {
            if (narrative != null)
            {
                var btn = narrative.GetComponent<Button>();
                if (btn != null)
                {
                    startSceneUI.narrativeButton = btn;
                    EditorUtility.SetDirty(startSceneUI);
                }
            }
            if (level2 != null)
            {
                var btn = level2.GetComponent<Button>();
                if (btn != null)
                {
                    startSceneUI.level2Button = btn;
                    EditorUtility.SetDirty(startSceneUI);
                }
            }
            if (level3 != null)
            {
                var btn = level3.GetComponent<Button>();
                if (btn != null)
                {
                    startSceneUI.level3Button = btn;
                    EditorUtility.SetDirty(startSceneUI);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixButtonLayout] Menu buttons laid out — PLAY, LEVEL 2, LEVEL 3, PAUSE, NARRATIVE, SETTINGS.");
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
        if (btn == null) return;
        var label = btn.Find("Label");
        if (label == null) return;
        var tmp = label.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            EditorUtility.SetDirty(tmp);
        }
    }

    static Transform FindInactiveByName(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go.transform;
        return null;
    }

    static T FindInactive<T>() where T : Component
    {
        foreach (var obj in Resources.FindObjectsOfTypeAll<T>())
            if (obj.gameObject.scene.isLoaded) return obj;
        return null;
    }
}
