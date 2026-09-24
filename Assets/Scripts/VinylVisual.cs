using UnityEngine;

/// <summary>
/// Club artifact: a 12-inch vinyl disc, about 30cm across.
/// </summary>
public static class VinylVisual
{
    public static void AttachTo(GameObject artifact)
    {
        if (artifact == null || artifact.transform.Find("Vinyl") != null)
            return;

        var renderer = artifact.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        for (int i = 0; i < artifact.transform.childCount; i++)
        {
            var child = artifact.transform.GetChild(i);
            if (child.name != "ArtifactGlow")
                child.gameObject.SetActive(false);
        }

        var sphere = artifact.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.radius = 0.35f;
            sphere.center = new Vector3(0f, 0.18f, 0f);
        }

        var vinyl = Create(new Color(0.05f, 0.05f, 0.06f), 0.08f);
        var label = Create(new Color(0.85f, 0.15f, 0.45f), 0.35f);
        var hole = Create(new Color(0.02f, 0.02f, 0.02f), 0f);

        var root = new GameObject("Vinyl").transform;
        root.SetParent(artifact.transform, false);
        root.localPosition = new Vector3(0f, 0.18f, 0f);
        root.localRotation = Quaternion.Euler(68f, 15f, 0f);

        Add(PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(0.3f, 0.004f, 0.3f), Quaternion.identity, vinyl);
        Add(PrimitiveType.Cylinder, root, new Vector3(0f, 0.006f, 0f), new Vector3(0.1f, 0.004f, 0.1f), Quaternion.identity, label);
        Add(PrimitiveType.Cylinder, root, new Vector3(0f, 0.01f, 0f), new Vector3(0.012f, 0.004f, 0.012f), Quaternion.identity, hole);
    }

    static void Add(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material mat)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = "VinylPart";
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

    static Material Create(Color color, float emission)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = color;
        if (emission > 0f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
        }
        return mat;
    }
}
