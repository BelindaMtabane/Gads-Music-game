using System.Collections.Generic;
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

    private const float CurtainSideMinX = 7f;
    private const float CurtainSideMaxX = 10f;
    private const float CurtainSpawnY = 3f;
    private const float SpawnSurfaceLift = 0.12f;
    public const float CurtainPickupGap = 16f;
    private const int GroundLayerMask = 1 << 6;

    public void SpawnGameObjects(GameObject ground)
    {
        if (ground == null) return;

        var footprint = GroundChain.GetSpawnFootprint(ground);
        GetSpawnRange(ground, footprint, out float minX, out float maxX, out float minZ, out float maxZ);
        float rayStartY = footprint.max.y + 5f;

        var placed = new List<Vector3>(spawnCount);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawnPickup = PickRandomNonArtifact();
            if (spawnPickup == null) continue;

            Vector3 position = Vector3.zero;
            bool placedClear = false;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float spawnX = Random.Range(minX, maxX);
                float spawnZ = Random.Range(minZ, maxZ);
                position = SnapToGroundSurface(spawnX, spawnZ, ground, rayStartY);
                if (!IsCrowded(position, placed, 7f) && !BlocksCurtainView(position))
                {
                    placedClear = true;
                    break;
                }
            }

            if (!placedClear) continue;
            placed.Add(position);
            SpawnOnGround(spawnPickup, position, ground);
        }

        TrySpawnSideCurtain(ground, footprint);
        SpawnArtifacts(ground, footprint, minX, maxX, minZ, maxZ, rayStartY);
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
                if (e.prefab != null && !e.prefab.CompareTag("Artifact"))
                    totalWeight += Mathf.Max(1, e.weight);

            if (totalWeight > 0)
            {
                int roll = Random.Range(0, totalWeight);
                int cumulative = 0;
                foreach (var e in weightedObjects)
                {
                    if (e.prefab == null || e.prefab.CompareTag("Artifact")) continue;
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
            if (pick.CompareTag("Artifact")) continue;
            if (artifactTemplate != null && pick == artifactTemplate) continue;
            return pick;
        }

        return spawnObjects[Random.Range(0, spawnObjects.Length)];
    }

    private void SpawnArtifacts(GameObject ground, Bounds footprint, float minX, float maxX, float minZ, float maxZ, float rayStartY)
    {
        if (artifactTemplate == null || artifactsPerSegment < 1)
            return;

        _segmentsConsideredForArtifacts++;
        int interval = Mathf.Max(1, artifactSegmentInterval);
        if ((_segmentsConsideredForArtifacts % interval) != 0)
            return;

        for (int i = 0; i < artifactsPerSegment; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            Vector3 position = SnapToGroundSurface(x, z, ground, rayStartY);
            for (int attempt = 0; attempt < 8 && BlocksCurtainView(position); attempt++)
            {
                z = Random.Range(minZ, maxZ);
                position = SnapToGroundSurface(Random.Range(minX, maxX), z, ground, rayStartY);
            }
            position.y += 0.35f;
            AddArtifactGlow(SpawnOnGround(artifactTemplate, position, ground));
        }
    }

    static bool IsCrowded(Vector3 candidate, List<Vector3> placed, float minDistance)
    {
        float minSqr = minDistance * minDistance;
        for (int i = 0; i < placed.Count; i++)
        {
            Vector3 delta = placed[i] - candidate;
            delta.y = 0f;
            if (delta.sqrMagnitude < minSqr)
                return true;
        }
        return false;
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

    private void TrySpawnSideCurtain(GameObject ground, Bounds footprint)
    {
        if (curtainPrefab == null || Random.value > curtainSpawnChance)
            return;

        float side = Random.value < 0.5f ? -1f : 1f;
        float x = side * Random.Range(CurtainSideMinX, CurtainSideMaxX);
        float z = Random.Range(footprint.min.z + 5f, footprint.max.z - 5f);
        if (ground.GetComponent<GroundSegmentLifetime>() == null)
            z = Mathf.Max(z, MinSpawnZ);

        Vector3 position = new Vector3(x, CurtainSpawnY, z);
        var curtain = Object.Instantiate(curtainPrefab, position, curtainPrefab.transform.rotation);
        curtain.transform.localScale = curtainPrefab.transform.localScale;
        PrepareSpawnedClone(curtain);
        GroundSegmentContent.Bind(curtain, ground);
        ClearPickupsAheadOf(curtain.transform.position);
    }

    public GameObject SpawnSideCurtainAt(float worldZ)
    {
        if (curtainPrefab == null)
            return null;

        float side = Random.value < 0.5f ? -1f : 1f;
        float x = side * Random.Range(CurtainSideMinX, CurtainSideMaxX);
        GameObject curtain = Instantiate(curtainPrefab, new Vector3(x, CurtainSpawnY, worldZ), curtainPrefab.transform.rotation);
        curtain.transform.localScale = curtainPrefab.transform.localScale;
        PrepareSpawnedClone(curtain);
        ClearPickupsAheadOf(curtain.transform.position);
        Destroy(curtain, 65f);
        return curtain;
    }

    static bool BlocksCurtainView(Vector3 position)
    {
        var curtains = Object.FindObjectsByType<CurtainObstacle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < curtains.Length; i++)
        {
            if (curtains[i] == null) continue;
            float ahead = position.z - curtains[i].transform.position.z;
            if (ahead > -2.5f && ahead < CurtainPickupGap)
                return true;
        }
        return false;
    }

    public static void ClearPickupsAheadOf(Vector3 curtainPos)
    {
        var roots = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int shifted = 0;
        for (int i = 0; i < roots.Length; i++)
        {
            var t = roots[i];
            if (t == null || t.parent != null) continue;
            if (!IsCollectible(t)) continue;

            float ahead = t.position.z - curtainPos.z;
            if (ahead <= -2.5f || ahead >= CurtainPickupGap) continue;

            var p = t.position;
            p.z = curtainPos.z + CurtainPickupGap + shifted * 5f;
            t.position = p;
            shifted++;
        }
    }

    static bool IsCollectible(Transform t)
    {
        try
        {
            return t.CompareTag("Speed")
                || t.CompareTag("Sneak")
                || t.CompareTag("HealthINC")
                || t.CompareTag("JumpBoost")
                || t.CompareTag("Artifact");
        }
        catch (UnityException)
        {
            return false;
        }
    }
}
