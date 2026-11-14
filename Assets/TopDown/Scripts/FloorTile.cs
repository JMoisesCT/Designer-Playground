using UnityEngine;
using UnityEngine.Tilemaps;

namespace TopDown
{
    // Suelo especial (hielo, arena...): se pinta en el Tilemap "Suelo" y cambia cómo camina el jugador encima.
    // Es un Rule Tile: los bordes con el pasto se ponen solos.
    [CreateAssetMenu(menuName = "TopDown/Suelo especial", fileName = "SueloEspecial")]
    public class FloorTile : RuleTile
    {
        [Header("Movimiento encima")]
        [Tooltip("Velocidad del jugador encima: 1 = normal, 0,5 = la mitad, 2 = el doble.")]
        [SerializeField, Range(0.2f, 2f)] private float _speedMultiplier = 1f;

        [Tooltip("Resbala: el jugador tarda en arrancar, frenar y girar.")]
        [SerializeField] private bool _slippery;

        public float SpeedMultiplier => _speedMultiplier;
        public bool Slippery => _slippery;
    }
}
