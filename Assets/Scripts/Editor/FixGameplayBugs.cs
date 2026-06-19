using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click fix for L1 gameplay bugs:
///  1. Obstacles solid, pickups triggers (see Fix Gameplay Collision)
///  2. Sets Ground tiles to layer 6 so GroundChain can track them
///  3. Disables instantKill on obstacle templates
///  4. Ensures enemy starts far enough back
/// Run via Tools → Fix Gameplay Bugs
/// </summary>
public static class FixGameplayBugs
{
    const int GroundLayer = 6;

    [MenuItem("Tools/Fix Gameplay Bugs")]
    public static void Run()
    {
        FixGameplayCollision.Run();
        int fixes = 0;

        string[] gameplayRoots = new[]
        {
            "Random Obstacle - health Decrease",
            "Slow down  - Obstacle",
            "Sneak - Pickup",
            "Speed boost - Pickup",
            "Jump Boost  - Pickup",
            "Health - Pickup",
            "Artifact - Goal",
        };

        foreach (var rootName in gameplayRoots)
        {
            var go = GameObject.Find(rootName);
            if (go == null) { Debug.LogWarning($"[FixGameplayBugs] '{rootName}' not found — skipping."); continue; }

            var hdo = go.GetComponent<HealthDecreaseObstacle>();
            if (hdo != null && hdo.instantKill)
            {
                hdo.instantKill = false;
                EditorUtility.SetDirty(hdo);
                fixes++;
                Debug.Log($"[FixGameplayBugs] Turned off instantKill on {rootName}");
            }
        }

        string[] groundNames = new[] { "Ground", "Ground (1)" };
        foreach (var gName in groundNames)
        {
            var go = GameObject.Find(gName);
            if (go == null) continue;
            if (go.layer != GroundLayer)
            {
                go.layer = GroundLayer;
                EditorUtility.SetDirty(go);
                fixes++;
                Debug.Log($"[FixGameplayBugs] Set '{gName}' to layer {GroundLayer}");
            }
        }

        var enemy = GameObject.Find("Enemy");
        if (enemy != null)
        {
            var pos = enemy.transform.position;
            if (pos.z > -18f)
            {
                pos.z = -20f;
                enemy.transform.position = pos;
                EditorUtility.SetDirty(enemy);
                fixes++;
                Debug.Log("[FixGameplayBugs] Moved Enemy to z=-20");
            }

            foreach (var col in enemy.GetComponentsInChildren<Collider>(true))
            {
                if (!col.isTrigger)
                {
                    col.isTrigger = true;
                    EditorUtility.SetDirty(col);
                    fixes++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixGameplayBugs] Done — {fixes} extra fixes applied, scene saved.");
    }
}
