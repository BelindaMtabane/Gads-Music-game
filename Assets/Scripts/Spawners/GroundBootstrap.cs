using UnityEngine;

/// <summary>
/// Keeps the scene's static Ground tile active with a reliable floor collider.
/// </summary>
public static class GroundBootstrap
{
    public static void EnsureSceneGround(GameObject ground)
    {
        if (ground == null) return;

        ground.SetActive(true);

        if (ground.GetComponent<SceneGroundAnchor>() == null)
            ground.AddComponent<SceneGroundAnchor>();

        var meshFilter = ground.GetComponent<MeshFilter>();
        var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        if (mesh == null) return;

        var box = ground.GetComponent<BoxCollider>();
        if (box == null)
            box = ground.AddComponent<BoxCollider>();

        Physics.SyncTransforms();
        var meshBounds = mesh.bounds;
        var lossy = ground.transform.lossyScale;
        float sizeX = Mathf.Abs(meshBounds.size.x * lossy.x);
        float sizeZ = Mathf.Abs(meshBounds.size.z * lossy.z);
        const float floorHeight = 1f;

        box.center = new Vector3(
            meshBounds.center.x * lossy.x,
            meshBounds.center.y * lossy.y - floorHeight * 0.5f + 0.02f,
            meshBounds.center.z * lossy.z);
        box.size = new Vector3(sizeX, floorHeight, sizeZ);
        box.isTrigger = false;

        var meshCol = ground.GetComponent<MeshCollider>();
        if (meshCol != null)
            meshCol.enabled = false;
    }
}
