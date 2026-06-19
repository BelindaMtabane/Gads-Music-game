using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Level 3 spawn balance — more obstacles, fewer pickups, weighted toward hazards.
/// Run via Tools → Setup Level 3 Spawn Balance.
/// </summary>
public static class SetupLevel3SpawnBalance
{
    public const string ScenePath = "Assets/Scenes/Level3UndergroundDance.unity";
    public const string BodyMatPath = "Assets/Materials/ClubObstacleBody.mat";
    public const string NeonMatPath = "Assets/Materials/ClubObstacleNeon.mat";
    public const string NeonStackName = "Neon Stack - Obstacle";

    [MenuItem("Tools/Setup Level 3 Spawn Balance")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Material bodyMat = EnsureBodyMaterial();
        Material neonMat = EnsureNeonMaterial();
        GameObject neonStack = EnsureNeonStackTemplate(bodyMat, neonMat);
        WireSpawner(neonStack);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupLevel3SpawnBalance] Level 3 spawner balanced — weighted obstacles, fewer pickups.");
    }

    static Material EnsureBodyMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(BodyMatPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, BodyMatPath);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", new Color(0.08f, 0.08f, 0.12f));
        mat.SetColor("_Color", new Color(0.08f, 0.08f, 0.12f));
        mat.SetFloat("_Smoothness", 0.55f);
        mat.SetFloat("_Metallic", 0.2f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material EnsureNeonMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(NeonMatPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, NeonMatPath);
        }

        var neon = new Color(0.95f, 0.15f, 0.95f);
        mat.shader = shader;
        mat.SetColor("_BaseColor", neon);
        mat.SetColor("_Color", neon);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", neon * 2.5f);
        mat.SetFloat("_Smoothness", 0.85f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject EnsureNeonStackTemplate(Material bodyMat, Material neonMat)
    {
        var existing = FindInScene(NeonStackName);
        if (existing != null)
            Object.DestroyImmediate(existing);

        var root = new GameObject(NeonStackName);
        root.tag = "HealthDEC";
        root.transform.position = new Vector3(-5f, 0f, 44f);

        var baseBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseBlock.name = "SpeakerBase";
        baseBlock.transform.SetParent(root.transform, false);
        baseBlock.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        baseBlock.transform.localScale = new Vector3(1.6f, 1.1f, 1.1f);
        Object.DestroyImmediate(baseBlock.GetComponent<Collider>());
        baseBlock.GetComponent<Renderer>().sharedMaterial = bodyMat;

        var neonRing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        neonRing.name = "NeonRing";
        neonRing.transform.SetParent(root.transform, false);
        neonRing.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        neonRing.transform.localScale = new Vector3(1.75f, 0.18f, 1.25f);
        Object.DestroyImmediate(neonRing.GetComponent<Collider>());
        neonRing.GetComponent<Renderer>().sharedMaterial = neonMat;

        var stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stack.name = "SpeakerStack";
        stack.transform.SetParent(root.transform, false);
        stack.transform.localPosition = new Vector3(0f, 2.05f, 0f);
        stack.transform.localScale = new Vector3(1.1f, 0.85f, 1.1f);
        Object.DestroyImmediate(stack.GetComponent<Collider>());
        stack.GetComponent<Renderer>().sharedMaterial = bodyMat;

        var col = root.AddComponent<BoxCollider>();
        col.isTrigger = false;
        col.center = new Vector3(0f, 1.35f, 0f);
        col.size = new Vector3(1.5f, 2.7f, 1.2f);

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var hazard = root.AddComponent<HealthDecreaseObstacle>();
        hazard.damage = 10;
        hazard.vibeDamage = 18;
        hazard.instantKill = false;

        root.SetActive(false);
        EditorUtility.SetDirty(root);
        return root;
    }

    static void WireSpawner(GameObject neonStack)
    {
        var spawnerGo = GameObject.Find("spawnObjects");
        if (spawnerGo == null)
        {
            Debug.LogError("[SetupLevel3SpawnBalance] spawnObjects not found.");
            return;
        }

        var spawner = spawnerGo.GetComponent<Spawner>();
        if (spawner == null)
        {
            Debug.LogError("[SetupLevel3SpawnBalance] Spawner missing.");
            return;
        }

        var randomObstacle = FindInScene("Random Obstacle - health Decrease");
        var slowObstacle   = FindInScene("Slow down  - Obstacle");
        var jumpPickup     = FindInScene("Jump Boost  - Pickup");
        var speedPickup    = FindInScene("Speed boost - Pickup");

        spawner.spawnCount = 13;
        spawner.weightedObjects = new[]
        {
            Entry(randomObstacle, 4),
            Entry(slowObstacle,   4),
            Entry(neonStack,      3),
            Entry(jumpPickup,     1),
            Entry(speedPickup,    1),
        };

        spawner.spawnObjects = new[]
        {
            randomObstacle,
            slowObstacle,
            neonStack,
        };

        EditorUtility.SetDirty(spawner);
    }

    static WeightedSpawnEntry Entry(GameObject prefab, int weight)
    {
        return new WeightedSpawnEntry { prefab = prefab, weight = weight };
    }

    static GameObject FindInScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go;
        return null;
    }
}
