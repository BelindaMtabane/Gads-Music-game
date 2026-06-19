using UnityEngine;

/// <summary>
/// Links a spawned pickup/obstacle to its ground tile for cleanup when the tile is removed.
/// </summary>
public class GroundSegmentContent : MonoBehaviour
{
    public static void Bind(GameObject content, GameObject groundSegment)
    {
        if (content == null || groundSegment == null) return;

        var marker = content.GetComponent<GroundSegmentContent>();
        if (marker == null)
            marker = content.AddComponent<GroundSegmentContent>();

        var lifetime = groundSegment.GetComponent<GroundSegmentLifetime>();
        if (lifetime != null)
            lifetime.RegisterContent(content);
    }
}
