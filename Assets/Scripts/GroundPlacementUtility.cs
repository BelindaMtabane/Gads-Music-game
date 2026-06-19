using UnityEngine;

/// <summary>
/// Raycasts to the Ground layer and places character pivots so feet sit on the floor.
/// </summary>
public static class GroundPlacementUtility
{
    public const int GroundLayer = 6;

    public static LayerMask DefaultGroundMask => 1 << GroundLayer;

    public static bool SnapTransformFeetToGround(
        Transform subject,
        LayerMask groundMask,
        float groundOffset = 0.05f,
        float rayHeight = 8f)
    {
        if (subject == null) return false;

        if (groundMask.value == 0)
            groundMask = DefaultGroundMask;

        Physics.SyncTransforms();
        Vector3 origin = subject.position + Vector3.up * rayHeight;
        float maxDist = rayHeight + GetPivotToFeet(subject, groundOffset) + 8f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        float pivotToFeet = GetPivotToFeet(subject, groundOffset);
        subject.position = new Vector3(subject.position.x, hit.point.y + pivotToFeet, subject.position.z);
        return true;
    }

    public static float GetPivotToFeet(Transform subject, float groundOffset = 0.05f)
    {
        var cc = subject.GetComponent<CharacterController>();
        if (cc != null)
            return cc.center.y + cc.height * 0.5f + groundOffset;

        Physics.SyncTransforms();
        float feetY = subject.position.y;
        bool found = false;

        foreach (var col in subject.GetComponentsInChildren<Collider>())
        {
            if (col.isTrigger) continue;
            feetY = Mathf.Min(feetY, col.bounds.min.y);
            found = true;
        }

        if (found)
            return subject.position.y - feetY + groundOffset;

        float scaledHalfHeight = 1f * Mathf.Abs(subject.lossyScale.y);
        return scaledHalfHeight + groundOffset;
    }
}
