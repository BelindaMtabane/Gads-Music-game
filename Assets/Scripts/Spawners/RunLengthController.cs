using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the infinite ground stream: spawn ahead, rescue below, prune behind.
/// Instance state only — no static front edge that can go stale between runs.
/// </summary>
[DefaultExecutionOrder(-100)]
public class RunLengthController : MonoBehaviour
{
    public Transform player;
    public GameObject groundPrefab;
    public Spawner spawner;

    [Tooltip("How far ahead of the player (metres) to keep ground spawned.")]
    public float lookAhead = 150f;

    [Tooltip("How far behind the player (metres) before a passed tile is destroyed.")]
    public float keepBehind = 80f;

    private const float SeamOverlap = 10f;
    private const int MinSegmentsAhead = 2;
    private const int MinActiveSegments = 2;

    private readonly List<GameObject> _segments = new List<GameObject>();
    private GameObject _tileTemplate;
    private float _tileHalfLength = 35f;
    private float _spawnFrontZ;

    private bool _streamReady;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        InitializeStream();
    }

    void InitializeStream()
    {
        if (_streamReady) return;

        ResolveReferences();
        if (groundPrefab == null || player == null)
            return;

        GroundBootstrap.EnsureSceneGround(groundPrefab);

        if (_tileTemplate == null)
            _tileTemplate = BuildTileTemplate(groundPrefab);

        _tileHalfLength = GroundChain.ComputeHalfLength(_tileTemplate);
        _spawnFrontZ = GroundChain.GetStaticFrontZ(groundPrefab);

        FillAhead(player.position.z + lookAhead);
        _streamReady = true;
    }

    public void PopulatePickupsForRun()
    {
        if (spawner == null)
            spawner = FindSpawner();
        if (spawner == null) return;

        GroundChain.PopulateAllSceneGround(spawner);

        foreach (var segment in _segments)
        {
            if (segment == null) continue;
            segment.GetComponent<GroundSegmentLifetime>()?.TryPopulatePickups(spawner);
        }
    }

    private void FixedUpdate()
    {
        if (player == null || _tileTemplate == null) return;
        EnsureFloorUnderPlayer();
    }

    private void LateUpdate()
    {
        if (!_streamReady)
            InitializeStream();

        if (player == null || _tileTemplate == null) return;

        float playerZ = player.position.z;
        FillAhead(playerZ + lookAhead);
        EnsureFloorUnderPlayer();
        PruneBehind(playerZ);
    }

    void ResolveReferences()
    {
        if (player == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
                player = playerGo.transform;
        }

        if (spawner == null)
            spawner = FindSpawner();
    }

    GameObject BuildTileTemplate(GameObject source)
    {
        var template = new GameObject("GroundTileTemplate");
        template.transform.SetParent(transform, false);
        template.layer = source.layer;
        template.transform.localRotation = Quaternion.identity;
        template.transform.localScale = source.transform.lossyScale;

        var srcFilter = source.GetComponent<MeshFilter>();
        var srcRenderer = source.GetComponent<MeshRenderer>();
        if (srcFilter != null && srcFilter.sharedMesh != null)
        {
            var filter = template.AddComponent<MeshFilter>();
            filter.sharedMesh = srcFilter.sharedMesh;

            if (srcRenderer != null)
            {
                var renderer = template.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = srcRenderer.sharedMaterials;
            }
        }

        foreach (Transform child in source.transform)
        {
            if (GroundStreamExclude.ShouldSkipChild(child))
                continue;

            var clone = Instantiate(child.gameObject, template.transform);
            clone.name = child.name;
            clone.transform.localPosition = child.localPosition;
            clone.transform.localRotation = child.localRotation;
            clone.transform.localScale = child.localScale;
        }

        template.SetActive(false);
        return template;
    }

    void FillAhead(float targetFrontZ)
    {
        PurgeNullSegments();

        float playerZ = player.position.z;
        if (_segments.Count == 0 || CountTilesAheadOf(playerZ) < MinSegmentsAhead)
            RecomputeSpawnFront();

        if (_spawnFrontZ < playerZ + 30f)
            RecomputeSpawnFront();

        int safety = 0;
        while (_spawnFrontZ < targetFrontZ - 0.5f && safety < 64)
        {
            if (!SpawnNextTile())
            {
                RecomputeSpawnFront();
                if (!SpawnNextTile())
                    break;
            }
            safety++;
        }
    }

    bool SpawnNextTile()
    {
        float centerZ = _spawnFrontZ - SeamOverlap + _tileHalfLength;
        var segment = GroundSegmentFactory.Create(
            _tileTemplate,
            new Vector3(0f, 0f, centerZ),
            spawner);

        if (segment == null)
            return false;

        segment.transform.SetParent(transform, true);
        _segments.Add(segment);
        Physics.SyncTransforms();
        _spawnFrontZ = Mathf.Max(_spawnFrontZ, GroundChain.GetBounds(segment).max.z);

        if (spawner != null && GameManager.GameStarted)
            segment.GetComponent<GroundSegmentLifetime>()?.TryPopulatePickups(spawner);

        return true;
    }

    void EnsureFloorUnderPlayer()
    {
        if (HasSolidGroundAt(player.position))
            return;

        float centerZ = player.position.z;
        var segment = GroundSegmentFactory.Create(
            _tileTemplate,
            new Vector3(0f, 0f, centerZ),
            spawner);

        if (segment == null)
            return;

        segment.transform.SetParent(transform, true);
        _segments.Add(segment);
        RecomputeSpawnFront();

        if (spawner != null && GameManager.GameStarted)
            segment.GetComponent<GroundSegmentLifetime>()?.TryPopulatePickups(spawner);
    }

    void PruneBehind(float playerZ)
    {
        PurgeNullSegments();

        for (int i = _segments.Count - 1; i >= 0; i--)
        {
            if (_segments.Count <= MinActiveSegments)
                break;

            var segment = _segments[i];
            if (segment == null)
            {
                _segments.RemoveAt(i);
                continue;
            }

            var lifetime = segment.GetComponent<GroundSegmentLifetime>();
            if (lifetime != null && lifetime.IsPlayerStandingOn())
                continue;

            var bounds = GroundChain.GetBounds(segment);
            if (playerZ >= bounds.min.z - 12f && playerZ <= bounds.max.z + 12f)
                continue;

            if (bounds.max.z >= playerZ - keepBehind)
                continue;

            Destroy(segment);
            _segments.RemoveAt(i);
        }

        RecomputeSpawnFront();
    }

    void RecomputeSpawnFront()
    {
        _spawnFrontZ = GroundChain.GetStaticFrontZ(groundPrefab);

        foreach (var segment in _segments)
        {
            if (segment == null) continue;
            _spawnFrontZ = Mathf.Max(_spawnFrontZ, GroundChain.GetBounds(segment).max.z);
        }
    }

    void PurgeNullSegments()
    {
        for (int i = _segments.Count - 1; i >= 0; i--)
        {
            if (_segments[i] == null)
                _segments.RemoveAt(i);
        }
    }

    int CountTilesAheadOf(float playerZ)
    {
        int count = 0;
        foreach (var segment in _segments)
        {
            if (segment == null) continue;
            if (GroundChain.GetBounds(segment).max.z > playerZ + 2f)
                count++;
        }
        return count;
    }

    static bool HasSolidGroundAt(Vector3 worldPos)
    {
        int groundMask = 1 << 6;
        Vector3 origin = worldPos + Vector3.up * 0.5f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 15f, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        return worldPos.y - hit.point.y < 2.5f;
    }

    static Spawner FindSpawner()
    {
        var rlcSpawner = GameObject.Find("spawnObjects")?.GetComponent<Spawner>();
        if (rlcSpawner != null) return rlcSpawner;
        return FindAnyObjectByType<Spawner>();
    }
}
