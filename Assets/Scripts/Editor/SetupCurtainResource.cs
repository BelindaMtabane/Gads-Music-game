using UnityEditor;
using UnityEngine;

/// <summary>
/// Copies curtain_texture.png into Assets/Resources so LevelCutscenePlayer
/// can load it at runtime with Resources.Load.
/// Runs automatically after each recompile and is also available as a menu item.
/// </summary>
[InitializeOnLoad]
public static class SetupCurtainResource
{
    const string Src = "Assets/Materials/Textures/curtain_texture.png";
    const string Dst = "Assets/Resources/curtain_texture.png";

    static SetupCurtainResource()
    {
        EditorApplication.delayCall += CopyIfMissing;
    }

    [MenuItem("Tools/Setup Curtain Resource")]
    public static void CopyIfMissing()
    {
        // Check using AssetDatabase (more reliable than File.Exists in editor)
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(Dst) != null)
            return;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (AssetDatabase.LoadAssetAtPath<Texture2D>(Src) == null)
        {
            Debug.LogWarning("[SetupCurtainResource] Source texture not found at: " + Src);
            return;
        }

        bool ok = AssetDatabase.CopyAsset(Src, Dst);
        if (ok)
        {
            AssetDatabase.Refresh();
            Debug.Log("[SetupCurtainResource] Copied curtain_texture to Assets/Resources/");
        }
        else
        {
            Debug.LogWarning("[SetupCurtainResource] CopyAsset failed — run Tools > Setup Curtain Resource manually.");
        }
    }
}
