using UnityEngine;

// Same 5s slow as the guitar strings. PianoMissSlow can be cancelled by a speed pickup.
public class SlowDownObstacle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!PlayerColliderUtility.IsPlayer(other)) return;
        ApplyToPlayer(PlayerColliderUtility.GetMovement(other));
    }

    public void ApplyToPlayer(PlayerMovement movement)
    {
        if (movement == null) return;

        var col = GetComponent<Collider>();
        if (col != null && !col.enabled) return;
        if (col != null) col.enabled = false;

        PianoMissSlow.Apply(movement);
        Destroy(gameObject);
    }
}
