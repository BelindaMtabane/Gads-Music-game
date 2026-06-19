using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Level 2 museum statue obstacle — medium hazard that drains Vibe directly.
/// Swaps out the Health pickup template for the statue on Level2Museum.
/// Run via Tools → Setup Museum Statue (Level 2).
/// </summary>
public static class SetupMuseumStatueObstacle
{
    public const string ScenePath = "Assets/Scenes/Level2Museum.unity";
    public const string MaterialPath = "Assets/Materials/MuseumStatue.mat";
    public const string TemplateName = "Museum Statue - Obstacle";
    public const string RemovedPickupName = "Health - Pickup";

    [MenuItem("Tools/Setup Museum Statue (Level 2)")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Material stoneMat = EnsureMaterial();
        GameObject template = EnsureStatueTemplate(stoneMat);
        WireSpawner(template);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupMuseumStatueObstacle] Museum statue added to Level2Museum; Health pickup removed from spawn pool.");
    }

    static Material EnsureMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", new Color(0.58f, 0.55f, 0.5f));
        mat.SetColor("_Color", new Color(0.58f, 0.55f, 0.5f));
        mat.SetFloat("_Smoothness", 0.35f);
        mat.SetFloat("_Metallic", 0.05f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject EnsureStatueTemplate(Material mat)
    {
        var existing = FindInScene(TemplateName);
        if (existing != null)
            Object.DestroyImmediate(existing);

        var root = new GameObject(TemplateName);
        root.tag = "HealthDEC";
        root.transform.position = new Vector3(6f, 0f, 42f);

        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pedestal.name = "Pedestal";
        pedestal.transform.SetParent(root.transform, false);
        pedestal.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        pedestal.transform.localScale = new Vector3(1.5f, 0.7f, 1.2f);
        Object.DestroyImmediate(pedestal.GetComponent<Collider>());

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "StatueBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        body.transform.localScale = new Vector3(0.85f, 1.05f, 0.85f);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "StatueHead";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0f, 2.55f, 0f);
        head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        Object.DestroyImmediate(head.GetComponent<Collider>());

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterial = mat;

        var col = root.AddComponent<BoxCollider>();
        col.isTrigger = false;
        col.center = new Vector3(0f, 1.45f, 0f);
        col.size = new Vector3(1.35f, 2.9f, 1.15f);

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var hazard = root.AddComponent<HealthDecreaseObstacle>();
        hazard.damage = 8;
        hazard.vibeDamage = 22;
        hazard.instantKill = false;

        root.SetActive(false);
        EditorUtility.SetDirty(root);
        return root;
    }

    static void WireSpawner(GameObject statueTemplate)
    {
        var spawnerGo = GameObject.Find("spawnObjects");
        if (spawnerGo == null)
        {
            Debug.LogError("[SetupMuseumStatueObstacle] spawnObjects not found.");
            return;
        }

        var spawner = spawnerGo.GetComponent<Spawner>();
        if (spawner == null)
        {
            Debug.LogError("[SetupMuseumStatueObstacle] Spawner component missing.");
            return;
        }

        var list = new System.Collections.Generic.List<GameObject>();
        foreach (var entry in spawner.spawnObjects ?? System.Array.Empty<GameObject>())
        {
            if (entry == null) continue;
            if (entry.name == RemovedPickupName) continue;
            list.Add(entry);
        }

        if (!list.Contains(statueTemplate))
            list.Add(statueTemplate);

        spawner.spawnObjects = list.ToArray();
        spawner.weightedObjects = System.Array.Empty<WeightedSpawnEntry>();
        EditorUtility.SetDirty(spawner);
    }

    static GameObject FindInScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go;
        return null;
    }
}
