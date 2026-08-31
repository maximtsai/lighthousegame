using TMPro;
using UnityEngine;

/// <summary>
/// Warms TMP font assets on the main thread before scene load so TextCore
/// background jobs don't call GetName or pixelsPerPoint during threaded deserialization.
/// Runs once per play session.
/// </summary>
public static class TmpFontWarmup
{
    private static bool warmed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void WarmUpBeforeSceneLoad()
    {
        WarmUpAll();
    }

    public static void WarmUpAll()
    {
        if (warmed)
            return;

        warmed = true;

        if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null)
            TouchFont(TMP_Settings.defaultFontAsset);

        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        for (int i = 0; i < fonts.Length; i++)
            TouchFont(fonts[i]);
    }

    private static void TouchFont(TMP_FontAsset font)
    {
        if (font == null)
            return;

        _ = font.name;
        _ = font.faceInfo.familyName;
        font.HasCharacter(' ');
    }
}