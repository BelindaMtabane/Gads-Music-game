/// <summary>
/// Canonical gameplay scene names and asset paths (renamed from MainGameL1/2/3).
/// </summary>
public static class GameplaySceneNames
{
    public const string L1Opera = "MainGameL1Opera";
    public const string L2Museum = "Level2Museum";
    public const string L3Club = "Level3UndergroundDance";

    public const string L1Path = "Assets/Scenes/MainGameL1Opera.unity";
    public const string L2Path = "Assets/Scenes/Level2Museum.unity";
    public const string L3Path = "Assets/Scenes/Level3UndergroundDance.unity";

    public static readonly string[] AllPaths = { L1Path, L2Path, L3Path };
    public static readonly string[] AllNames = { L1Opera, L2Museum, L3Club };

    public static bool IsGameplayScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        for (int i = 0; i < AllNames.Length; i++)
        {
            if (sceneName == AllNames[i]) return true;
        }
        return sceneName is "MainGameL1" or "MainGameL2" or "MainGameL3";
    }

    /// <summary>Maps a loaded gameplay scene name to level 1–3 (0 if unknown).</summary>
    public static int GetLevelNumber(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return 0;
        for (int i = 0; i < AllNames.Length; i++)
        {
            if (sceneName == AllNames[i])
                return i + 1;
        }
        if (sceneName == "MainGameL1") return 1;
        if (sceneName == "MainGameL2") return 2;
        if (sceneName == "MainGameL3") return 3;
        return 0;
    }
}
