using UnityEngine;

/// <summary>
/// Hides a drum obstacle and shows a microphone that uses the same floor roll.
/// </summary>
public static class ClubMicrophone
{
    public static void ReplaceLoadedDrums()
    {
        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name.IndexOf("Drum", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            Replace(transforms[i].gameObject);
        }
    }

    public static void Replace(GameObject go)
    {
        if (go == null || !ContainsDrum(go))
            return;

        Transform host = go.transform;
        while (host.parent != null && !IsContainer(host.parent))
            host = host.parent;

        if (host.GetComponent<PlayerMovement>() != null)
            return;
        if (host.Find("Microphone") != null)
            return;

        var renderers = host.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;

        var mic = new GameObject("Microphone").transform;
        mic.SetParent(host, false);
        mic.localPosition = new Vector3(0f, 0.15f, 0f);
        mic.localRotation = Quaternion.Euler(70f, 0f, 0f);

        var metal = CreateMetal();
        var dark = CreateDark();
        AddPart(PrimitiveType.Capsule, mic, new Vector3(0f, 0f, 0f), new Vector3(0.16f, 0.42f, 0.16f), Quaternion.identity, metal);
        AddPart(PrimitiveType.Sphere, mic, new Vector3(0f, 0.55f, 0f), new Vector3(0.38f, 0.38f, 0.38f), Quaternion.identity, dark);
        AddPart(PrimitiveType.Cylinder, mic, new Vector3(0f, 0.55f, 0.16f), new Vector3(0.22f, 0.04f, 0.22f), Quaternion.Euler(90f, 0f, 0f), metal);
    }

    static bool ContainsDrum(GameObject go)
    {
        var transforms = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name.IndexOf("Drum", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    static bool IsContainer(Transform t)
    {
        if (t.GetComponent<Spawner>() != null) return true;
        if (t.GetComponent<GroundSegmentLifetime>() != null) return true;
        if (t.name == "spawnObjects") return true;
        if (t.name.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return false;
    }

    static void AddPart(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material mat)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = "MicPart";
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = rotation;
        part.transform.localScale = scale;
        var renderer = part.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;
        var collider = part.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);
    }

    static Material CreateMetal()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        Color silver = new Color(0.78f, 0.8f, 0.84f);
        mat.color = silver;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", silver * 0.25f);
        return mat;
    }

    static Material CreateDark()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = new Color(0.08f, 0.08f, 0.1f);
        return mat;
    }
}
