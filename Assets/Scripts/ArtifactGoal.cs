using UnityEngine;

/// <summary>
/// Collectible artifact.  Handled exclusively by PickupBase via PickupTriggerProxy
/// so that collection → victory runs through a single code path.
///
/// ArtifactGoal now acts as a plain marker component; the actual pickup logic lives
/// in PickupBase.OnTriggerEnter (tag "Artifact") which already calls
/// CollectArtifact → TryTriggerVictory → GameManager.TriggerVictory.
///
/// The class is kept to avoid missing-script errors on existing prefabs.
/// </summary>
public class ArtifactGoal : MonoBehaviour
{
    // No OnTriggerEnter — PickupBase (via PickupTriggerProxy) is the sole authority
    // for artifact collection and victory detection.  Removing the duplicate handler
    // prevents the double "Player collected N artifacts" log and the redundant
    // TryTriggerVictory call that occurred every time the 2nd artifact was grabbed.
}
