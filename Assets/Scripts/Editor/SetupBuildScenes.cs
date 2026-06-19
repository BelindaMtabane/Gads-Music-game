using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Adds the game scenes to Build Settings in the correct order.
/// Run via  Tools → Setup Build Scenes.
/// Build order:
///   0 - SplashScene
///   1 - StartScene
///   2 - MainGameL1
///   3 - Level2IntroScene
///   4 - MainGameL2
///   5 - Level3IntroScene
///   6 - MainGameL3
///   7 - DeathScene
///   8 - VictoryScene
/// </summary>
public class SetupBuildScenes
{
    private static readonly string[] GameScenes = new string[]
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

    [MenuItem("Tools/Setup Build Scenes")]
    public static void Setup()
    {
        var scenes = new EditorBuildSettingsScene[GameScenes.Length];
        for (int i = 0; i < GameScenes.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(GameScenes[i], true);

        EditorBuildSettings.scenes = scenes;

        var splashScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenes[0]);
        if (splashScene != null)
            EditorSceneManager.playModeStartScene = splashScene;

        Debug.Log("[SetupBuild] Build Settings updated:\n" +
                  "  0 → SplashScene\n" +
                  "  1 → StartScene\n" +
                  "  2 → MainGameL1\n" +
                  "  3 → Level2IntroScene\n" +
                  "  4 → MainGameL2\n" +
                  "  5 → Level3IntroScene\n" +
                  "  6 → MainGameL3\n" +
                  "  7 → DeathScene\n" +
                  "  8 → VictoryScene");
    }
}
