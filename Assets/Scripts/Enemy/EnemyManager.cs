using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;

    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = player.position + new Vector3(0f, 0f, -20f);

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
