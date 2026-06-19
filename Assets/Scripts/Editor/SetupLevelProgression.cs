using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sets up Level 1 → 2 → 3 progression, cutscenes, and build settings.
/// Run via Tools → Setup Level Progression (All Levels).
/// </summary>
public static class SetupLevelProgression
{
    const string L1Path = "Assets/Scenes/MainGameL1.unity";
    const string L2Path = "Assets/Scenes/MainGameL2.unity";
    const string L3Path = "Assets/Scenes/MainGameL3.unity";
    const string L2IntroPath = "Assets/Scenes/Level2IntroScene.unity";
    const string L3IntroPath = "Assets/Scenes/Level3IntroScene.unity";

    [MenuItem("Tools/Setup Level Progression (All Levels)")]
    public static void SetupAll()
    {
        EnsureSceneCopy(L1Path, L2Path, "MainGameL2");
        EnsureSceneCopy(L2Path, L3Path, "MainGameL3");

        WireGameplayScene(L1Path, 1);
        WireGameplayScene(L2Path, 2);
        WireGameplayScene(L3Path, 3);
        WireLevelIntro(L2IntroPath, 2, "MainGameL2", "START LEVEL 2");
        WireLevelIntro(L3IntroPath, 3, "MainGameL3", "START LEVEL 3");
        UpdateBuildScenes();
        EnsureAIDialogueInStartScene();

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[LevelProgression] All 3 levels ready.\n" +
                  "  Cutscenes: L1 Opera curtains (in-game) | L2 museum security door | L3 club disco ball\n" +
                  "  Flow: Start → L1 → L2 Intro → L2 → L3 Intro → L3 → Victory");
    }

    [MenuItem("Tools/Setup Level Progression (L1 + L2)")]
    public static void SetupL1L2() => SetupAll();

    static void EnsureSceneCopy(string source, string dest, string label)
    {
        if (File.Exists(dest))
        {
            Debug.Log($"[LevelProgression] {label} already exists.");
            return;
        }

        if (!File.Exists(source))
        {
            Debug.LogError($"[LevelProgression] Missing source scene: {source}");
            return;
        }

        if (!AssetDatabase.CopyAsset(source, dest))
        {
            Debug.LogError($"[LevelProgression] Failed to copy → {dest}");
            return;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[LevelProgression] Created {label}.");
    }

    static void WireGameplayScene(string scenePath, int level)
    {
        if (!File.Exists(scenePath)) return;

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.levelNumber = level;
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

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LevelProgression] Wired {scenePath} as level {level}.");
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

    static void UpdateBuildScenes()
    {
        string[] scenes =
        {
            "Assets/Scenes/SplashScene.unity",
            "Assets/Scenes/StartScene.unity",
            "Assets/Scenes/MainGameL1.unity",
            "Assets/Scenes/Level2IntroScene.unity",
            "Assets/Scenes/MainGameL2.unity",
            "Assets/Scenes/Level3IntroScene.unity",
            "Assets/Scenes/MainGameL3.unity",
            "Assets/Scenes/DeathScene.unity",
            "Assets/Scenes/VictoryScene.unity",
        };

        var build = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            build[i] = new EditorBuildSettingsScene(scenes[i], true);

        EditorBuildSettings.scenes = build;
        Debug.Log("[LevelProgression] Build settings updated for all levels.");
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
