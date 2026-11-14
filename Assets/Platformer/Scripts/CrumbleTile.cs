using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer
{
    // Tile de suelo frágil: se pinta en el Tilemap "Terreno" y se cae poco después de que el jugador lo pise (ver TileCrumbler).
    [CreateAssetMenu(menuName = "Platformer/Tile frágil", fileName = "TileFragil")]
    public class CrumbleTile : Tile
    {
        [Tooltip("Segundos que tiembla desde que el jugador lo pisa hasta que cae.")]
        [SerializeField, Range(0f, 3f)] private float _delay = 0.4f;

        [Tooltip("Segundos hasta que vuelve a aparecer. 0 = no vuelve.")]
        [SerializeField, Range(0f, 20f)] private float _respawnTime = 4f;

        [Tooltip("Sonido al caer. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        public float Delay => _delay;
        public float RespawnTime => _respawnTime;
        public AudioClip CrumbleSound => _sound;
    }
}
