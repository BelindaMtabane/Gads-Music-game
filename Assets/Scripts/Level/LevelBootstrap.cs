using UnityEngine;

/// <summary>
/// Applies per-level tuning when a gameplay scene loads.
/// Add to the GameManager object in MainGameL1 / MainGameL2.
/// </summary>
public class LevelBootstrap : MonoBehaviour
{
    [Tooltip("If 0, uses LevelProgress.CurrentLevel.")]
    public int levelNumber;

    public LevelDefinition definition;

    private void Awake()
    {
        int level = levelNumber > 0 ? levelNumber : LevelProgress.CurrentLevel;
        if (level <= 0) level = 1;

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (levelNumber <= 0 && sceneName.Contains("L3", System.StringComparison.OrdinalIgnoreCase))
            level = 3;
        else if (levelNumber <= 0 && sceneName.Contains("L2", System.StringComparison.OrdinalIgnoreCase))
            level = 2;
        else if (levelNumber <= 0 && sceneName.Contains("L1", System.StringComparison.OrdinalIgnoreCase))
            level = 1;

        definition = LevelCatalog.Get(level);
        LevelProgress.SetLevel(level);
        Apply(definition);
    }

    public void Apply(LevelDefinition def)
    {
        if (def == null) return;

        var movement = FindAnyObjectByType<PlayerMovement>();
        if (movement != null)
        {
            movement.baseForwardSpeed = def.forwardSpeed;
            movement.forwardSpeed     = def.forwardSpeed;
        }

        // Apply per-level health and hit budget
        var pickup = FindAnyObjectByType<PickupBase>();
        if (pickup != null)
        {
            pickup.maxHealth     = def.startHealth;
            pickup.currentHealth = def.startHealth;
            pickup.hitCounter    = def.hitCount;
        }

        var enemy = FindAnyObjectByType<EnemyBase>();
        if (enemy != null)
        {
            enemy.ApplyLevelTuning(def.guardSpeed, def.guardBoostSpeed);
            if (enemy.GetComponent<GuardDialogueController>() == null)
                enemy.gameObject.AddComponent<GuardDialogueController>();
        }

        var runLength = FindAnyObjectByType<RunLengthController>();
        if (runLength != null)
            runLength.lookAhead = def.runLookAhead;

        var spawner = GameObject.Find("spawnObjects")?.GetComponent<Spawner>();
        if (spawner == null)
            spawner = FindAnyObjectByType<Spawner>();
        if (spawner != null)
            spawner.spawnCount = def.spawnCount;

        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
            gm.ApplyLevelDefinition(def);

        ScaleArtifacts(def.artifactScale);
        ConfigureLevelTheme(def);
        ApplyEnvironment(def);
    }

    static void ApplyEnvironment(LevelDefinition def)
    {
        var gm = FindAnyObjectByType<GameManager>();
        if (gm == null) return;

        var ambience = gm.GetComponent<LevelAmbience>();
        if (ambience == null) ambience = gm.gameObject.AddComponent<LevelAmbience>();
        ambience.Apply(def);

        var dressing = gm.GetComponent<LevelSetDressing>();
        if (dressing == null) dressing = gm.gameObject.AddComponent<LevelSetDressing>();
        dressing.Apply(def);
    }

    static void ConfigureLevelTheme(LevelDefinition def)
    {
        var curtainSpawner = Object.FindAnyObjectByType<CurtainSpawnController>();
        if (curtainSpawner != null)
            curtainSpawner.enabled = def.spawnCurtains;

        var spawner = GameObject.Find("spawnObjects")?.GetComponent<Spawner>();
        if (spawner == null)
            spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner != null)
            spawner.curtainSpawnChance = def.spawnCurtains ? spawner.curtainSpawnChance : 0f;
    }

    static void ScaleArtifacts(float scale)
    {
        foreach (var tag in GameObject.FindGameObjectsWithTag("Artifact"))
        {
            if (tag == null) continue;
            tag.transform.localScale = Vector3.one * scale;
        }
    }
}
