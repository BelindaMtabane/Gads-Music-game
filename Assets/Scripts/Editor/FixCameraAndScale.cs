using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fixes two "scene looks oversized" issues in MainGameL1:
///
///  1. Camera position — moves Main Camera from 12.4 units behind player to 7 units
///     and adds a 12° downward tilt so the track fills more of the screen and the
///     horizon line drops, giving the stage a proper runner-game feel.
///
///  2. Ground track width — reduces Ground.transform.localScale.x from 8.42 (84-unit
///     wide track) to 2.5 (25-unit wide track). The player strafes ±6 units from
///     centre, so 25 units is generous while removing the dead space on either side
///     that made the stage look like an empty field.
///     GroundSegmentFactory copies lossyScale from the template, so spawned tiles
///     will also be 25 units wide automatically.
///
/// Run via  Tools → Fix Camera and Scale
/// </summary>
public static class FixCameraAndScale
{
    [MenuItem("Tools/Fix Camera and Scale")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().name != "MainGameL1")
        {
            Debug.LogWarning("[FixCameraAndScale] Open MainGameL1 first.");
            return;
        }

        int fixes = 0;

        // ── 1. Camera ─────────────────────────────────────────────────────────
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var camTransform = player.transform.Find("Main Camera");
            if (camTransform == null)
            {
                // Some setups nest the camera differently — search in children
                var cam = player.GetComponentInChildren<Camera>();
                if (cam != null) camTransform = cam.transform;
            }

            if (camTransform != null)
            {
                // Move camera closer: z=7 instead of 12.4  (local +Z = world -Z on a
                // 180°-rotated player, so larger value = further behind player)
                var lp = camTransform.localPosition;
                var targetLocalPos = new Vector3(lp.x, lp.y, 7f);
                if (Vector3.Distance(lp, targetLocalPos) > 0.1f)
                {
                    camTransform.localPosition = targetLocalPos;
                    EditorUtility.SetDirty(camTransform.gameObject);
                    fixes++;
                    Debug.Log($"[FixCameraAndScale] Camera moved from z={lp.z:F2} to z=7 (closer follow).");
                }

                // Tilt 12° downward. Camera world rotation = parent(0,180,0) × local(x,180,0) = (x,0,0).
                // So local pitch = desired world pitch → local euler.x = 12.
                var le = camTransform.localEulerAngles;
                float targetPitch = 12f;
                float currentPitch = le.x > 180f ? le.x - 360f : le.x; // normalise to [-180,180]
                if (Mathf.Abs(currentPitch - targetPitch) > 0.5f)
                {
                    camTransform.localEulerAngles = new Vector3(targetPitch, le.y, le.z);
                    EditorUtility.SetDirty(camTransform.gameObject);
                    fixes++;
                    Debug.Log($"[FixCameraAndScale] Camera tilted 12° downward (was {currentPitch:F1}°).");
                }
            }
            else
            {
                Debug.LogWarning("[FixCameraAndScale] Could not find Main Camera child under Player.");
            }
        }
        else
        {
            Debug.LogError("[FixCameraAndScale] Player not found.");
        }

        // ── 2. Ground track width ─────────────────────────────────────────────
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            var s = ground.transform.localScale;
            const float TargetXScale = 2.5f; // → 25-unit-wide track (was 8.42 → 84 units)
            if (Mathf.Abs(s.x - TargetXScale) > 0.01f)
            {
                ground.transform.localScale = new Vector3(TargetXScale, s.y, s.z);
                EditorUtility.SetDirty(ground);
                fixes++;
                Debug.Log($"[FixCameraAndScale] Ground track width: {s.x * 10f:F0} → {TargetXScale * 10f:F0} units.");
            }
            else
            {
                Debug.Log($"[FixCameraAndScale] Ground track width already {TargetXScale * 10f:F0} units — OK.");
            }
        }
        else
        {
            Debug.LogError("[FixCameraAndScale] Ground not found.");
        }

        // ── 3. Save ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixCameraAndScale] Done — {fixes} fixes applied, scene saved.");
    }
}
