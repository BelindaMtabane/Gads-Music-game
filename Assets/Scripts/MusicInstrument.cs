using UnityEngine;

/// <summary>
/// Collectible instrument artifact.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicInstrument : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Sound to play when collected")]
    public AudioClip instrumentSound;

    [Tooltip("How many artifacts this counts as")]
    public int artifactValue = 1;

    [Tooltip("Money value added to the scoreboard")]
    public int moneyValue = 500;

    private AudioSource _audio;
    private bool        _collected = false;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        if (instrumentSound != null)
            _audio.clip = instrumentSound;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || !PlayerColliderUtility.IsPlayer(other)) return;
        _collected = true;

        PickupBase pickup = PlayerColliderUtility.GetPickup(other);
        if (pickup != null)
            pickup.CollectArtifact(artifactValue, moneyValue);

        AudioManager.Instance?.PlayArtifactPickupSfx();
        PickupCollectUtility.TryConsume(gameObject);
    }
}
