using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Show/hide inactive spawn templates in the Scene view for sizing and layout.
/// Templates are grayed out at runtime so only spawned clones appear in play mode.
/// </summary>
[InitializeOnLoad]
public static class SpawnTemplateEditor
{
    const string PreviewRootName = "SpawnTemplatePreview";
    const float PreviewSpacing = 7f;
    static readonly Vector3 PreviewOrigin = new Vector3(0f, 1.2f, 38f);

    static readonly string[] PickupObstacleNames =
    {
        "Random Obstacle - health Decrease",
        "Slow down  - Obstacle",
        "Sneak - Pickup",
        "Speed boost - Pickup",
        "Jump Boost  - Pickup",
        "Health - Pickup",
        "Artifact - Goal",
        "Curtain",
    };

    static SpawnTemplateEditor()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                HideTemplates(silent: true);
        };
    }

    [MenuItem("Tools/Show Spawn Templates (Edit Scene)")]
    public static void ShowTemplatesMenu()
    {
        if (!IsGameplayScene())
        {
            Debug.LogWarning("[SpawnTemplateEditor] Open MainGameL1, L2, or L3 first.");
            return;
        }

        ShowTemplates();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SpawnTemplateEditor] Templates are visible for editing. Run Tools → Hide Spawn Templates before play, or they auto-hide when you press Play.");
    }

    [MenuItem("Tools/Hide Spawn Templates")]
    public static void HideTemplatesMenu()
    {
        if (!IsGameplayScene()) return;
        HideTemplates(silent: false);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Resize Artifact Music Note")]
    public static void ResizeMusicNoteMenu()
    {
        if (!IsGameplayScene())
        {
            Debug.LogWarning("[SpawnTemplateEditor] Open a MainGameL scene first.");
            return;
        }

        ResizeArtifactMusicNote(1.15f);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SpawnTemplateEditor] musicnote-artifact scaled to 1.15 and placeholder mesh hidden on Artifact - Goal.");
    }

    public static void ShowTemplates()
    {
        var root = EnsurePreviewRoot();
        var templates = CollectTemplates();
        int index = 0;

        foreach (var go in templates)
        {
            if (go == null) continue;

            Spawner.PrepareSpawnedClone(go);
            go.SetActive(true);

            if (go.transform.parent != root.transform)
                go.transform.SetParent(root.transform, true);

            int col = index % 3;
            int row = index / 3;
            go.transform.localPosition = new Vector3(
                (col - 1) * PreviewSpacing,
                0f,
                -row * PreviewSpacing);

            EditorUtility.SetDirty(go);
            index++;
        }

        root.SetActive(true);
        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    public static void HideTemplates(bool silent)
    {
        foreach (var name in PickupObstacleNames)
        {
            var go = FindInOpenScene(name);
            if (go == null) continue;

            if (go.transform.parent != null && go.transform.parent.name == PreviewRootName)
                go.transform.SetParent(null, true);

            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }

        var root = GameObject.Find(PreviewRootName);
        if (root != null)
            root.SetActive(false);

        if (!silent)
            Debug.Log("[SpawnTemplateEditor] Spawn templates hidden again (normal for play mode).");
    }

    public static void ResizeArtifactMusicNote(float uniformScale)
    {
        var artifact = FindInOpenScene("Artifact - Goal");
        if (artifact == null)
        {
            Debug.LogWarning("[SpawnTemplateEditor] Artifact - Goal not found.");
            return;
        }

        foreach (Transform child in artifact.transform)
        {
            if (!child.name.Contains("musicnote")) continue;
            child.localScale = Vector3.one * uniformScale;
            child.localPosition = new Vector3(child.localPosition.x, 1.1f, child.localPosition.z);
            EditorUtility.SetDirty(child.gameObject);
        }

        var parentRenderer = artifact.GetComponent<MeshRenderer>();
        if (parentRenderer != null)
        {
            parentRenderer.enabled = false;
            EditorUtility.SetDirty(parentRenderer);
        }

        if (artifact.TryGetComponent<SphereCollider>(out var sphere))
        {
            sphere.radius = 0.85f;
            sphere.center = new Vector3(0f, 0.9f, 0f);
            EditorUtility.SetDirty(sphere);
        }

        EditorUtility.SetDirty(artifact);
    }

    static List<GameObject> CollectTemplates()
    {
        var list = new List<GameObject>();
        foreach (var name in PickupObstacleNames)
        {
            var go = FindInOpenScene(name);
            if (go != null)
                list.Add(go);
        }
        return list;
    }

    static GameObject EnsurePreviewRoot()
    {
        var existing = GameObject.Find(PreviewRootName);
        if (existing != null)
            return existing;

        var root = new GameObject(PreviewRootName);
        root.transform.position = PreviewOrigin;
        return root;
    }

    static bool IsGameplayScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        return scene.StartsWith("MainGameL");
    }

    static GameObject FindInOpenScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            if (go.name == name) return go;
        }
        return null;
    }
}
