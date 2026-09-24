using System.Collections;
using UnityEngine;

/// <summary>
/// One white key in a floor piano. Open keys are safe lanes. Red keys are closed.
/// </summary>
public class PianoKey : MonoBehaviour
{
    public bool correct;
    public float minX;
    public float maxX;

    bool _resolved;

    void OnTriggerStay(Collider other)
    {
        if (_resolved || !PlayerColliderUtility.IsPlayer(other))
            return;

        PlayerMovement movement = PlayerColliderUtility.GetMovement(other);
        if (movement == null)
            return;

        float x = movement.transform.position.x;
        if (x < minX || x >= maxX)
            return;

        var body = movement.GetComponent<CharacterController>();
        if (body != null && !body.isGrounded)
            return;

        _resolved = true;
        var guard = Object.FindAnyObjectByType<EnemyBase>();
        if (guard == null)
            return;

        if (correct)
            guard.RelievePianoPressure();
        else
            guard.AddPianoPressure();
    }
}

/// <summary>
/// One slow at a time, for 5 seconds. A speed pickup cancels it so only the boost remains.
/// </summary>
public class PianoMissSlow : MonoBehaviour
{
    const float SlowAmount = 4f;
    const float Duration = 5f;

    PlayerMovement _movement;
    Coroutine _routine;
    bool _applied;

    public bool IsActive => _applied;

    public static void Apply(PlayerMovement movement)
    {
        if (movement == null)
            return;

        // Don't cut speed out from under a boost that is already running.
        var pickup = movement.GetComponent<PickupBase>();
        if (pickup != null && pickup.IsSpeedBoostActive)
            return;

        var slow = movement.GetComponent<PianoMissSlow>();
        if (slow == null)
            slow = movement.gameObject.AddComponent<PianoMissSlow>();
        slow.Begin(movement);
    }

    // Don't add the lost speed back. SpeedBoost writes the new speed itself.
    public void Cancel()
    {
        if (_routine != null)
            StopCoroutine(_routine);
        _routine = null;
        _applied = false;
    }

    void Begin(PlayerMovement movement)
    {
        _movement = movement;
        if (!_applied)
        {
            movement.forwardSpeed = Mathf.Max(1f, movement.forwardSpeed - SlowAmount);
            _applied = true;
            AudioManager.Instance?.PlaySlowDownObstacleSfx();
            Object.FindAnyObjectByType<EnemyBase>()?.AddPianoPressure();
        }

        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(Recover());
    }

    IEnumerator Recover()
    {
        yield return new WaitForSeconds(Duration);
        if (_applied && _movement != null)
            _movement.forwardSpeed = _movement.baseForwardSpeed;
        _applied = false;
        _routine = null;
    }
}

/// <summary>
/// Guitar strings slow the runner. Lasers are only a touch hit.
/// </summary>
public class TrackContact : MonoBehaviour
{
    public bool slowPlayer;
    public bool speedGuard;
    bool _done;

    void OnTriggerEnter(Collider other)
    {
        if (_done || !PlayerColliderUtility.IsPlayer(other))
            return;

        _done = true;
        if (slowPlayer)
            PianoMissSlow.Apply(PlayerColliderUtility.GetMovement(other));

        if (speedGuard)
            Object.FindAnyObjectByType<EnemyBase>()?.AddPianoPressure();

        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
}
