using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires the NarrativeButton in StartScene and fixes the guard ground offset.
/// Run via Tools → Setup Narrative Button.
/// </summary>
public static class SetupNarrativeButton
{
    [MenuItem("Tools/Setup Narrative Button")]
    public static void Run()
    {
        // ── Find MenuPanel including inactive objects ───────────────────────────
        Transform menuPanel = FindInactiveByName("MenuPanel");
        if (menuPanel == null)
        {
            Debug.LogWarning("[SetupNarrativeButton] MenuPanel not found. Open StartScene first.");
            return;
        }

        // ── Find or create NarrativeButton under MenuPanel ─────────────────────
        Transform narrativeTf = menuPanel.Find("NarrativeButton");
        if (narrativeTf == null)
        {
            // Duplicate PlayButton as template
            Transform playBtn = menuPanel.Find("PlayButton");
            if (playBtn == null) { Debug.LogWarning("[SetupNarrativeButton] PlayButton not found."); return; }
            var dup = Object.Instantiate(playBtn.gameObject, menuPanel);
            dup.name = "NarrativeButton";
            narrativeTf = dup.transform;
        }

        // ── Position below SettingsButton ──────────────────────────────────────
        Transform settingsBtn = menuPanel.Find("SettingsButton");
        if (settingsBtn != null)
        {
            var settingsRt = settingsBtn.GetComponent<RectTransform>();
            var narrativeRt = narrativeTf.GetComponent<RectTransform>();
            if (settingsRt != null && narrativeRt != null)
            {
                narrativeRt.anchoredPosition = new Vector2(
                    settingsRt.anchoredPosition.x,
                    settingsRt.anchoredPosition.y - 157f);   // same spacing as other buttons
            }
        }

        // ── Set label text ─────────────────────────────────────────────────────
        var labelTf = narrativeTf.Find("Label");
        if (labelTf != null)
        {
            var tmp = labelTf.GetComponent<TextMeshProUGUI>();
            if (tmp != null) { tmp.text = "NARRATIVE"; EditorUtility.SetDirty(tmp); }
        }

        // ── Wire StartSceneUI.narrativeButton ──────────────────────────────────
        var startSceneUI = FindInactive<StartSceneUI>();
        if (startSceneUI != null)
        {
            var btn = narrativeTf.GetComponent<Button>();
            if (btn != null) { startSceneUI.narrativeButton = btn; EditorUtility.SetDirty(startSceneUI); }
            Debug.Log("[SetupNarrativeButton] StartSceneUI.narrativeButton wired.");
        }
        else
        {
            Debug.LogWarning("[SetupNarrativeButton] StartSceneUI not found — wire narrativeButton manually in Inspector.");
        }

        Debug.Log("[SetupNarrativeButton] NARRATIVE button set up.");

        // ── Save the scene ─────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SetupNarrativeButton] StartScene saved.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static Transform FindInactiveByName(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.scene.isLoaded && go.name == name)
                return go.transform;
        }
        return null;
    }

    static T FindInactive<T>() where T : Component
    {
        foreach (var obj in Resources.FindObjectsOfTypeAll<T>())
        {
            if (obj.gameObject.scene.isLoaded)
                return obj;
        }
        return null;
    }
}
