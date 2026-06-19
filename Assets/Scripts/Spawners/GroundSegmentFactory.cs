using UnityEngine;

/// <summary>
/// Spawns ground tiles with mesh + reliable box collider + back trigger.
/// </summary>
public static class GroundSegmentFactory
{
    public static GameObject Create(GameObject template, Vector3 worldCenter, Spawner pickupSpawner)
    {
        if (template == null)
            return null;

        try
        {
            return CreateInternal(template, worldCenter, pickupSpawner);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GroundSegmentFactory] Create failed: {ex.Message}");
            return null;
        }
    }

    static GameObject CreateInternal(GameObject template, Vector3 worldCenter, Spawner pickupSpawner)
    {
        var segment = new GameObject("GroundSegment");
        segment.layer = template.layer;
        segment.transform.position = worldCenter;
        segment.transform.rotation = Quaternion.identity;
        segment.transform.localScale = template.transform.lossyScale;

        var srcFilter = template.GetComponent<MeshFilter>();
        var srcRenderer = template.GetComponent<MeshRenderer>();
        if (srcFilter != null && srcFilter.sharedMesh != null)
        {
            var filter = segment.AddComponent<MeshFilter>();
            filter.sharedMesh = srcFilter.sharedMesh;

            if (srcRenderer != null)
            {
                var renderer = segment.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = srcRenderer.sharedMaterials;
            }

            AddFloorCollider(segment, srcFilter.sharedMesh);
        }

        AddBackTrigger(segment, template, pickupSpawner);
        CopyTrackDecorations(segment, template);
        segment.AddComponent<GroundSegmentLifetime>();
        return segment;
    }

    static void AddFloorCollider(GameObject segment, Mesh mesh)
    {
        Physics.SyncTransforms();
        var meshBounds = mesh.bounds;
        var lossy = segment.transform.lossyScale;

        float sizeX = Mathf.Abs(meshBounds.size.x * lossy.x);
        float sizeZ = Mathf.Abs(meshBounds.size.z * lossy.z);
        float floorHeight = 1f;

        var box = segment.AddComponent<BoxCollider>();
        box.center = new Vector3(
            meshBounds.center.x * lossy.x,
            meshBounds.center.y * lossy.y - floorHeight * 0.5f + 0.02f,
            meshBounds.center.z * lossy.z);
        box.size = new Vector3(sizeX, floorHeight, sizeZ);
    }

    private static void CopyTrackDecorations(GameObject segment, GameObject template)
    {
        if (template == null) return;

        foreach (Transform child in template.transform)
        {
            if (child.name == "groundtrigger" || child.CompareTag("GroundTrigger"))
                continue;

            var clone = Object.Instantiate(child.gameObject, segment.transform);
            clone.name = child.name;
            clone.transform.localPosition = child.localPosition;
            clone.transform.localRotation = child.localRotation;
            clone.transform.localScale = child.localScale;
        }
    }

    private static void AddBackTrigger(GameObject segment, GameObject template, Spawner pickupSpawner)
    {
        float halfLength = GroundChain.ComputeHalfLength(segment);

        var triggerGo = new GameObject("groundtrigger");
        triggerGo.tag = "GroundTrigger";
        triggerGo.transform.SetParent(segment.transform, false);
        triggerGo.transform.localPosition = new Vector3(0f, 0.5f, -halfLength + 3f);
        triggerGo.transform.localScale = Vector3.one;

        var box = triggerGo.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(80f, 2f, 4f);

        var spawner = triggerGo.AddComponent<GroundSpawner>();
        spawner.groundPrefabTrigger = template;
        spawner.spawnObjects = pickupSpawner;
    }
}
