using UnityEngine;

public class HealthDecreaseObstacle : MonoBehaviour
{
    [Tooltip("How much health to take away (ignored when instantKill is true)")]
    public int damage = 15;

    [Tooltip("Always drains Vibe directly, even when Shield remains.")]
    public int vibeDamage = 5;

    [Tooltip("High-danger obstacles end the run immediately.")]
    public bool instantKill = false;

    // Collision is handled by PickupBase.OnTriggerEnter (tag "HealthDEC") which calls
    // ApplyObstacleDamage(this). A second OnTriggerEnter here would apply damage twice
    // on the same frame, so this class is now a pure data container.
}
