using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates a child "PickupTrigger" GameObject on the Player with a CapsuleCollider
/// trigger + PickupTriggerProxy.
///
/// Background: Unity suppresses OnTriggerEnter on any object that has a
/// CharacterController — the CC takes over the capsule geometry and the sibling
/// CapsuleCollider is ignored by the physics engine. Moving the trigger to a
/// child object (no CC) fixes this without touching CharacterController behaviour.
///
/// Run via  Tools → Fix Pickup Trigger
/// </summary>
public static class FixPickupTrigger
{
    [MenuItem("Tools/Fix Pickup Trigger")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().name != "MainGameL1")
        {
            Debug.LogWarning("[FixPickupTrigger] Open MainGameL1 first.");
            return;
        }

        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("[FixPickupTrigger] Player not found."); return; }

        var cc = player.GetComponent<CharacterController>();

        // ── Reuse existing child or create one ───────────────────────────────
        var existing = player.transform.Find("PickupTrigger");
        GameObject triggerGo = existing != null
            ? existing.gameObject
            : new GameObject("PickupTrigger");

        if (existing == null)
        {
            triggerGo.transform.SetParent(player.transform, false);
            triggerGo.transform.localPosition = Vector3.zero;
            triggerGo.transform.localRotation = Quaternion.identity;
            triggerGo.transform.localScale    = Vector3.one;
        }

        // ── CapsuleCollider sized to match the CharacterController ───────────
        var col = triggerGo.GetComponent<CapsuleCollider>();
        if (col == null) col = triggerGo.AddComponent<CapsuleCollider>();

        col.isTrigger = true;
        if (cc != null)
        {
            // Match CC geometry so the trigger volume covers the whole player body.
            // Expand radius slightly so it catches objects the player's shoulder brushes.
            col.center    = cc.center;
            col.height    = cc.height;
            col.radius    = cc.radius + 0.15f;
            col.direction = 1; // Y-axis capsule
        }
        else
        {
            col.center    = new Vector3(0f, 0.9f, 0f);
            col.height    = 1.8f;
            col.radius    = 0.45f;
            col.direction = 1;
        }
        EditorUtility.SetDirty(col);

        // ── PickupTriggerProxy ───────────────────────────────────────────────
        var proxy = triggerGo.GetComponent<PickupTriggerProxy>();
        if (proxy == null) proxy = triggerGo.AddComponent<PickupTriggerProxy>();
        EditorUtility.SetDirty(proxy);

        // ── Disable the root CapsuleCollider — it's suppressed by CC anyway ─
        var rootCol = player.GetComponent<CapsuleCollider>();
        if (rootCol != null && rootCol.enabled)
        {
            rootCol.enabled = false;
            EditorUtility.SetDirty(rootCol);
            Debug.Log("[FixPickupTrigger] Disabled root CapsuleCollider (suppressed by CharacterController anyway).");
        }

        EditorUtility.SetDirty(triggerGo);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixPickupTrigger] Done — PickupTrigger child added/updated on Player, scene saved.");
    }
}
