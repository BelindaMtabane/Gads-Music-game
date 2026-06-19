using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pickups = triggers (collect on overlap). Obstacles = solid (player bumps).
/// Run via Tools → Fix Gameplay Collision on each MainGame scene.
/// </summary>
public static class FixGameplayCollision
{
    [MenuItem("Tools/Fix Gameplay Collision")]
    public static void Run()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!GameplaySceneNames.IsGameplayScene(sceneName))
        {
            Debug.LogWarning("[FixGameplayCollision] Open a gameplay scene (L1 Opera, L2 Museum, or L3 Club) first.");
            return;
        }

        int fixes = FixOpenScene();
        Debug.Log($"[FixGameplayCollision] Done — {fixes} fixes applied in {sceneName}.");
    }

    public static int FixOpenScene()
    {
        int fixes = 0;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            fixes += FixHierarchy(root);

        var player = GameObject.Find("Player");
        if (player != null)
            fixes += FixPlayerPickupTrigger(player);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return fixes;
    }

    [MenuItem("Tools/Fix Gameplay Collision (All Levels)")]
    public static void RunAllLevels()
    {
        int total = 0;
        foreach (var path in GameplaySceneNames.AllPaths)
        {
            if (!System.IO.File.Exists(path)) continue;
            EditorSceneManager.OpenScene(path);
            total += FixOpenScene();
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log($"[FixGameplayCollision] Updated {total} fix passes across gameplay scenes.");
    }

    static int FixHierarchy(GameObject root)
    {
        int fixes = 0;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            var go = t.gameObject;
            if (GameplayCollisionUtility.IsSolidObstacleRoot(go))
            {
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                {
                    if (col.isTrigger)
                    {
                        col.isTrigger = false;
                        EditorUtility.SetDirty(col);
                        fixes++;
                    }
                }

                GameplayCollisionUtility.EnsureKinematicRigidbody(go);
                fixes++;
                continue;
            }

            if (GameplayCollisionUtility.IsPickupRoot(go))
            {
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                {
                    if (!col.isTrigger)
                    {
                        col.isTrigger = true;
                        EditorUtility.SetDirty(col);
                        fixes++;
                    }
                }

                GameplayCollisionUtility.EnsureKinematicRigidbody(go);
                fixes++;
            }
        }

        return fixes;
    }

    static int FixPlayerPickupTrigger(GameObject player)
    {
        int fixes = 0;
        var cc = player.GetComponent<CharacterController>();
        var triggerGo = player.transform.Find("PickupTrigger");
        if (triggerGo == null)
        {
            triggerGo = new GameObject("PickupTrigger").transform;
            triggerGo.SetParent(player.transform, false);
            triggerGo.localPosition = Vector3.zero;
            fixes++;
        }

        var col = triggerGo.GetComponent<CapsuleCollider>();
        if (col == null) col = triggerGo.gameObject.AddComponent<CapsuleCollider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            fixes++;
        }

        if (cc != null)
        {
            col.center = cc.center;
            col.height = cc.height;
            col.radius = cc.radius + 0.35f;
            col.direction = 1;
        }

        if (triggerGo.GetComponent<PickupTriggerProxy>() == null)
        {
            triggerGo.gameObject.AddComponent<PickupTriggerProxy>();
            fixes++;
        }

        var rootCol = player.GetComponent<CapsuleCollider>();
        if (rootCol != null && rootCol.enabled)
        {
            rootCol.enabled = false;
            fixes++;
        }

        EditorUtility.SetDirty(triggerGo.gameObject);
        return fixes;
    }
}
