using UnityEngine;
using UnityEngine.Tilemaps;

namespace TopDown
{
    // Tile peligroso (pinchos): se pinta en el suelo y quita un corazón al jugador que lo pisa.
    [CreateAssetMenu(menuName = "TopDown/Tile peligroso", fileName = "TilePeligroso")]
    public class HazardTile : Tile
    {
    }
}
