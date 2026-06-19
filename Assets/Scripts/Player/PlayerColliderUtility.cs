using UnityEngine;

/// <summary>
/// Player trigger hits come from the child "PickupTrigger" (Untagged), not the Player root.
/// Use these helpers instead of CompareTag("Player") on trigger colliders.
/// </summary>
public static class PlayerColliderUtility
{
    public static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        if (other.GetComponentInParent<PlayerMovement>() != null) return true;
        if (other.GetComponentInParent<PickupBase>() != null) return true;
        return false;
    }

    public static PickupBase GetPickup(Collider other)
    {
        return other != null ? other.GetComponentInParent<PickupBase>() : null;
    }

    public static PlayerMovement GetMovement(Collider other)
    {
        return other != null ? other.GetComponentInParent<PlayerMovement>() : null;
    }
}
