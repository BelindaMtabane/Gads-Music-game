using System.Collections;
using UnityEngine;

/// <summary>
/// Keeps red curtain obstacles appearing on the track sides during a run.
/// </summary>
public class CurtainSpawnController : MonoBehaviour
{
    public Spawner spawner;
    public float firstSpawnDelay = 8f;
    public float spawnInterval = 12f;
    [Range(0f, 1f)]
    public float spawnChance = 0.8f;
    public float spawnAheadMin = 20f;
    public float spawnAheadMax = 40f;

    private Transform _player;

    private void Start()
    {
        if (spawner == null)
            spawner = FindAnyObjectByType<Spawner>();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _player = player.transform;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!GameManager.GameStarted || spawner == null || spawner.curtainPrefab == null)
                continue;

            if (_player == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                    continue;
                _player = player.transform;
            }

            if (Random.value > spawnChance)
                continue;

            float z = _player.position.z + Random.Range(spawnAheadMin, spawnAheadMax);
            spawner.SpawnSideCurtainAt(z);
        }
    }
}
