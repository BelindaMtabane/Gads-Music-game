using UnityEngine;

// Player-side catch for slow pickups. The timer lives on PianoMissSlow.
public class ObstacleDown : MonoBehaviour
{
    PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SlowDown"))
            return;

        PianoMissSlow.Apply(playerMovement);
        Destroy(other.gameObject);
    }
}
