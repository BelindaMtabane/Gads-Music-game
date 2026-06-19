using UnityEngine;

// Collection is handled by PickupBase via PickupTriggerProxy (Speed tag).
public class SpeedBoostPickup : MonoBehaviour
{
    public float speedBonus = 5f;
    public float duration = 5f;
}
