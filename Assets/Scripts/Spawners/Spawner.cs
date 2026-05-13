using UnityEngine;

public class Spawner : MonoBehaviour
{
    //Create the variables for the objects
    public int spawnCount = 5;
    public GameObject[] spawnObjects;
    //public GameObject brickPickup;
    public void SpawnGameObjects(GameObject ground)
    {
        //Create a for loop to spawn mulitple
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawnPickup = spawnObjects[Random.Range(0, spawnObjects.Length)];
            Vector3 randomPosition = ground.transform.position + new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-50f, 50f));
            //Spawn the object
            GameObject spawnedObject = Instantiate(spawnPickup, randomPosition, Quaternion.identity);

            //Destroy after a certain amount of questions
            Destroy(spawnedObject, 50f);
        }
        
    }
}
