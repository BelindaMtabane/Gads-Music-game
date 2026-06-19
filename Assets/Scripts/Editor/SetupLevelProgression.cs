using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sets up Level 1 → 2 → 3 progression, syncs gameplay from L1, and build settings.
/// Run via Tools → Setup Level Progression (All Levels).
/// </summary>
public static class SetupLevelProgression
{
    const string L2IntroPath = "Assets/Scenes/Level2IntroScene.unity";
    const string L3IntroPath = "Assets/Scenes/Level3IntroScene.unity";

    [MenuItem("Tools/Setup Level Progression (All Levels)")]
    public static void SetupAll()
    {
        SyncGameplayFromL1();
        WireGameplayScene(GameplaySceneNames.L1Path, 1);
        WireGameplayScene(GameplaySceneNames.L2Path, 2);
        WireGameplayScene(GameplaySceneNames.L3Path, 3);
        WireLevelIntro(L2IntroPath, 2, GameplaySceneNames.L2Museum, "START LEVEL 2");
        WireLevelIntro(L3IntroPath, 3, GameplaySceneNames.L3Club, "START LEVEL 3");
        SetupBuildScenes.Setup();
        EnsureAIDialogueInStartScene();
        FixSceneReferencesInAssets();

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[LevelProgression] All 3 levels synced from L1 and wired.\n" +
                  "  L1 → " + GameplaySceneNames.L1Opera + "\n" +
                  "  L2 → " + GameplaySceneNames.L2Museum + "\n" +
                  "  L3 → " + GameplaySceneNames.L3Club);
    }

    [MenuItem("Tools/Sync Gameplay Scenes From L1")]
    public static void SyncGameplayFromL1()
    {
        if (!File.Exists(GameplaySceneNames.L1Path))
        {
            Debug.LogError("[LevelProgression] Missing " + GameplaySceneNames.L1Path);
            return;
        }

        ForceCopyScene(GameplaySceneNames.L1Path, GameplaySceneNames.L2Path);
        ForceCopyScene(GameplaySceneNames.L1Path, GameplaySceneNames.L3Path);
        AssetDatabase.Refresh();
        Debug.Log("[LevelProgression] Copied L1 gameplay/UI into L2 and L3 (textures unchanged — swap materials per level).");
    }

    static void ForceCopyScene(string source, string dest)
    {
        if (!File.Exists(source))
            return;

        File.Copy(source, dest, overwrite: true);
        Debug.Log($"[LevelProgression] {Path.GetFileName(source)} → {Path.GetFileName(dest)}");
    }

    static void WireGameplayScene(string scenePath, int level)
    {
        if (!File.Exists(scenePath)) return;

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var def = LevelCatalog.Get(level);

        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.levelNumber = level;
            gm.levelIntroSceneName = def.nextIntroScene;
            EditorUtility.SetDirty(gm);
        }

        var bootstrap = Object.FindAnyObjectByType<LevelBootstrap>();
        if (bootstrap == null && gm != null)
            bootstrap = gm.gameObject.AddComponent<LevelBootstrap>();
        if (bootstrap != null)
        {
            bootstrap.levelNumber = level;
            EditorUtility.SetDirty(bootstrap);
        }

        var enemy = Object.FindAnyObjectByType<EnemyBase>();
        if (enemy != null && enemy.GetComponent<GuardDialogueController>() == null)
            enemy.gameObject.AddComponent<GuardDialogueController>();

        FixGameplayCollision.FixOpenScene();

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LevelProgression] Wired {scenePath} as level {level} ({def.displayName}).");
    }

    static void WireLevelIntro(string introPath, int level, string gameplayScene, string buttonLabel)
    {
        if (!File.Exists(introPath)) return;

        var scene = EditorSceneManager.OpenScene(introPath, OpenSceneMode.Single);
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var hud = Object.FindAnyObjectByType<LevelIntroScreenHUD>();
        if (hud == null && canvas != null)
            hud = canvas.gameObject.AddComponent<LevelIntroScreenHUD>();

        if (hud != null)
        {
            hud.levelNumber = level;
            hud.gameplaySceneName = gameplayScene;

            if (hud.continueButton == null)
                hud.continueButton = CreateContinueButton(canvas, buttonLabel);
            else
                SetButtonLabel(hud.continueButton, buttonLabel);

            EditorUtility.SetDirty(hud);
        }

        EditorSceneManager.SaveScene(scene);
    }

    static Button CreateContinueButton(Canvas canvas, string label)
    {
        if (canvas == null) return null;

        var btnGo = new GameObject("ContinueButton");
        btnGo.transform.SetParent(canvas.transform, false);
        var rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.08f);
        rt.anchorMax = new Vector2(0.65f, 0.14f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = btnGo.AddComponent<Image>();
        img.color = new Color(0.15f, 0.55f, 0.95f, 0.95f);

        var btn = btnGo.AddComponent<Button>();
        var labelGo = new GameObject("Text");
        labelGo.transform.SetParent(btnGo.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }

    static void SetButtonLabel(Button btn, string label)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
    }

    static void FixSceneReferencesInAssets()
    {
        FixStartScene();
        FixDeathScene();
    }

    static void FixStartScene()
    {
        const string path = "Assets/Scenes/StartScene.unity";
        if (!File.Exists(path)) return;

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var ui = Object.FindAnyObjectByType<StartSceneUI>();
        if (ui != null)
        {
            ui.gameSceneName = GameplaySceneNames.L1Opera;
            EditorUtility.SetDirty(ui);
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void FixDeathScene()
    {
        const string path = "Assets/Scenes/DeathScene.unity";
        if (!File.Exists(path)) return;

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var hud = Object.FindAnyObjectByType<DeathHUD>();
        if (hud != null)
        {
            hud.gameSceneName = GameplaySceneNames.L1Opera;
            EditorUtility.SetDirty(hud);
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void EnsureAIDialogueInStartScene()
    {
        const string startPath = "Assets/Scenes/StartScene.unity";
        if (!File.Exists(startPath)) return;

        var scene = EditorSceneManager.OpenScene(startPath, OpenSceneMode.Single);
        if (Object.FindAnyObjectByType<AIDialogueBootstrap>() != null)
            return;

        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        canvas.gameObject.AddComponent<AIDialogueBootstrap>();
        EditorSceneManager.SaveScene(scene);
    }
}
