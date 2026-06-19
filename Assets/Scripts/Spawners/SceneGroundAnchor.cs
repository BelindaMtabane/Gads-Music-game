using UnityEngine;

/// <summary>
/// Marks the scene's static starting floor. Never removed during a run.
/// </summary>
public class SceneGroundAnchor : MonoBehaviour
{
    private bool _pickupsPopulated;

    public bool TryPopulatePickups(Spawner spawner)
    {
        if (_pickupsPopulated || spawner == null)
            return false;

        _pickupsPopulated = true;
        spawner.SpawnGameObjects(gameObject);
        return true;
    }
}
