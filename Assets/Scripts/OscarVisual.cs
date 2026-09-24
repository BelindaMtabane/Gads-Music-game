using UnityEngine;

/// <summary>
/// Museum artifact: a gold award about 34cm tall, the height of a real Oscar.
/// </summary>
public static class OscarVisual
{
    public static void AttachTo(GameObject artifact)
    {
        if (artifact == null || artifact.transform.Find("Oscar") != null)
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
            sphere.radius = 0.4f;
            sphere.center = new Vector3(0f, 0.18f, 0f);
        }

        var gold = Create(new Color(0.92f, 0.76f, 0.28f), 0.55f);
        var black = Create(new Color(0.08f, 0.08f, 0.09f), 0f);
        var root = new GameObject("Oscar").transform;
        root.SetParent(artifact.transform, false);
        root.localPosition = new Vector3(0f, 0.02f, 0f);
        root.localRotation = Quaternion.identity;

        Add(PrimitiveType.Cube, root, new Vector3(0f, 0.015f, 0f), new Vector3(0.12f, 0.03f, 0.09f), Quaternion.identity, black);
        Add(PrimitiveType.Cylinder, root, new Vector3(0f, 0.045f, 0f), new Vector3(0.07f, 0.008f, 0.07f), Quaternion.identity, gold);
        Add(PrimitiveType.Capsule, root, new Vector3(0f, 0.16f, 0f), new Vector3(0.045f, 0.07f, 0.04f), Quaternion.identity, gold);
        Add(PrimitiveType.Sphere, root, new Vector3(0f, 0.28f, 0f), new Vector3(0.055f, 0.055f, 0.05f), Quaternion.identity, gold);
        Add(PrimitiveType.Cube, root, new Vector3(0.02f, 0.2f, 0.03f), new Vector3(0.012f, 0.16f, 0.012f), Quaternion.Euler(0f, 0f, -8f), gold);
    }

    static void Add(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material mat)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = "OscarPart";
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
