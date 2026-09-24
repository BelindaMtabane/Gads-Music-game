using UnityEngine;

/// <summary>
/// A trumpet beam packed with notes. Dodging through it is safe. Running into it slows the player.
/// </summary>
public class NoteBeam : MonoBehaviour
{
    bool _resolved;

    void OnTriggerStay(Collider other)
    {
        if (_resolved || !PlayerColliderUtility.IsPlayer(other))
            return;

        PlayerMovement movement = PlayerColliderUtility.GetMovement(other);
        if (movement == null)
            return;

        _resolved = true;
        if (!movement.IsDodging)
            PianoMissSlow.Apply(movement);
    }
}

/// <summary>
/// A cluster of dancers blocking one lane. Touching them can drop one carried artifact.
/// </summary>
public class DanceCrowd : MonoBehaviour
{
    bool _taken;

    public void TryTakeArtifact(PickupBase pickup)
    {
        if (_taken || pickup == null)
            return;
        if (!pickup.LoseArtifact())
            return;
        _taken = true;
    }
}

/// <summary>
/// Bobs and turns a dancer so the crowd reads as moving.
/// </summary>
public class DancerBob : MonoBehaviour
{
    float _phase;
    float _baseY;

    void Start()
    {
        _phase = Random.Range(0f, 6.28f);
        _baseY = transform.localPosition.y;
    }

    void Update()
    {
        if (!GameManager.GameStarted)
            return;

        Vector3 pos = transform.localPosition;
        pos.y = _baseY + Mathf.Sin(Time.time * 6.5f + _phase) * 0.16f;
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 2.4f + _phase) * 28f, 0f);
    }
}
