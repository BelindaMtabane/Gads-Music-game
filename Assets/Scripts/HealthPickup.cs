using UnityEngine;

// Collection is handled by PickupBase via PickupTriggerProxy (HealthINC tag).
public class HealthPickup : MonoBehaviour
{
    [Tooltip("Legacy — healing is applied in PickupBase.")]
    public int healAmount = 20;
}
