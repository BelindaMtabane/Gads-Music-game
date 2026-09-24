using UnityEngine;

/// <summary>
/// Shows the shoe, note, headset, and artefact models as small collectibles.
/// </summary>
public static class PickupPresentation
{
    const float TargetSize = 1.2f;
    const float FloorClearance = 0.16f;
    const int GroundLayerMask = 1 << 6;

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
        if (LevelProgress.CurrentLevel == 3)
            LightDarkPickup(pickup);
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
            if (IsDecoration(child))
                continue;
            if (child.GetComponentInChildren<Renderer>() == null)
                continue;

            if (!TryGetBounds(child, out Bounds bounds))
                continue;

            float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (size > 0.05f && Mathf.Abs(size - TargetSize) > 0.08f)
            {
                child.localScale *= TargetSize / size;
                if (!TryGetBounds(child, out bounds))
                    continue;
            }

            float floorY = FloorHeight(pickup.transform.position);
            float lift = (floorY + FloorClearance) - bounds.min.y;
            child.position += Vector3.up * lift;
        }
    }

    static float FloorHeight(Vector3 position)
    {
        Vector3 origin = new Vector3(position.x, position.y + 8f, position.z);
        var hits = Physics.RaycastAll(origin, Vector3.down, 24f, GroundLayerMask, QueryTriggerInteraction.Ignore);
        float highest = float.NegativeInfinity;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].point.y > highest)
                highest = hits[i].point.y;
        }

        return highest > float.NegativeInfinity ? highest : position.y;
    }

    static void LightDarkPickup(GameObject pickup)
    {
        if (pickup.transform.Find("PickupLight") != null)
            return;

        var lightGo = new GameObject("PickupLight");
        lightGo.transform.SetParent(pickup.transform, false);
        lightGo.transform.localPosition = Vector3.up * 0.5f;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 5f;
        light.intensity = 2.6f;
        light.color = new Color(0.45f, 0.9f, 1f);

        var renderers = pickup.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || IsDecoration(renderers[i].transform))
                continue;

            var mats = renderers[i].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] == null)
                    continue;
                Color c = mats[m].HasProperty("_BaseColor") ? mats[m].GetColor("_BaseColor") : mats[m].color;
                float luma = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                if (luma > 0.28f)
                    continue;

                Color neon = Color.Lerp(new Color(0.25f, 0.85f, 1f), new Color(1f, 0.15f, 0.8f), 0.45f);
                if (mats[m].HasProperty("_BaseColor"))
                    mats[m].SetColor("_BaseColor", neon);
                mats[m].color = neon;
                mats[m].EnableKeyword("_EMISSION");
                mats[m].SetColor("_EmissionColor", neon * 1.8f);
            }
        }
    }

    static void ShrinkTrigger(GameObject pickup)
    {
        var sphere = pickup.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.radius = 0.55f;
            sphere.center = new Vector3(0f, 0.55f, 0f);
        }

        var box = pickup.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = new Vector3(1.1f, 1.1f, 1.1f);
            box.center = new Vector3(0f, 0.55f, 0f);
        }

        var capsule = pickup.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = 0.5f;
            capsule.height = 1.15f;
            capsule.center = new Vector3(0f, 0.58f, 0f);
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
