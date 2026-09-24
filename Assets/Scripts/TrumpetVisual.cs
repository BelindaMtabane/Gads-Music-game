using UnityEngine;

/// <summary>
/// Builds a gold trumpet on the collectible artifact so the pickup reads as an instrument.
/// </summary>
public static class TrumpetVisual
{
    public static void AttachTo(GameObject artifact)
    {
        if (artifact == null || artifact.transform.Find("Trumpet") != null)
            return;

        var renderer = artifact.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        HideOtherChildren(artifact.transform);

        var gold = CreateGold();
        var root = new GameObject("Trumpet").transform;
        root.SetParent(artifact.transform, false);
        root.localPosition = new Vector3(0f, 0.16f, 0f);
        root.localRotation = Quaternion.Euler(8f, 40f, 78f);
        root.localScale = Vector3.one;

        // About half a metre long, next to a two-metre runner.
        AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.02f), new Vector3(0.028f, 0.16f, 0.028f), Quaternion.Euler(90f, 0f, 0f), gold);
        AddPart(PrimitiveType.Sphere, root, new Vector3(0f, 0f, 0.22f), new Vector3(0.11f, 0.11f, 0.07f), Quaternion.identity, gold);
        AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0f, -0.18f), new Vector3(0.016f, 0.035f, 0.016f), Quaternion.Euler(90f, 0f, 0f), gold);

        for (int i = 0; i < 3; i++)
        {
            float z = -0.02f + i * 0.045f;
            AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0.04f, z), new Vector3(0.012f, 0.02f, 0.012f), Quaternion.identity, gold);
        }
    }

    static void HideOtherChildren(Transform artifact)
    {
        for (int i = 0; i < artifact.childCount; i++)
        {
            var child = artifact.GetChild(i);
            if (child.name == "ArtifactGlow")
                continue;
            child.gameObject.SetActive(false);
        }

        var sphere = artifact.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.radius = 0.45f;
            sphere.center = new Vector3(0f, 0.2f, 0f);
        }
    }

    static void AddPart(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material mat)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = "TrumpetPart";
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = rotation;
        part.transform.localScale = scale;

        var partRenderer = part.GetComponent<Renderer>();
        if (partRenderer != null)
            partRenderer.sharedMaterial = mat;

        var collider = part.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);
    }

    static Material CreateGold()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader);
        Color gold = new Color(0.83f, 0.69f, 0.22f);
        mat.color = gold;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", gold * 0.45f);
        return mat;
    }
}
