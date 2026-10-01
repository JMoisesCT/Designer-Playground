using UnityEditor;
using UnityEngine;

/// <summary>
/// Aplica automáticamente los ajustes de pixel art a toda textura dentro de las carpetas de arte:
/// PPU = GameConstants.PixelsPerUnit, filtro Point, sin compresión, sin mipmaps y malla Full Rect.
/// No toca el slicing existente. Cualquier PNG nuevo en Assets/Game/Art/ se importa ya como sprite listo.
/// </summary>
public class PixelArtImportPostprocessor : AssetPostprocessor
{
    public static readonly string[] PixelArtFolders =
    {
        "Assets/ThirdParty/Pixel Adventure 1/",
        "Assets/Game/Art/",
    };

    // Subir este número fuerza a Unity a reimportar las texturas con las reglas nuevas.
    public override uint GetVersion() => 1;

    public static bool IsPixelArtPath(string path)
    {
        foreach (string folder in PixelArtFolders)
        {
            if (path.StartsWith(folder, System.StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private void OnPreprocessTexture()
    {
        if (!IsPixelArtPath(assetPath))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;

        if (importer.importSettingsMissing)
        {
            importer.textureType = TextureImporterType.Sprite;
        }

        if (importer.textureType != TextureImporterType.Sprite)
        {
            return;
        }

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spritePixelsPerUnit = GameConstants.PixelsPerUnit;
        settings.filterMode = FilterMode.Point;
        settings.mipmapEnabled = false;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
