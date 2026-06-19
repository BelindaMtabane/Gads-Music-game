using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Applies Goethe playtest feedback: HUD readability, run speed, level length.
/// </summary>
public static class SetupGoetheImprovements
{
    public const string MainGameScene = GameplaySceneNames.L1Path;

    [MenuItem("Tools/Setup All Goethe Improvements")]
    public static void RunAll()
    {
        PickupSfxGenerator.GenerateAll();
        SetupMainMenu.Setup();
        SetupAudio.Setup();
        SetupCurtainObstacle.Setup();
        SetupPickupPedestals.Setup();
        ApplyMainGame();
        Debug.Log("[SetupGoetheImprovements] Full Goethe improvement pass complete.");
    }

    [MenuItem("Tools/Setup Goethe Feedback")]
    public static void ApplyMainGame()
    {
        var scene = EditorSceneManager.OpenScene(MainGameScene, OpenSceneMode.Single);

        ApplyHudText("health", 46, new Color(0.95f, 0.95f, 0.95f, 1f));
        ApplyHudText("artifact", 46, new Color(1f, 0.82f, 0.15f, 1f));
        ApplyHudText("Hitsleft", 46, new Color(0.95f, 0.95f, 0.95f, 1f));
        ApplyHudText("Sneak", 46, new Color(0.75f, 0.9f, 1f, 1f));

        var playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.baseForwardSpeed = 8f;
            playerMovement.baseSidewaySpeed = 8f;
            playerMovement.forwardSpeed = 8f;
            playerMovement.sidewaySpeed = 8f;
            EditorUtility.SetDirty(playerMovement);
        }

        var spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner != null)
        {
            spawner.spawnCount = 8;
            EditorUtility.SetDirty(spawner);
        }

        var curtainSpawner = Object.FindAnyObjectByType<CurtainSpawnController>();
        if (curtainSpawner != null)
        {
            curtainSpawner.firstSpawnDelay = 6f;
            curtainSpawner.spawnInterval = 10f;
            EditorUtility.SetDirty(curtainSpawner);
        }

        WireRunLengthAndArtifacts();

        var hud = Object.FindAnyObjectByType<HUDfunctions>();
        if (hud != null)
            EditorUtility.SetDirty(hud);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupGoetheImprovements] HUD fonts, run speed, and spawn density updated for MainGameL1.");
    }

    private static void ApplyHudText(string objectName, int fontSize, Color color)
    {
        var go = GameObject.Find(objectName);
        if (go == null) return;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;

        var rect = go.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(320f, 56f);

        EditorUtility.SetDirty(tmp);
    }

    private static void WireRunLengthAndArtifacts()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null && canvas.GetComponent<GameplayCanvasGuard>() == null)
            canvas.AddComponent<GameplayCanvasGuard>();

        var spawner = Object.FindAnyObjectByType<Spawner>();
        var artifact = GameObject.Find("Artifact - Goal");
        var ground = GameObject.Find("Ground");
        var player = GameObject.FindGameObjectWithTag("Player");

        if (spawner != null && artifact != null)
        {
            spawner.artifactTemplate = artifact;
            spawner.artifactsPerSegment = 1;

            var list = new List<GameObject>();
            if (spawner.spawnObjects != null)
                list.AddRange(spawner.spawnObjects);
            list.RemoveAll(go => go == null || go == artifact || go.CompareTag("Artifact"));
            spawner.spawnObjects = list.ToArray();
            EditorUtility.SetDirty(spawner);
        }

        var runLength = Object.FindAnyObjectByType<RunLengthController>();
        if (runLength == null)
        {
            var go = new GameObject("RunLengthController");
            runLength = go.AddComponent<RunLengthController>();
        }

        if (ground != null) runLength.groundPrefab = ground;
        if (spawner != null) runLength.spawner = spawner;
        if (player != null) runLength.player = player.transform;
        runLength.lookAhead = 100f;
        EditorUtility.SetDirty(runLength);
    }
}
