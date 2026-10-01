/// <summary>
/// Valores globales del proyecto. Cambiarlos afecta a la escala de todo el juego:
/// decidirlos antes de construir niveles.
/// </summary>
public static class GameConstants
{
    /// <summary>Píxeles por unidad de todos los sprites. 1 tile de 16x16 = 1 unidad.</summary>
    public const int PixelsPerUnit = 16;

    /// <summary>Resolución interna del juego en píxeles de arte (24 x 13.5 tiles visibles).</summary>
    public const int ReferenceResolutionX = 384;
    public const int ReferenceResolutionY = 216;
}
