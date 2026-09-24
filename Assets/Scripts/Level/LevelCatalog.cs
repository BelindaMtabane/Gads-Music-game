using UnityEngine;

[System.Serializable]
public class LevelDefinition
{
    public int levelNumber = 1;
    public string displayName = "Opera House";
    public string codename = "Pulse Infiltration";
    public string gameplayScene = GameplaySceneNames.L1Opera;
    public string nextIntroScene = "Level2IntroScene";
    public string briefingTitle = "Opera House Briefing";

    public LevelCutsceneType gameplayCutscene = LevelCutsceneType.None;
    public LevelCutsceneType introCutscene = LevelCutsceneType.None;

    [TextArea(2, 4)]
    public string briefingLine1 = "Target artifact confirmed beneath the stage.";
    [TextArea(2, 4)]
    public string briefingLine2 = "The guards respond to movement and sound. Stay sharp.";

    [TextArea(2, 4)]
    public string startHint1 = "Run! Collect two instruments before the guard catches you!";
    [TextArea(2, 4)]
    public string startHint2 = "Jump obstacles and grab power-ups along the way.";

    // ── Movement ──────────────────────────────────────────────────────────────
    public float forwardSpeed    = 5.5f;
    public float guardSpeed      = 3f;
    public float guardBoostSpeed = 22f;
    public float runLookAhead    = 120f;

    // ── Spawning ──────────────────────────────────────────────────────────────
    public int   spawnCount      = 4;
    public int   artifactsToWin  = 2;
    /// <summary>HUD name for this level's goal item. The count format stays the same.</summary>
    public string collectibleName = "Artifacts";
    public float artifactScale   = 2.0f;
    public bool  spawnCurtains   = true;
    /// <summary>Spawn a guaranteed artifact on every Nth ground segment. 1 = every segment.</summary>
    public int   artifactEverySegments = 1;

    // ── Difficulty ────────────────────────────────────────────────────────────
    /// <summary>Headphone shields the player starts with. Each hit spends one.</summary>
    public int   hitCount        = 10;
    /// <summary>Player starting health for this level.</summary>
    public int   startHealth     = 100;
}

public static class LevelCatalog
{
    // ── L1: Opera House — Easy ────────────────────────────────────────────────
    // Low speed, few obstacles, all curtains, big visible artifacts.
    // Guard is closer in speed ratio; players learn the controls.
    static readonly LevelDefinition Level1 = new LevelDefinition
    {
        levelNumber      = 1,
        displayName      = "Opera House",
        codename         = "The Red Curtain Run",
        gameplayScene    = "MainGameL1Opera",
        nextIntroScene   = "Level2IntroScene",
        briefingTitle    = "Opera House — Level 1",
        gameplayCutscene = LevelCutsceneType.OperaCurtain,
        introCutscene    = LevelCutsceneType.None,
        briefingLine1    = "The Conductors hid a legendary instrument behind the red curtains.",
        briefingLine2    = "Two artifacts are hidden in the hall. Collect both and escape the guard.",
        startHint1       = "Pulse, run to the rhythm. Two gold artifacts are on this stage.",
        startHint2       = "Each piano row leaves 2, 3, or 4 white lanes. Walk any white one. A red lane makes the guard faster.",
        collectibleName  = "Instruments",
        forwardSpeed     = 8.0f,
        guardSpeed       = 6.5f,
        guardBoostSpeed  = 12f,
        runLookAhead     = 140f,
        spawnCount       = 5,
        artifactsToWin   = 2,
        artifactScale    = 1f,
        spawnCurtains    = false,
        artifactEverySegments = 2,
        hitCount         = 10,       // Generous — easy mode
        startHealth      = 100,
    };

    // ── L2: Music Museum — Medium ─────────────────────────────────────────────
    // More speed, more obstacles, museum security door cutscene.
    // Guard pressure increases; rolling drums + alarms.
    static readonly LevelDefinition Level2 = new LevelDefinition
    {
        levelNumber      = 2,
        displayName      = "Music Museum",
        codename         = "Hall of Echoes",
        gameplayScene    = "Level2Museum",
        nextIntroScene   = "Level3IntroScene",
        briefingTitle    = "Music Museum — Level 2",
        gameplayCutscene = LevelCutsceneType.MuseumSecurityDoor,
        introCutscene    = LevelCutsceneType.None,
        briefingLine1    = "Lasers, piano keys, and guitar strings fill the museum hall.",
        briefingLine2    = "Collect five Oscar awards and get out before lockdown.",
        startHint1       = "Jump the red lasers. Press G to dodge under the guitar strings.",
        startHint2       = "Five awards win the level. White piano lanes are safe. Red lanes make the guard faster.",
        forwardSpeed     = 10.0f,    // Noticeably faster than L1
        guardSpeed       = 8.5f,
        guardBoostSpeed  = 16f,      // Surge: guard at 16, player at 10 = 6 m/s close rate
        runLookAhead     = 150f,
        spawnCount       = 8,        // Double the obstacles
        artifactsToWin   = 5,
        collectibleName  = "Oscars",
        artifactScale    = 1f,
        spawnCurtains    = false,
        hitCount         = 7,        // Fewer lives
        startHealth      = 100,
    };

    // ── L3: Underground Club — Hard ───────────────────────────────────────────
    // Fastest speed, most obstacles, disco ball cutscene, tightest guard gap.
    // Lasers and sound-wave traps demand fast reactions.
    static readonly LevelDefinition Level3 = new LevelDefinition
    {
        levelNumber      = 3,
        displayName      = "Underground Club",
        codename         = "Neon Beat Vault",
        gameplayScene    = "Level3UndergroundDance",
        nextIntroScene   = "VictoryScene",
        briefingTitle    = "Underground Club — Level 3",
        gameplayCutscene = LevelCutsceneType.ClubDiscoBall,
        introCutscene    = LevelCutsceneType.None,
        briefingLine1    = "The artifact is hidden beneath the main DJ stage.",
        briefingLine2    = "Collect nine vinyl records and escape before the club locks down.",
        startHint1       = "Press G to dodge the trumpet beams. A hit slows you down.",
        startHint2       = "Nine vinyl records win the level. Dancers can take one if you touch them.",
        forwardSpeed     = 13.0f,    // Fastest — hard to control by design
        guardSpeed       = 11.5f,
        guardBoostSpeed  = 22f,      // Surge: guard at 22, player at 13 = 9 m/s close rate (hard)
        runLookAhead     = 175f,
        spawnCount       = 13,       // Most obstacles
        artifactsToWin   = 9,
        collectibleName  = "Vinyls",
        artifactScale    = 1f,
        spawnCurtains    = false,
        hitCount         = 5,        // Very few lives — punishing
        startHealth      = 80,       // Lower HP — extra pressure
    };

    public static LevelDefinition Get(int level)
    {
        return level switch
        {
            3 => Level3,
            2 => Level2,
            _ => Level1
        };
    }

    public static string GetGameplayScene(int level)
    {
        return Get(level).gameplayScene;
    }
}
