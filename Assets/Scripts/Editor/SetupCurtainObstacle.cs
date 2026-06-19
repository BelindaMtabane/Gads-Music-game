using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Curtain texture, prefab, and side-only random spawning (no fixed track walls).
/// Run via Tools → Setup Curtain Obstacle.
/// </summary>
public static class SetupCurtainObstacle
{
    public const string TexturePath = "Assets/Materials/Textures/curtain_texture.png";
    public const string MaterialPath = "Assets/Materials/Curtain.mat";
    public const string PrefabPath = "Assets/prefab/Curtain.prefab";
    public const string MainGameScene = "Assets/Scenes/MainGameL1.unity";

    private static readonly Vector3 SpawnCurtainScale = new Vector3(7.5f, 6f, 0.45f);

    [MenuItem("Tools/Setup Curtain Obstacle")]
    public static void Setup()
    {
        EnsureCurtainTag();
        EnsureTextureImport();
        Material curtainMat = EnsureMaterial();

        var scene = EditorSceneManager.OpenScene(MainGameScene, OpenSceneMode.Single);
        RemoveFixedCurtains();
        ClearPianoDoors();

        GameObject spawnTemplate = EnsureSpawnTemplate(curtainMat);
        SavePrefab(spawnTemplate);
        WireSpawner(spawnTemplate);
        EnsureCurtainSpawnController();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupCurtainObstacle] Fixed curtains removed; side-only random curtain spawn wired.");
    }

    private static void EnsureCurtainTag()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == "Curtain")
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Curtain";
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureTextureImport()
    {
        var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[SetupCurtainObstacle] Missing texture at {TexturePath}");
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static Material EnsureMaterial()
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null)
        {
            Debug.LogError("[SetupCurtainObstacle] Could not load curtain texture.");
            return null;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        mat.shader = shader;
        mat.SetTexture("_BaseMap", texture);
        mat.SetTexture("_MainTex", texture);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Smoothness", 0.2f);
        mat.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void RemoveFixedCurtains()
    {
        var toRemove = new System.Collections.Generic.List<GameObject>();
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.StartsWith("DoorCurtain") || go.name.StartsWith("Curtain ("))
                toRemove.Add(go);
        }

        foreach (var go in toRemove)
            Object.DestroyImmediate(go);
    }

    private static void ClearPianoDoors()
    {
        var puzzle = Object.FindAnyObjectByType<PianoPuzzle>();
        if (puzzle == null) return;

        puzzle.doors = System.Array.Empty<GameObject>();
        EditorUtility.SetDirty(puzzle);
    }

    private static GameObject EnsureSpawnTemplate(Material curtainMat)
    {
        var spawnerRoot = GameObject.Find("spawnObjects");
        Transform parent = spawnerRoot != null ? spawnerRoot.transform : null;

        GameObject curtain = GameObject.Find("Curtain");
        if (curtain == null)
        {
            curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curtain.name = "Curtain";
        }

        if (parent != null)
            curtain.transform.SetParent(parent, false);

        curtain.transform.localPosition = Vector3.zero;
        curtain.transform.localRotation = Quaternion.identity;
        curtain.transform.localScale = SpawnCurtainScale;
        curtain.SetActive(false);
        curtain.tag = "Curtain";

        var renderer = curtain.GetComponent<MeshRenderer>();
        if (renderer != null && curtainMat != null)
            renderer.sharedMaterial = curtainMat;

        var collider = curtain.GetComponent<BoxCollider>() ?? curtain.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        var obstacle = curtain.GetComponent<CurtainObstacle>() ?? curtain.AddComponent<CurtainObstacle>();
        obstacle.forwardSlowAmount = 3f;
        obstacle.sidewaySlowAmount = 4f;
        obstacle.duration = 2.5f;

        EditorUtility.SetDirty(curtain);
        return curtain;
    }

    private static void WireSpawner(GameObject spawnTemplate)
    {
        var spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner == null)
        {
            Debug.LogError("[SetupCurtainObstacle] Spawner not found in MainGameL1.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        spawner.curtainPrefab = prefab != null ? prefab : spawnTemplate;
        spawner.curtainSpawnChance = 0.7f;

        var list = new System.Collections.Generic.List<GameObject>(spawner.spawnObjects ?? System.Array.Empty<GameObject>());
        list.RemoveAll(go => go == null || go.CompareTag("Curtain") || go.name == "Curtain");
        spawner.spawnObjects = list.ToArray();

        EditorUtility.SetDirty(spawner);
    }

    private static void EnsureCurtainSpawnController()
    {
        var spawnerRoot = GameObject.Find("spawnObjects");
        if (spawnerRoot == null) return;

        var spawner = spawnerRoot.GetComponent<Spawner>();
        var controller = spawnerRoot.GetComponent<CurtainSpawnController>() ?? spawnerRoot.AddComponent<CurtainSpawnController>();
        controller.spawner = spawner;
        controller.firstSpawnDelay = 8f;
        controller.spawnInterval = 12f;
        controller.spawnChance = 0.8f;
        EditorUtility.SetDirty(controller);
    }

    private static void SavePrefab(GameObject spawnTemplate)
    {
        if (spawnTemplate == null) return;

        if (!AssetDatabase.IsValidFolder("Assets/prefab"))
            AssetDatabase.CreateFolder("Assets", "prefab");

        bool wasActive = spawnTemplate.activeSelf;
        spawnTemplate.SetActive(true);
        PrefabUtility.SaveAsPrefabAsset(spawnTemplate, PrefabPath);
        spawnTemplate.SetActive(wasActive);
    }
}
