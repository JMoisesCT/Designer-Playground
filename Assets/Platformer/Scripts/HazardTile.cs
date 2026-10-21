using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer
{
    // Tile peligroso (pinchos): se pinta en el Tilemap "Terreno" como cualquier otro y el jugador muere al tocarlo.
    [CreateAssetMenu(menuName = "Platformer/Tile peligroso", fileName = "TilePeligroso")]
    public class HazardTile : Tile
    {
    }
}
