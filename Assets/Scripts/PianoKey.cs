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
/// One slow at a time. Another wrong note restarts the 10 seconds without stacking the speed cut.
/// </summary>
public class PianoMissSlow : MonoBehaviour
{
    const float SlowAmount = 4f;
    const float Duration = 10f;

    PlayerMovement _movement;
    Coroutine _routine;
    bool _applied;

    public static void Apply(PlayerMovement movement)
    {
        if (movement == null)
            return;

        var slow = movement.GetComponent<PianoMissSlow>();
        if (slow == null)
            slow = movement.gameObject.AddComponent<PianoMissSlow>();
        slow.Begin(movement);
    }

    void Begin(PlayerMovement movement)
    {
        _movement = movement;
        if (!_applied)
        {
            movement.forwardSpeed = Mathf.Max(1f, movement.forwardSpeed - SlowAmount);
            _applied = true;
            AudioManager.Instance?.PlaySlowDownObstacleSfx();
        }

        if (_routine != null)
            StopCoroutine(_routine);
        _routine = StartCoroutine(Recover());
    }

    IEnumerator Recover()
    {
        yield return new WaitForSeconds(Duration);
        if (_applied && _movement != null)
        {
            _movement.forwardSpeed += SlowAmount;
            _applied = false;
        }
        _routine = null;
    }
}
