using UnityEngine;

/// <summary>
/// Pickups use triggers (run through to collect). Obstacles use solid colliders (bump/stop).
/// </summary>
public static class GameplayCollisionUtility
{
    public const string TagHealthDec = "HealthDEC";
    public const string TagSlowDown = "SlowDown";

    static readonly string[] PickupTags =
    {
        "HealthINC", "Artifact", "JumpBoost", "Sneak", "Speed"
    };

    static readonly string[] SolidObstacleTags =
    {
        TagHealthDec, TagSlowDown
    };

    public static bool IsPickupTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        for (int i = 0; i < PickupTags.Length; i++)
        {
            if (tag == PickupTags[i]) return true;
        }
        return false;
    }

    public static bool IsSolidObstacleTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        for (int i = 0; i < SolidObstacleTags.Length; i++)
        {
            if (tag == SolidObstacleTags[i]) return true;
        }
        return false;
    }

    public static bool IsPickupRoot(GameObject go)
    {
        if (go == null) return false;
        return IsPickupTag(go.tag) || go.GetComponent<MusicInstrument>() != null;
    }

    public static bool IsSolidObstacleRoot(GameObject go)
    {
        if (go == null) return false;
        return IsSolidObstacleTag(go.tag)
               || go.GetComponent<HealthDecreaseObstacle>() != null
               || go.GetComponent<SlowDownObstacle>() != null;
    }

    public static void ConfigureObject(GameObject go)
    {
        if (go == null) return;

        if (IsSolidObstacleRoot(go))
        {
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                col.isTrigger = false;
            EnsureKinematicRigidbody(go);
            return;
        }

        if (IsPickupRoot(go))
        {
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                col.isTrigger = true;
            EnsureKinematicRigidbody(go);
        }
    }

    public static void EnsureKinematicRigidbody(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}
