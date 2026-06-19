using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Puts visible pedestal cubes under spawn pickups so artifacts and power-ups are easy to spot.
/// Run via Tools → Setup Pickup Pedestals.
/// </summary>
public static class SetupPickupPedestals
{
    public const string MainGameScene = GameplaySceneNames.L1Path;

    private const string PedestalName = "Pedestal";

    [MenuItem("Tools/Setup Pickup Pedestals")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene(MainGameScene, OpenSceneMode.Single);

        ApplyPedestal(FindPickup("Artifact - Goal"), "Assets/Images/colour1 4.mat", 1.8f, 1.1f, 1.5f);
        ApplyPedestal(FindPickup("Health - Pickup"), "Assets/Images/colour1 5.mat", 1.4f, 0.9f, 1.25f);
        ApplyPedestal(FindPickup("Jump Boost  - Pickup"), "Assets/Images/colour1 2.mat", 1.4f, 0.9f, 1.25f);
        ApplyPedestal(FindPickup("Speed boost - Pickup"), "Assets/Images/colour1 3.mat", 1.4f, 0.9f, 1.25f);
        ApplyPedestal(FindPickup("Sneak - Pickup"), "Assets/Images/colour1 6.mat", 1.4f, 0.9f, 1.25f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupPickupPedestals] Pedestal cubes added under spawn pickups in MainGameL1.");
    }

    private static GameObject FindPickup(string objectName)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!GameplaySceneNames.IsGameplayScene(go.scene.name)) continue;
            if (go.name == objectName && go.transform.parent == null)
                return go;
        }

        return null;
    }

    private static void ApplyPedestal(GameObject pickup, string materialPath, float pedestalWidth, float pedestalHeight, float modelLift)
    {
        if (pickup == null)
        {
            Debug.LogWarning($"[SetupPickupPedestals] Pickup not found for material {materialPath}.");
            return;
        }

        var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (mat == null)
        {
            Debug.LogWarning($"[SetupPickupPedestals] Missing material at {materialPath}.");
            return;
        }

        Transform root = pickup.transform;
        Transform pedestal = root.Find(PedestalName);
        if (pedestal == null)
        {
            var pedestalGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestalGo.name = PedestalName;
            pedestalGo.transform.SetParent(root, false);
            Object.DestroyImmediate(pedestalGo.GetComponent<Collider>());
            pedestal = pedestalGo.transform;
        }

        pedestal.localPosition = new Vector3(0f, pedestalHeight * 0.5f, 0f);
        pedestal.localRotation = Quaternion.identity;
        pedestal.localScale = new Vector3(pedestalWidth, pedestalHeight, pedestalWidth);

        var pedestalRenderer = pedestal.GetComponent<MeshRenderer>();
        if (pedestalRenderer != null)
            pedestalRenderer.sharedMaterial = mat;

        bool isArtifact = pickup.CompareTag("Artifact");
        float modelScale = isArtifact ? 1.6f : 1.2f;

        foreach (Transform child in root)
        {
            if (child == pedestal) continue;

            Vector3 pos = child.localPosition;
            if (pos.y < modelLift)
                child.localPosition = new Vector3(pos.x, modelLift, pos.z);

            child.localScale *= modelScale;
        }

        AdjustTriggerCollider(pickup, pedestalHeight, modelLift, isArtifact);
        EditorUtility.SetDirty(pickup);
    }

    private static void AdjustTriggerCollider(GameObject pickup, float pedestalHeight, float modelLift, bool isArtifact)
    {
        float triggerHeight = modelLift + (isArtifact ? 1.2f : 0.8f);

        if (pickup.TryGetComponent<BoxCollider>(out var box))
        {
            box.center = new Vector3(0f, triggerHeight * 0.5f, 0f);
            box.size = new Vector3(isArtifact ? 2.2f : 1.8f, triggerHeight, isArtifact ? 2.2f : 1.8f);
            return;
        }

        if (pickup.TryGetComponent<SphereCollider>(out var sphere))
        {
            sphere.center = new Vector3(0f, modelLift * 0.5f, 0f);
            sphere.radius = isArtifact ? 1.35f : 1.1f;
            return;
        }

        if (pickup.TryGetComponent<CapsuleCollider>(out var capsule))
        {
            capsule.center = new Vector3(0f, modelLift * 0.5f, 0f);
            capsule.height = triggerHeight;
            capsule.radius = isArtifact ? 1.1f : 0.9f;
        }
    }
}
