using UnityEngine;

/// <summary>
/// Hides the plain cube on pickups so the shoe, note, headset, and artefact models show.
/// </summary>
public static class PickupPresentation
{
    public static void Reveal()
    {
        RevealTag("Speed");
        RevealTag("Sneak");
        RevealTag("HealthINC");
        RevealTag("JumpBoost");
        RevealTag("Artifact");
    }

    public static void RevealObject(GameObject pickup)
    {
        if (pickup == null)
            return;
        if (!IsPickup(pickup))
            return;
        Reveal(pickup);
    }

    static void RevealTag(string tag)
    {
        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (!SafeTag(transforms[i], tag))
                continue;
            Reveal(transforms[i].gameObject);
        }
    }

    static void Reveal(GameObject pickup)
    {
        bool hasModel = false;
        var renderers = pickup.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject == pickup)
                continue;
            if (!renderers[i].gameObject.activeSelf)
                continue;
            hasModel = true;
            renderers[i].enabled = true;
        }

        if (!hasModel)
            return;

        var cube = pickup.GetComponent<MeshRenderer>();
        if (cube != null)
            cube.enabled = false;
    }

    static bool IsPickup(GameObject pickup)
    {
        return SafeTag(pickup.transform, "Speed")
            || SafeTag(pickup.transform, "Sneak")
            || SafeTag(pickup.transform, "HealthINC")
            || SafeTag(pickup.transform, "JumpBoost")
            || SafeTag(pickup.transform, "Artifact");
    }

    static bool SafeTag(Transform t, string tag)
    {
        try
        {
            return t.CompareTag(tag);
        }
        catch (UnityException)
        {
            return false;
        }
    }
}
