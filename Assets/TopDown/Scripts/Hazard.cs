using UnityEngine;

namespace TopDown
{
    // Quita corazones al jugador mientras lo toca (enemigos, trampas). Necesita un collider con "Is Trigger".
    public class Hazard : MonoBehaviour
    {
        [Tooltip("Corazones que quita cada golpe.")]
        [SerializeField, Range(1, 10)] private int _damage = 1;

        // Se llama en cada paso de la física: tras la invulnerabilidad, si sigue tocándolo, vuelve a hacer daño.
        private void OnTriggerStay2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
        }
    }
}
