using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks pickups/obstacles placed on a spawned ground tile.
/// Destruction is driven by RunLengthController — not per-frame on this component.
/// </summary>
public class GroundSegmentLifetime : MonoBehaviour
{
    private Transform _player;
    private readonly List<GameObject> _spawnedContent = new List<GameObject>();
    private bool _pickupsPopulated;

    public bool TryPopulatePickups(Spawner spawner)
    {
        if (_pickupsPopulated || spawner == null)
            return false;

        _pickupsPopulated = true;
        spawner.SpawnGameObjects(gameObject);
        return true;
    }

    private void Start()
    {
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
            _player = playerGo.transform;
    }

    private void OnDestroy()
    {
        CleanupContent();
    }

    public void RegisterContent(GameObject content)
    {
        if (content == null || _spawnedContent.Contains(content)) return;
        _spawnedContent.Add(content);
    }

    public bool IsPlayerStandingOn()
    {
        if (_player == null) return false;

        Physics.SyncTransforms();
        Bounds bounds = GroundChain.GetBounds(gameObject);
        if (bounds.size.sqrMagnitude < 0.01f)
            return false;
        Vector3 p = _player.position;

        bool withinFootprint =
            p.z >= bounds.min.z - 3f && p.z <= bounds.max.z + 3f &&
            p.x >= bounds.min.x - 4f && p.x <= bounds.max.x + 4f;

        if (withinFootprint && p.y - bounds.max.y < 6f)
            return true;

        Vector3 origin = p + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 8f,
            GroundPlacementUtility.DefaultGroundMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null &&
                (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
                return true;
        }

        return false;
    }

    void CleanupContent()
    {
        float playerZ = _player != null ? _player.position.z : float.MaxValue;

        for (int i = _spawnedContent.Count - 1; i >= 0; i--)
        {
            var obj = _spawnedContent[i];
            if (obj == null) continue;

            if (obj.transform.position.z > playerZ + 8f)
                continue;

            Destroy(obj);
        }
    }
}
