using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground tile bounds helpers and legacy entry points used by GroundSpawner triggers.
/// Streaming is owned by RunLengthController.
/// </summary>
public static class GroundChain
{
    private static GameObject _spawnTemplate;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _spawnTemplate = null;
    }

    public static void SetSpawnTemplate(GameObject template)
    {
        _spawnTemplate = template;
    }

    public static float GetStaticFrontZ(GameObject primaryGround)
    {
        float front = primaryGround != null ? GetBounds(primaryGround).max.z : 0f;

        foreach (var anchor in Object.FindObjectsByType<SceneGroundAnchor>(FindObjectsSortMode.None))
        {
            if (anchor == null) continue;
            front = Mathf.Max(front, GetBounds(anchor.gameObject).max.z);
        }

        return front;
    }

    public static float ComputeHalfLength(GameObject groundObject)
    {
        if (groundObject == null) return 35f;

        Physics.SyncTransforms();
        var bounds = GetBounds(groundObject);
        if (bounds.size.sqrMagnitude > 0.01f)
            return bounds.extents.z;

        const float defaultPlaneLength = 10f;
        float meshLength = defaultPlaneLength;
        var meshFilter = groundObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            meshLength = meshFilter.sharedMesh.bounds.size.z;

        float scaleZ = Mathf.Abs(groundObject.transform.lossyScale.z);
        return scaleZ * meshLength * 0.5f;
    }

    public static Bounds GetSpawnFootprint(GameObject go)
    {
        var bounds = GetBounds(go);
        if (bounds.size.sqrMagnitude > 0.01f)
            return bounds;

        float half = ComputeHalfLength(go);
        return new Bounds(go.transform.position, new Vector3(20f, 1f, half * 2f));
    }

    public static void PopulateAllSceneGround(Spawner spawner)
    {
        if (spawner == null) return;

        foreach (var anchor in Object.FindObjectsByType<SceneGroundAnchor>(FindObjectsSortMode.None))
        {
            if (anchor == null) continue;

            var bounds = GetBounds(anchor.gameObject);
            if (bounds.max.z < Spawner.MinSpawnZ && anchor.gameObject.name == "Ground")
                continue;

            anchor.TryPopulatePickups(spawner);
        }
    }

    public static Bounds GetBounds(GameObject go)
    {
        if (go == null)
            return new Bounds(Vector3.zero, Vector3.zero);

        Physics.SyncTransforms();

        var meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            return GetMeshWorldBounds(go.transform, meshFilter.sharedMesh);

        Bounds merged = new Bounds(go.transform.position, Vector3.zero);
        bool has = false;

        foreach (var col in go.GetComponents<Collider>())
        {
            if (col == null || col.isTrigger) continue;
            if (!has)
            {
                merged = col.bounds;
                has = true;
            }
            else
            {
                merged.Encapsulate(col.bounds);
            }
        }

        if (has) return merged;

        return new Bounds(go.transform.position, Vector3.zero);
    }

    static Bounds GetMeshWorldBounds(Transform transform, Mesh mesh)
    {
        var meshBounds = mesh.bounds;
        var lossy = transform.lossyScale;
        Vector3 worldCenter = transform.TransformPoint(meshBounds.center);
        Vector3 worldSize = new Vector3(
            Mathf.Abs(meshBounds.size.x * lossy.x),
            Mathf.Abs(meshBounds.size.y * lossy.y),
            Mathf.Abs(meshBounds.size.z * lossy.z));
        return new Bounds(worldCenter, worldSize);
    }

    // Legacy — GroundSpawner trigger backup
    public static void FillGapsAhead(Transform player, GameObject template, Spawner pickupSpawner, float lookAhead)
    {
        var stream = Object.FindAnyObjectByType<RunLengthController>();
        if (stream != null) return;

        if (player == null || template == null) return;
        SetSpawnTemplate(template);
        float front = GetBounds(template).max.z;
        float target = player.position.z + lookAhead;
        int safety = 0;
        while (front < target && safety < 8)
        {
            float half = ComputeHalfLength(template);
            float centerZ = front - 10f + half;
            var seg = GroundSegmentFactory.Create(template, new Vector3(0f, 0f, centerZ), pickupSpawner);
            if (seg == null) break;
            front = GetBounds(seg).max.z;
            if (pickupSpawner != null && GameManager.GameStarted)
                pickupSpawner.SpawnGameObjects(seg);
            safety++;
        }
    }

    public static GameObject SpawnNextSegment(Spawner pickupSpawner)
    {
        var template = _spawnTemplate;
        if (template == null) return null;

        float half = ComputeHalfLength(template);
        float front = GetBounds(template).max.z;
        var seg = GroundSegmentFactory.Create(template, new Vector3(0f, 0f, front - 10f + half), pickupSpawner);
        if (seg != null && pickupSpawner != null && GameManager.GameStarted)
            pickupSpawner.SpawnGameObjects(seg);
        return seg;
    }
}
