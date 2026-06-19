using UnityEngine;

/// <summary>
/// Far-end or one-off Ground children that must not appear on streamed floor tiles.
/// On the scene Ground anchor: moves to the scene root once (world position preserved).
/// On a streamed segment (safety): destroys the duplicate.
/// </summary>
[DefaultExecutionOrder(-50)]
public class GroundStreamExclude : MonoBehaviour
{
    void Awake()
    {
        if (IsUnderStreamedSegment(transform))
        {
            Destroy(gameObject);
            return;
        }

        if (IsUnderSceneGround(transform))
            transform.SetParent(null, true);
    }

    public static bool ShouldSkipChild(Transform child)
    {
        if (child == null) return true;
        if (child.name == "groundtrigger" || child.CompareTag("GroundTrigger"))
            return true;
        if (child.GetComponent<GroundStreamExclude>() != null)
            return true;
        return false;
    }

    static bool IsUnderStreamedSegment(Transform self)
    {
        var t = self.parent;
        while (t != null)
        {
            if (t.GetComponent<GroundSegmentLifetime>() != null)
                return true;
            if (t.name == "GroundSegment" || t.name == "GroundTileTemplate")
                return true;
            t = t.parent;
        }
        return false;
    }

    static bool IsUnderSceneGround(Transform self)
    {
        var p = self.parent;
        if (p == null) return false;
        if (p.name == "Ground")
            return true;
        return p.GetComponent<SceneGroundAnchor>() != null;
    }
}
