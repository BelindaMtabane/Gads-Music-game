using UnityEngine;

/// <summary>
/// Shows the shoe, note, headset, and artefact models as small collectibles.
/// </summary>
public static class PickupPresentation
{
    const float TargetSize = 0.42f;

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
        if (pickup.transform.Find("PickupSized") != null)
            return;

        for (int i = 0; i < pickup.transform.childCount; i++)
        {
            var child = pickup.transform.GetChild(i);
            if (child.name == "Pedestal")
                child.gameObject.SetActive(false);
        }

        bool hasModel = false;
        var renderers = pickup.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject == pickup)
                continue;
            if (!renderers[i].gameObject.activeInHierarchy)
                continue;
            if (IsDecoration(renderers[i].transform))
                continue;
            renderers[i].enabled = true;
            hasModel = true;
        }

        if (!hasModel)
            return;

        var cube = pickup.GetComponent<MeshRenderer>();
        if (cube != null)
            cube.enabled = false;

        FitModels(pickup);
        if (!HasThemedArtifact(pickup.transform))
            ShrinkTrigger(pickup);

        var marker = new GameObject("PickupSized");
        marker.transform.SetParent(pickup.transform, false);
    }

    static void FitModels(GameObject pickup)
    {
        for (int i = 0; i < pickup.transform.childCount; i++)
        {
            var child = pickup.transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
                continue;
            if (IsDecoration(child) || IsThemedArtifact(child.name))
                continue;
            if (child.GetComponentInChildren<Renderer>() == null)
                continue;

            if (!TryGetBounds(child, out Bounds bounds))
                continue;

            float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (size > TargetSize + 0.08f)
            {
                child.localScale *= TargetSize / size;
                if (!TryGetBounds(child, out bounds))
                    continue;
            }

            float lift = (pickup.transform.position.y + 0.06f) - bounds.min.y;
            child.position += Vector3.up * lift;
        }
    }

    static void ShrinkTrigger(GameObject pickup)
    {
        var sphere = pickup.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.radius = 0.32f;
            sphere.center = new Vector3(0f, 0.2f, 0f);
        }

        var box = pickup.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = new Vector3(0.5f, 0.5f, 0.5f);
            box.center = new Vector3(0f, 0.22f, 0f);
        }

        var capsule = pickup.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = 0.28f;
            capsule.height = 0.6f;
            capsule.center = new Vector3(0f, 0.28f, 0f);
        }
    }

    static bool TryGetBounds(Transform model, out Bounds bounds)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        bounds = new Bounds(model.position, Vector3.zero);
        bool any = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled)
                continue;
            if (!any)
            {
                bounds = renderers[i].bounds;
                any = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        return any && bounds.size.sqrMagnitude > 0.0001f;
    }

    static bool HasThemedArtifact(Transform pickup)
    {
        for (int i = 0; i < pickup.childCount; i++)
        {
            if (IsThemedArtifact(pickup.GetChild(i).name))
                return true;
        }
        return false;
    }

    static bool IsThemedArtifact(string name)
    {
        return name == "Trumpet" || name == "Oscar" || name == "Vinyl";
    }

    static bool IsDecoration(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (current.name == "Pedestal" || current.name == "ArtifactGlow" || current.name == "PickupSized")
                return true;
            current = current.parent;
        }
        return false;
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
