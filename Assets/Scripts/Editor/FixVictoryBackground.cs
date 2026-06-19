using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Replaces the background image on the VictoryScene canvas with the
/// "victory" texture found at Assets/Materials/Textures/victory.
///
/// Run via  Tools → Fix Victory Background
/// </summary>
public static class FixVictoryBackground
{
    [MenuItem("Tools/Fix Victory Background")]
    public static void Run()
    {
        if (SceneManager.GetActiveScene().name != "VictoryScene")
        {
            Debug.LogWarning("[FixVictoryBackground] Open VictoryScene first.");
            return;
        }

        // ── 1. Find the new texture in the project ────────────────────────────
        // Search for any texture/sprite asset named "victory" under Materials/Textures
        Sprite newSprite = null;
        Texture2D newTex = null;

        string[] guids = AssetDatabase.FindAssets("victory t:Sprite", new[] { "Assets/Materials/Textures" });
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("victory t:Sprite");   // widen search

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Debug.Log($"[FixVictoryBackground] Found sprite at: {path}");
        }

        // Fallback: load as Texture2D and create a Sprite from it
        if (newSprite == null)
        {
            string[] texGuids = AssetDatabase.FindAssets("victory t:Texture2D", new[] { "Assets/Materials/Textures" });
            if (texGuids.Length == 0)
                texGuids = AssetDatabase.FindAssets("victory t:Texture2D");

            if (texGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(texGuids[0]);
                newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                // Try to load sub-asset sprite first (if texture was imported as Sprite)
                var allAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in allAtPath)
                {
                    if (a is Sprite s) { newSprite = s; break; }
                }

                if (newSprite == null && newTex != null)
                {
                    // Create a full-rect sprite on the fly
                    newSprite = Sprite.Create(
                        newTex,
                        new Rect(0, 0, newTex.width, newTex.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }

                Debug.Log($"[FixVictoryBackground] Loaded texture at: {path}  ({newTex?.width}×{newTex?.height})");
            }
        }

        if (newSprite == null && newTex == null)
        {
            Debug.LogError("[FixVictoryBackground] Could not find 'victory' texture under Assets/Materials/Textures. " +
                           "Make sure the file is named 'victory' and is imported as Sprite or Texture2D.");
            return;
        }

        // ── 2. Find the background Image in the scene ─────────────────────────
        // Try common names first, then fall back to the first root-level Image
        Image bgImage = null;

        foreach (var name in new[] { "Background", "BG", "BackgroundImage", "bg", "Bg", "VictoryBG", "VictoryBackground" })
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                bgImage = go.GetComponent<Image>();
                if (bgImage != null) break;
            }
        }

        // Fallback: find the Image with the largest rect (most likely the background)
        if (bgImage == null)
        {
            Image largest = null;
            float largestArea = 0f;
            foreach (var img in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                var rt = img.rectTransform;
                float area = rt.rect.width * rt.rect.height;
                if (area > largestArea)
                {
                    largestArea = area;
                    largest = img;
                }
            }
            bgImage = largest;
            if (bgImage != null)
                Debug.Log($"[FixVictoryBackground] Using largest Image as background: '{bgImage.gameObject.name}'");
        }

        if (bgImage == null)
        {
            Debug.LogError("[FixVictoryBackground] No UI Image found in VictoryScene.");
            return;
        }

        // ── 3. Swap the sprite / texture ──────────────────────────────────────
        Undo.RecordObject(bgImage, "Fix Victory Background");

        if (newSprite != null)
        {
            bgImage.sprite = newSprite;
            bgImage.type   = Image.Type.Simple;
            bgImage.preserveAspect = false;
            Debug.Log($"[FixVictoryBackground] Sprite set on '{bgImage.gameObject.name}' → '{newSprite.name}'");
        }

        EditorUtility.SetDirty(bgImage);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixVictoryBackground] Done — VictoryScene saved.");
    }
}
