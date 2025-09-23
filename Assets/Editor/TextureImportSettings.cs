using UnityEngine;
using UnityEditor;

public class TextureImportSettings : AssetPostprocessor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;
        // Apply to newly imported textures only
        // Crude check to skip ones we've already touched
        if (!importer.isReadable)
        {
            importer.filterMode = FilterMode.Point;
        }
    }
}
