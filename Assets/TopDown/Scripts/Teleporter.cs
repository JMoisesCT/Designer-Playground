using UnityEngine;

namespace TopDown
{
    // Círculo mágico que lleva al jugador a otro sitio (círculo morado que se arrastra en la escena).
    // Para ir y volver, pon dos: cada uno con su salida al lado del otro (no encima).
    public class Teleporter : MonoBehaviour
    {
        [Tooltip("Adónde lleva, relativo al teletransporte. También puedes arrastrar el círculo morado en la escena.")]
        [SerializeField] private Vector2 _exitOffset = new(6f, 0f);

        [Tooltip("Activado: también lleva cajas y perseguidores, que se quedan a vigilar al otro lado.")]
        [SerializeField] private bool _affectsEverything;

        [Tooltip("Sonido al teletransportarse. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        public Vector2 Exit => (Vector2)transform.position + _exitOffset;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Solo lo que se mueve con física (jugador, cajas, perseguidores), y no las zonas de daño de los enemigos.
            Rigidbody2D body = other.attachedRigidbody;
            if (other.isTrigger || body == null || body.bodyType != RigidbodyType2D.Dynamic) return;

            bool isPlayer = body.CompareTag("Player");
            if (!isPlayer && !_affectsEverything) return;

            // Se mueve el objeto entero: su posición es la de los pies + lo que haya entre los pies y el centro.
            Vector2 offset = body.position - (Vector2)other.bounds.center;
            body.position = Exit + offset;
            body.transform.position = body.position;
            if (body.TryGetComponent(out ChaserEnemy chaser)) chaser.MoveHome(body.position);
            Sound.Play(_sound);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.3f, 1f);
            Gizmos.DrawLine(transform.position, Exit);
            Gizmos.DrawWireSphere(Exit, 0.4f);
        }
    }
}
