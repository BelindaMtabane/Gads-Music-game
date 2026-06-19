using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundPrefabTrigger;
    public Spawner spawnObjects;

    private bool hasSpawned;

    void Start()
    {
        if (spawnObjects == null)
            spawnObjects = Object.FindFirstObjectByType<Spawner>();

        if (groundPrefabTrigger != null)
            GroundChain.SetSpawnTemplate(groundPrefabTrigger);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.GameStarted || !PlayerColliderUtility.IsPlayer(other) || hasSpawned)
            return;

        hasSpawned = true;
        SpawnGround();
    }

    void SpawnGround()
    {
        if (groundPrefabTrigger == null)
        {
            Debug.LogError("Ground Prefab is NOT assigned!");
            return;
        }

        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
            GroundChain.FillGapsAhead(playerGo.transform, groundPrefabTrigger, spawnObjects, 150f);
        else
            GroundChain.SpawnNextSegment(spawnObjects);
    }
}
