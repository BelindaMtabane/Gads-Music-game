using UnityEngine;

/// <summary>
/// Cross-scene session state for Rhythm Raiders: Beat Horizon level flow.
/// </summary>
public static class LevelProgress
{
    public const int MaxLevel = 3;

    public static int CurrentLevel { get; private set; } = 1;

    public static void SetLevel(int level)
    {
        CurrentLevel = Mathf.Clamp(level, 1, MaxLevel);
    }

    public static void ResetToFirstLevel()
    {
        CurrentLevel = 1;
    }

    public static void AdvanceLevel()
    {
        if (CurrentLevel < MaxLevel)
            CurrentLevel++;
    }

    public static string GetGameplayScene()
    {
        return LevelCatalog.GetGameplayScene(CurrentLevel);
    }

    public static string GetIntroSceneForLevel(int level)
    {
        return level switch
        {
            2 => "Level2IntroScene",
            3 => "Level3IntroScene",
            _ => "StartScene"
        };
    }
}
