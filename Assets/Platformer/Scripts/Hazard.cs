using UnityEngine;

namespace Platformer
{
    // Mata al jugador al tocarlo (sierras, zonas mortales...). Sirve con collider normal o con "Is Trigger".
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            KillPlayer(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            KillPlayer(collision.collider);
        }

        private static void KillPlayer(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) player.Die();
        }
    }
}
