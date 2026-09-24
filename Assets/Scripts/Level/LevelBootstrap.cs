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
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int level = levelNumber > 0 ? levelNumber : LevelProgress.CurrentLevel;
        if (level <= 0)
            level = GameplaySceneNames.GetLevelNumber(sceneName);
        if (level <= 0)
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
            pickup.maxHeadphones = def.hitCount;
            pickup.hitCounter    = def.hitCount;
        }

        var enemy = FindAnyObjectByType<EnemyBase>();
        if (enemy != null)
        {
            enemy.ApplyLevelTuning(def.guardSpeed, def.guardBoostSpeed);
            if (enemy.GetComponent<GuardDialogueController>() == null)
                enemy.gameObject.AddComponent<GuardDialogueController>();
            if (def.levelNumber == 1)
                EnsureGuardReadabilityLight(enemy.transform);
        }

        var runLength = FindAnyObjectByType<RunLengthController>();
        if (runLength != null)
            runLength.lookAhead = def.runLookAhead;

        var spawner = GameObject.Find("spawnObjects")?.GetComponent<Spawner>();
        if (spawner == null)
            spawner = FindAnyObjectByType<Spawner>();
        if (spawner != null)
        {
            spawner.spawnCount = def.spawnCount;
            spawner.artifactSegmentInterval = Mathf.Max(1, def.artifactEverySegments);
            if (def.levelNumber >= 2)
                spawner.artifactsPerSegment = 2;
            if (def.levelNumber == 1)
                TrumpetVisual.AttachTo(spawner.artifactTemplate);
            else if (def.levelNumber == 2)
                OscarVisual.AttachTo(spawner.artifactTemplate);
            else if (def.levelNumber == 3)
                VinylVisual.AttachTo(spawner.artifactTemplate);
        }

        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
            gm.ApplyLevelDefinition(def);

        ScaleArtifacts(def.artifactScale);
        ConfigureLevelTheme(def);
        ApplyEnvironment(def);
        BeatPulseVisual.BindLoadedObjects();
        DrumRollToCurtain.BindLoaded();
        if (def.levelNumber == 3)
            ClubMicrophone.ReplaceLoadedDrums();
        OpeningCurtain.BindAll();
        PickupPresentation.Reveal();
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
        {
            spawner.curtainSpawnChance = def.spawnCurtains ? spawner.curtainSpawnChance : 0f;

            if (!def.spawnCurtains)
            {
                // Remove any curtain prefabs from the inline spawn pool so they never
                // appear randomly on-track during L2 / L3 runs.
                var clean = new System.Collections.Generic.List<GameObject>(
                    spawner.spawnObjects ?? System.Array.Empty<GameObject>());
                clean.RemoveAll(go => go != null
                    && (go.CompareTag("Curtain") || go.name.StartsWith("Curtain")));
                spawner.spawnObjects = clean.ToArray();

                // Also clear the curtain prefab so TrySpawnSideCurtain never fires.
                spawner.curtainPrefab = null;
            }
        }
    }

    static void EnsureGuardReadabilityLight(Transform guard)
    {
        if (guard.Find("GuardSpotlight") != null) return;

        var lightGo = new GameObject("GuardSpotlight");
        lightGo.transform.SetParent(guard, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.4f, -0.6f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 8f;
        light.intensity = 1.7f;
        light.color = new Color(0.72f, 0.12f, 0.16f);
    }

    static void ScaleArtifacts(float scale)
    {
        var spawner = GameObject.Find("spawnObjects")?.GetComponent<Spawner>()
                      ?? Object.FindAnyObjectByType<Spawner>();
        if (spawner != null && spawner.artifactTemplate != null)
            spawner.artifactTemplate.transform.localScale = Vector3.one * scale;

        foreach (var tag in GameObject.FindGameObjectsWithTag("Artifact"))
        {
            if (tag == null) continue;
            tag.transform.localScale = Vector3.one * scale;
        }
    }
}
