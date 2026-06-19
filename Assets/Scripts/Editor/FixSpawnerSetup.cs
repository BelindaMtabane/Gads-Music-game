using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Wires up the Spawner system and fixes the duplicate-guard problem.
///
/// Run via  Tools → Fix Spawner Setup
///
/// Grayed-out pickups/obstacles in the Hierarchy are intentional inactive templates.
/// Clones are spawned at runtime on each ground tile.
/// </summary>
public static class FixSpawnerSetup
{
    static readonly string[] ObstaclePickupNames =
    {
        "Random Obstacle - health Decrease",
        "Slow down  - Obstacle",
        "Sneak - Pickup",
        "Speed boost - Pickup",
        "Jump Boost  - Pickup",
        "Health - Pickup",
    };

    const string ArtifactName = "Artifact - Goal";
    const string SpawnerObjectName = "spawnObjects";
    const string RunLengthControllerName = "RunLengthController";

    [MenuItem("Tools/Fix Spawner Setup")]
    public static void Run()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.StartsWith("MainGameL"))
        {
            Debug.LogWarning("[FixSpawnerSetup] Open MainGameL1, L2, or L3 first.");
            return;
        }

        int fixes = 0;

        var spawnerGo = GameObject.Find(SpawnerObjectName);
        if (spawnerGo == null)
        {
            Debug.LogError("[FixSpawnerSetup] spawnObjects not found.");
            return;
        }

        var spawner = spawnerGo.GetComponent<Spawner>();
        if (spawner == null)
        {
            spawner = spawnerGo.AddComponent<Spawner>();
            fixes++;
        }

        var rlcGo = GameObject.Find(RunLengthControllerName);
        if (rlcGo != null)
        {
            var rlc = rlcGo.GetComponent<RunLengthController>();
            if (rlc != null && rlc.spawner != spawner)
            {
                rlc.spawner = spawner;
                EditorUtility.SetDirty(rlc);
                fixes++;
            }

            var duplicate = rlcGo.GetComponent<Spawner>();
            if (duplicate != null && duplicate != spawner)
            {
                Object.DestroyImmediate(duplicate);
                fixes++;
                Debug.Log("[FixSpawnerSetup] Removed duplicate Spawner from RunLengthController.");
            }
        }

        var templates = new System.Collections.Generic.List<GameObject>();
        foreach (var name in ObstaclePickupNames)
        {
            var go = FindAnyInScene(name);
            if (go != null)
                templates.Add(go);
            else
                Debug.LogWarning($"[FixSpawnerSetup] Template '{name}' not found — skipping.");
        }

        spawner.spawnObjects = templates.ToArray();
        EditorUtility.SetDirty(spawner);
        fixes++;

        var artifact = FindAnyInScene(ArtifactName);
        if (artifact != null)
        {
            spawner.artifactTemplate = artifact;
            EditorUtility.SetDirty(spawner);
            fixes++;
        }

        foreach (var t in templates)
            PrepareTemplate(t);
        if (artifact != null)
            PrepareTemplate(artifact);

        var emGo = GameObject.Find("enemyManager");
        if (emGo != null)
        {
            var em = emGo.GetComponent<EnemyManager>();
            if (em != null && em.enabled)
            {
                em.enabled = false;
                EditorUtility.SetDirty(em);
                fixes++;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixSpawnerSetup] Done — {fixes} fixes applied. Use Tools → Show Spawn Templates (Edit Scene) to see and resize them.");
    }

    static void PrepareTemplate(GameObject template)
    {
        if (template == null) return;

        foreach (var renderer in template.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = true;
        foreach (var col in template.GetComponentsInChildren<Collider>(true))
            col.enabled = true;

        if (template.activeSelf)
        {
            template.SetActive(false);
            EditorUtility.SetDirty(template);
        }
    }

    static GameObject FindAnyInScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go;
        return null;
    }
}
