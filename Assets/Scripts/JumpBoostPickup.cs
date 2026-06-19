using UnityEngine;

// Collection is handled by PickupBase via PickupTriggerProxy (JumpBoost tag).
public class JumpBoostPickup : MonoBehaviour
{
    public float jumpBonus = 5f;
    public float duration = 5f;
}
