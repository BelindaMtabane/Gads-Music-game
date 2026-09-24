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

        var gold = CreateGold();
        var root = new GameObject("Trumpet").transform;
        root.SetParent(artifact.transform, false);
        root.localPosition = new Vector3(-0.7f, 0.9f, 0f);
        root.localRotation = Quaternion.Euler(-12f, 25f, 0f);
        root.localScale = Vector3.one * 0.9f;

        AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.15f), new Vector3(0.11f, 0.42f, 0.11f), Quaternion.Euler(90f, 0f, 0f), gold);
        AddPart(PrimitiveType.Sphere, root, new Vector3(0f, 0f, 0.62f), new Vector3(0.34f, 0.34f, 0.22f), Quaternion.identity, gold);
        AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0f, -0.42f), new Vector3(0.07f, 0.12f, 0.07f), Quaternion.Euler(90f, 0f, 0f), gold);

        for (int i = 0; i < 3; i++)
        {
            float z = -0.05f + i * 0.13f;
            AddPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0.12f, z), new Vector3(0.045f, 0.08f, 0.045f), Quaternion.identity, gold);
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
