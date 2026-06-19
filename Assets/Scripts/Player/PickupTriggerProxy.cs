using UnityEngine;

/// <summary>
/// Sits on a child GameObject "PickupTrigger" of the Player.
///
/// WHY THIS EXISTS:
/// Unity suppresses CapsuleCollider trigger events on any GameObject that also
/// has a CharacterController — the CharacterController takes over the capsule
/// physics and the separate CapsuleCollider is effectively ignored for OnTriggerEnter.
///
/// Moving the trigger collider to a child object bypasses that limitation:
/// child objects are unaffected by the parent's CharacterController, so their
/// trigger events fire normally.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class PickupTriggerProxy : MonoBehaviour
{
    private PickupBase _pickup;

    private void Awake()
    {
        _pickup = GetComponentInParent<PickupBase>();
        EnsureCollider();
    }

    private void Start()
    {
        EnsureCollider();
    }

    void EnsureCollider()
    {
        var col = GetComponent<CapsuleCollider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();
        col.isTrigger = true;

        var cc = GetComponentInParent<CharacterController>();
        if (cc != null)
        {
            col.center = cc.center;
            col.height = cc.height;
            col.radius = cc.radius + 0.35f;
            col.direction = 1;
        }

        GameplayCollisionUtility.EnsureKinematicRigidbody(gameObject);
    }

    private void OnTriggerEnter(Collider other) => Forward(other);

    private void OnTriggerStay(Collider other) => Forward(other);

    void Forward(Collider other)
    {
        if (_pickup == null || other == null) return;
        if (GameplayCollisionUtility.IsSolidObstacleRoot(other.gameObject)) return;
        _pickup.HandleTrigger(other);
    }
}
