using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fixes critical physics and placement issues in MainGame scenes:
///
///  1. Player CapsuleCollider → isTrigger = true
///  2. Ground tile scale → long enough to cover the player start (z ≈ 0)
///  3. Player / guard feet snapped onto the Ground layer via raycast
///  4. Enemy groundMask → Layer 6 ("Ground"), starting Z = -6
///
/// Run via  Tools → Fix Physics Setup
/// </summary>
public static class FixPhysicsSetup
{
    private static readonly Vector3 GroundPosition = new Vector3(0f, 0f, 34.5f);

    [MenuItem("Tools/Fix Physics Setup")]
    public static void Run()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.StartsWith("MainGame"))
        {
            Debug.LogWarning("[FixPhysicsSetup] Open a MainGame scene first.");
            return;
        }

        int fixes = 0;

        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            var rt = canvas.GetComponent<RectTransform>();
            if (rt != null && rt.localScale.sqrMagnitude < 0.001f)
            {
                rt.localScale = Vector3.one;
                EditorUtility.SetDirty(rt);
                fixes++;
                Debug.Log("[FixPhysicsSetup] Canvas scale reset to (1,1,1).");
            }
        }

        // ── Ground tile must span the player start at z = 0 ─────────────────
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            var gt = ground.transform;
            var scale = gt.localScale;
            bool scaleChanged = false;
            if (scale.z < 5f)
            {
                scale.z = 7f;
                scaleChanged = true;
            }
            if (scale.x < 2f)
            {
                scale.x = 2.5f;
                scaleChanged = true;
            }
            if (scaleChanged)
            {
                gt.localScale = scale;
                EditorUtility.SetDirty(gt);
                fixes++;
                Debug.Log($"[FixPhysicsSetup] Ground scale → {scale} so z=0 is on the floor.");
            }

            if (ground.GetComponent<SceneGroundAnchor>() == null)
            {
                ground.AddComponent<SceneGroundAnchor>();
                fixes++;
                Debug.Log("[FixPhysicsSetup] SceneGroundAnchor added to static Ground.");
            }

            if (Vector3.Distance(gt.position, GroundPosition) > 0.05f)
            {
                gt.position = GroundPosition;
                EditorUtility.SetDirty(gt);
                fixes++;
                Debug.Log("[FixPhysicsSetup] Ground position → (0, 0, 34.5).");
            }
        }
        else
        {
            Debug.LogWarning("[FixPhysicsSetup] Ground object not found.");
        }

        Physics.SyncTransforms();

        // ── 1. Player CapsuleCollider → trigger ──────────────────────────────
        // PickupBase.OnTriggerEnter fires when the Player's TRIGGER capsule overlaps
        // a pickup / obstacle collider. Without isTrigger=true nothing ever interacts.
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var caps = player.GetComponent<CapsuleCollider>();
            if (caps == null)
            {
                caps = player.AddComponent<CapsuleCollider>();
                Debug.Log("[FixPhysicsSetup] Added CapsuleCollider to Player.");
                fixes++;
            }

            if (!caps.isTrigger)
            {
                caps.isTrigger = true;
                EditorUtility.SetDirty(caps);
                fixes++;
                Debug.Log("[FixPhysicsSetup] Player CapsuleCollider → isTrigger = true");
            }
            else
            {
                Debug.Log("[FixPhysicsSetup] Player CapsuleCollider already a trigger — OK.");
            }

            // Make sure the capsule covers the player body (centre & size match CharacterController)
            var cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                bool changed = false;
                if (Vector3.Distance(caps.center, cc.center) > 0.05f)     { caps.center = cc.center;   changed = true; }
                if (Mathf.Abs(caps.height - cc.height)        > 0.05f)    { caps.height = cc.height;   changed = true; }
                if (Mathf.Abs(caps.radius - cc.radius)        > 0.05f)    { caps.radius = cc.radius;   changed = true; }
                if (changed) { EditorUtility.SetDirty(caps); fixes++; Debug.Log("[FixPhysicsSetup] Matched trigger capsule dimensions to CharacterController."); }
            }

            var pm = player.GetComponent<PlayerMovement>();
            float footOffset = pm != null ? pm.footGroundOffset : 0.08f;
            LayerMask groundMask = pm != null && pm.groundMask.value != 0
                ? pm.groundMask
                : GroundPlacementUtility.DefaultGroundMask;

            var pos = player.transform.position;
            pos.z = 0f;
            player.transform.position = pos;
            if (GroundPlacementUtility.SnapTransformFeetToGround(player.transform, groundMask, footOffset))
            {
                EditorUtility.SetDirty(player);
                fixes++;
                Debug.Log($"[FixPhysicsSetup] Player snapped to ground at {player.transform.position}.");
            }
        }
        else
        {
            Debug.LogError("[FixPhysicsSetup] Player not found.");
        }

        // ── 2. Enemy groundMask → layer 6 ────────────────────────────────────
        var enemy = GameObject.Find("Enemy");
        if (enemy != null)
        {
            var eb = enemy.GetComponent<EnemyBase>();
            if (eb != null)
            {
                int groundLayer = 6;
                int expectedMask = 1 << groundLayer;
                if (eb.groundMask.value != expectedMask)
                {
                    eb.groundMask = expectedMask;
                    EditorUtility.SetDirty(eb);
                    fixes++;
                    Debug.Log($"[FixPhysicsSetup] Enemy groundMask set to layer {groundLayer}.");
                }
            }

            // Ensure the enemy's catch collider is a trigger
            foreach (var col in enemy.GetComponentsInChildren<Collider>(true))
            {
                if (!col.isTrigger)
                {
                    col.isTrigger = true;
                    EditorUtility.SetDirty(col);
                    fixes++;
                    Debug.Log($"[FixPhysicsSetup] Enemy collider '{col.gameObject.name}' → trigger");
                }
            }

            // ── 3. Move guard further back — gives player a visible head-start ─
            // At guardSpeed 6.5 m/s vs playerSpeed 5.5, closing rate = 1.0 m/s.
            // Starting at z=-15 gives ~15 s before the guard arrives — tense but fair.
            var pos = enemy.transform.position;
            pos.z = -15f;
            enemy.transform.position = pos;

            LayerMask mask = eb != null && eb.groundMask.value != 0
                ? eb.groundMask
                : GroundPlacementUtility.DefaultGroundMask;
            float offset = eb != null ? eb.groundOffset : 0.05f;

            if (GroundPlacementUtility.SnapTransformFeetToGround(enemy.transform, mask, offset))
            {
                EditorUtility.SetDirty(enemy);
                fixes++;
                Debug.Log($"[FixPhysicsSetup] Enemy snapped to ground at {enemy.transform.position}.");
            }
            else
            {
                EditorUtility.SetDirty(enemy);
                fixes++;
                Debug.Log("[FixPhysicsSetup] Enemy Z set to -6 (ground raycast missed — check Ground layer).");
            }
        }
        else
        {
            Debug.LogError("[FixPhysicsSetup] Enemy not found.");
        }

        // ── 4. Save ───────────────────────────────────────────────────────────
        var runLength = Object.FindAnyObjectByType<RunLengthController>();
        if (runLength != null)
        {
            if (runLength.lookAhead < 120f)
            {
                runLength.lookAhead = 150f;
                EditorUtility.SetDirty(runLength);
                fixes++;
            }
            if (runLength.keepBehind < 60f)
            {
                runLength.keepBehind = 80f;
                EditorUtility.SetDirty(runLength);
                fixes++;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixPhysicsSetup] Done — {fixes} fixes applied, scene saved.");
    }

    [MenuItem("Tools/Play From Open Scene")]
    public static void PlayFromOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("[FixPhysicsSetup] Save the scene first.");
            return;
        }

        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        EditorSceneManager.playModeStartScene = asset;
        EditorApplication.isPlaying = true;
    }
}
