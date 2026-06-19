using TMPro;
using UnityEngine;

public static class TmpUiUtility
{
    static TMP_FontAsset _cachedFont;

    public static TMP_FontAsset GetDefaultFont()
    {
        if (_cachedFont != null) return _cachedFont;

        _cachedFont = TMP_Settings.defaultFontAsset;
        if (_cachedFont == null)
            _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        return _cachedFont;
    }

    static bool MaterialHasAtlas(Material mat)
    {
        if (mat == null) return false;
        return mat.GetTexture(ShaderUtilities.ID_MainTex) != null;
    }

    public static bool IsReady(TMP_Text text)
    {
        if (text == null || text.font == null) return false;
        return MaterialHasAtlas(text.fontSharedMaterial != null ? text.fontSharedMaterial : text.font.material);
    }

    public static void EnsureFont(TMP_Text text)
    {
        if (text == null) return;
        if (IsReady(text)) return;

        var font = GetDefaultFont();
        if (font == null) return;

        text.font = font;
        var mat = font.material;
        if (MaterialHasAtlas(mat))
            text.fontSharedMaterial = mat;
    }

    public static void SetSafeText(TMP_Text text, string value)
    {
        if (text == null) return;
        EnsureFont(text);
        if (!IsReady(text)) return;
        text.text = value ?? string.Empty;
    }

    public static void FixAllInScene()
    {
        foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            EnsureFont(tmp);
    }
}
