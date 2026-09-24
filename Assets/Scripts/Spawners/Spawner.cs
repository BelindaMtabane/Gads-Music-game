using UnityEngine;

[System.Serializable]
public class WeightedSpawnEntry
{
    public GameObject prefab;
    [Tooltip("Higher = more likely to spawn. E.g. weight 3 is 3× more common than weight 1.")]
    [Range(1, 20)]
    public int weight = 1;
}

public class Spawner : MonoBehaviour
{
    public const float MinSpawnZ = 25f;

    public int spawnCount = 8;

    [Tooltip("If populated, uses weighted random. Leave empty to fall back to spawnObjects.")]
    public WeightedSpawnEntry[] weightedObjects;

    [Tooltip("Legacy equal-weight pool. Used only when weightedObjects is empty.")]
    public GameObject[] spawnObjects;

    [Header("Artifacts")]
    public GameObject artifactTemplate;
    public int artifactsPerSegment = 1;
    [Tooltip("Place a guaranteed artifact on every Nth ground segment.")]
    public int artifactSegmentInterval = 1;

    int _segmentsConsideredForArtifacts;

    [Header("Curtain (side spawn only)")]
    public GameObject curtainPrefab;
    [Range(0f, 1f)]
    public float curtainSpawnChance = 0.7f;

    private const float SpawnSurfaceLift = 0.12f;
    const float SpawnGap = 18f;
    private const int GroundLayerMask = 1 << 6;
    static readonly float[] SpawnLanes = { -4.5f, 0f, 4.5f };

    public void SpawnGameObjects(GameObject ground)
    {
        if (ground == null) return;

        var footprint = GroundChain.GetSpawnFootprint(ground);
        GetSpawnRange(ground, footprint, out float minX, out float maxX, out float minZ, out float maxZ);
        float rayStartY = footprint.max.y + 5f;

        float span = Mathf.Max(0.01f, maxZ - minZ);
        int capacity = Mathf.Max(1, Mathf.FloorToInt(span / SpawnGap));
        int count = Mathf.Min(Mathf.Max(1, spawnCount), capacity);

        int artifactSlots = 0;
        if (artifactTemplate != null && artifactsPerSegment > 0 && ConsumeArtifactTurn())
            artifactSlots = Mathf.Min(artifactsPerSegment, count);

        for (int i = 0; i < count; i++)
        {
            float z = Mathf.Lerp(minZ, maxZ, (i + 0.5f) / count);
            float x = Mathf.Clamp(SpawnLanes[i % SpawnLanes.Length], minX, maxX);
            Vector3 position = SnapToGroundSurface(x, z, ground, rayStartY);

            if (i >= count - artifactSlots)
            {
                position.y += 0.35f;
                AddArtifactGlow(SpawnOnGround(artifactTemplate, position, ground));
                continue;
            }

            GameObject spawnPickup = PickRandomNonArtifact();
            if (spawnPickup == null) continue;
            SpawnOnGround(spawnPickup, position, ground);
        }
    }

    bool ConsumeArtifactTurn()
    {
        _segmentsConsideredForArtifacts++;
        int interval = Mathf.Max(1, artifactSegmentInterval);
        return (_segmentsConsideredForArtifacts % interval) == 0;
    }

    static GameObject SpawnOnGround(GameObject template, Vector3 worldPosition, GameObject groundSegment)
    {
        if (template == null || groundSegment == null) return null;

        var spawnedObject = Object.Instantiate(template, worldPosition, template.transform.rotation);
        spawnedObject.transform.localScale = template.transform.localScale;
        PrepareSpawnedClone(spawnedObject);
        GroundSegmentContent.Bind(spawnedObject, groundSegment);
        return spawnedObject;
    }

    public static void PrepareSpawnedClone(GameObject clone)
    {
        if (clone == null) return;

        clone.SetActive(true);
        foreach (var renderer in clone.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = true;
        foreach (var col in clone.GetComponentsInChildren<Collider>(true))
            col.enabled = true;

        GameplayCollisionUtility.ConfigureObject(clone);
        BeatPulseVisual.Attach(clone);
        DrumRollToCurtain.Attach(clone);
        if (LevelProgress.CurrentLevel == 3)
            ClubMicrophone.Replace(clone);
        OpeningCurtain.Ensure(clone);
        PickupPresentation.RevealObject(clone);
    }

    static void GetSpawnRange(GameObject ground, Bounds footprint, out float minX, out float maxX, out float minZ, out float maxZ)
    {
        bool isStreamedTile = ground.GetComponent<GroundSegmentLifetime>() != null;

        float minZRaw = footprint.min.z + 4f;
        float maxZRaw = footprint.max.z - 4f;
        if (!isStreamedTile)
            minZRaw = Mathf.Max(minZRaw, MinSpawnZ);

        if (maxZRaw - minZRaw < 6f)
        {
            float centerZ = footprint.center.z;
            float halfZ = Mathf.Max(footprint.extents.z - 4f, 8f);
            minZRaw = centerZ - halfZ;
            maxZRaw = centerZ + halfZ;
            if (!isStreamedTile)
                minZRaw = Mathf.Max(minZRaw, MinSpawnZ);
        }

        minZ = Mathf.Min(minZRaw, maxZRaw);
        maxZ = Mathf.Max(minZRaw, maxZRaw);

        minX = Mathf.Max(footprint.min.x + 2f, -6f);
        maxX = Mathf.Min(footprint.max.x - 2f, 6f);
        if (maxX - minX < 3f)
        {
            float centerX = Mathf.Clamp(footprint.center.x, -4f, 4f);
            minX = centerX - 4f;
            maxX = centerX + 4f;
        }
    }

    static Vector3 SnapToGroundSurface(float x, float z, GameObject ground, float rayStartY)
    {
        Vector3 origin = new Vector3(x, rayStartY, z);
        var hits = Physics.RaycastAll(origin, Vector3.down, 80f, GroundLayerMask, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                var t = hit.collider.transform;
                if (t == ground.transform || t.IsChildOf(ground.transform))
                    return hit.point + Vector3.up * SpawnSurfaceLift;
            }

            return hits[0].point + Vector3.up * SpawnSurfaceLift;
        }

        return new Vector3(x, SpawnSurfaceLift, z);
    }

    private GameObject PickRandomNonArtifact()
    {
        // ── Weighted pool (preferred) ─────────────────────────────────────────
        if (weightedObjects != null && weightedObjects.Length > 0)
        {
            int totalWeight = 0;
            foreach (var e in weightedObjects)
                if (e.prefab != null && !e.prefab.CompareTag("Artifact") && !IsCurtain(e.prefab))
                    totalWeight += Mathf.Max(1, e.weight);

            if (totalWeight > 0)
            {
                int roll = Random.Range(0, totalWeight);
                int cumulative = 0;
                foreach (var e in weightedObjects)
                {
                    if (e.prefab == null || e.prefab.CompareTag("Artifact") || IsCurtain(e.prefab)) continue;
                    cumulative += Mathf.Max(1, e.weight);
                    if (roll < cumulative) return e.prefab;
                }
            }
        }

        // ── Legacy equal-weight fallback ──────────────────────────────────────
        if (spawnObjects == null || spawnObjects.Length == 0) return null;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            var pick = spawnObjects[Random.Range(0, spawnObjects.Length)];
            if (pick == null) continue;
            if (pick.CompareTag("Artifact") || IsCurtain(pick)) continue;
            if (artifactTemplate != null && pick == artifactTemplate) continue;
            return pick;
        }

        return spawnObjects[Random.Range(0, spawnObjects.Length)];
    }

    static void AddArtifactGlow(GameObject artifact)
    {
        if (artifact == null || artifact.transform.Find("ArtifactGlow") != null)
            return;

        var lightGo = new GameObject("ArtifactGlow");
        lightGo.transform.SetParent(artifact.transform, false);
        lightGo.transform.localPosition = Vector3.up * 0.8f;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.831f, 0.686f, 0.216f);
        light.range = 5f;
        light.intensity = 1.5f;
    }

    public GameObject SpawnSideCurtainAt(float worldZ)
    {
        return null;
    }

    static bool IsCurtain(GameObject go)
    {
        if (go == null) return true;
        if (go.name.StartsWith("Curtain")) return true;
        try
        {
            return go.CompareTag("Curtain");
        }
        catch (UnityException)
        {
            return false;
        }
    }
}
