using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    public GameObject groundPrefabTrigger;
    private bool hasSpawned = false;
    //public SpawnObjects spawnObjects;

    void Start()
    {
        // Assign the Spawn objects script to the variable

        /*spawnObjects = Object.FindFirstObjectByType<SpawnObjects>(); // Updated to use the recommended method
        if (spawnObjects == null)
        {
            Debug.LogError("SpawnObjects script NOT found in scene!");
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            hasSpawned = true;
            SpawnGround();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited, so stop spawnning");
    }
    void SpawnGround()
    {
        // Instantiate a new ground piece at the desired position
        Vector3 spawnPosition = new Vector3(0f, 0f, transform.position.z + 90f); // Adjust the spawn position as needed
        GameObject newGround = Instantiate(groundPrefabTrigger, spawnPosition, Quaternion.identity);

        // Spawn the objects on the new grounds
        /*if (spawnObjects != null)
            spawnObjects.SpawnGameObjects(newGround);*/

        if (groundPrefabTrigger == null)
        {
            Debug.LogError("Ground Prefab is NOT assigned!");
            return;
        }

        // Destroy the ground after 10 seconds each
        Destroy(newGround, 25f);
    }
}
