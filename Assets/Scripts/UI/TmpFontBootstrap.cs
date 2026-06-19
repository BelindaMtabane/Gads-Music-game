using UnityEngine;

/// <summary>
/// Assigns LiberationSans to any scene TMP missing a font (prevents TMP_MaterialManager NRE).
/// </summary>
public static class TmpFontBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded() => TmpUiUtility.FixAllInScene();
}
