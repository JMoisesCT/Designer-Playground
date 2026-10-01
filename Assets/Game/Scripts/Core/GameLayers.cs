/// <summary>
/// Nombres de capas del proyecto. Única fuente de verdad: el setup del editor
/// las crea a partir de aquí y el código las usa en lugar de strings sueltos.
/// </summary>
public static class GameLayers
{
    /// <summary>Capas de física (GameObject → Layer).</summary>
    public static class Physics
    {
        public const string Ground = "Ground";
        public const string OneWayPlatform = "OneWayPlatform";
        public const string Player = "Player";
        public const string Hazard = "Hazard";
        public const string Collectible = "Collectible";
        public const string Interactable = "Interactable";

        public static readonly string[] All =
        {
            Ground, OneWayPlatform, Player, Hazard, Collectible, Interactable
        };
    }

    /// <summary>Sorting Layers (SpriteRenderer → Sorting Layer), de atrás hacia delante.</summary>
    public static class Sorting
    {
        public const string Background = "Background";
        public const string Default = "Default";
        public const string Terrain = "Terrain";
        public const string Traps = "Traps";
        public const string Items = "Items";
        public const string Player = "Player";
        public const string Foreground = "Foreground";
        public const string FX = "FX";

        public static readonly string[] All =
        {
            Background, Default, Terrain, Traps, Items, Player, Foreground, FX
        };
    }
}
