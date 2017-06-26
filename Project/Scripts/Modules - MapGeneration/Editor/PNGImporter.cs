using UnityEngine;
using UnityEditor;

public sealed class MapImporter: AssetPostprocessor
{
    /// <summary>
    /// PNGs containing this substring will be treated as textures representing maps, and imported with settings
    /// accordingly.
    /// </summary>
    public const string MAP_SUBSTRING = "_map";

    void OnPreprocessTexture()
    {
        string assetPathLowerCase = assetPath.ToLower();
        if (assetPathLowerCase.Contains(MAP_SUBSTRING))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.isReadable = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.alphaSource = TextureImporterAlphaSource.None;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
