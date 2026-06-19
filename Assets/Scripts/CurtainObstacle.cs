using System.Collections;
using UnityEngine;

/// <summary>
/// Theater curtain obstacle — slows forward run and strafe when the player runs through it.
/// </summary>
public class CurtainObstacle : MonoBehaviour
{
    [Tooltip("How much to reduce forward speed by")]
    public float forwardSlowAmount = 3f;

    [Tooltip("How much to reduce strafe speed by")]
    public float sidewaySlowAmount = 4f;

    [Tooltip("How long the curtain snare lasts in seconds")]
    public float duration = 2.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!PlayerColliderUtility.IsPlayer(other)) return;

        PlayerMovement movement = PlayerColliderUtility.GetMovement(other);
        if (movement == null) return;

        StartCoroutine(ApplyCurtainEffect(movement));
        PickupCollectUtility.TryConsume(gameObject);
    }

    private IEnumerator ApplyCurtainEffect(PlayerMovement movement)
    {
        movement.forwardSpeed = Mathf.Max(1f, movement.forwardSpeed - forwardSlowAmount);
        movement.sidewaySpeed = Mathf.Max(2f, movement.sidewaySpeed - sidewaySlowAmount);
        yield return new WaitForSeconds(duration);
        movement.forwardSpeed += forwardSlowAmount;
        movement.sidewaySpeed += sidewaySlowAmount;
    }
}
