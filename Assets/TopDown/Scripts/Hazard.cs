using System.Collections;
using UnityEngine;

namespace TopDown
{
    // Quita corazones al jugador mientras lo toca (enemigos, trampas). Necesita un collider con "Is Trigger".
    // El pulso EMP lo paraliza un rato: no se mueve, no dispara y no hace daño.
    public class Hazard : MonoBehaviour
    {
        private static readonly Color StunColor = new(0.4f, 0.6f, 1f);

        [Tooltip("Corazones que quita cada golpe.")]
        [SerializeField, Range(1, 10)] private int _damage = 1;

        private bool _stunned;

        // Se llama en cada paso de la física: tras la invulnerabilidad, si sigue tocándolo, vuelve a hacer daño.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (_stunned) return;
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
        }

        public void Stun(float duration)
        {
            if (!_stunned) StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            // Se apagan los demás scripts del objeto (movimiento, disparo, trampa...) y se tiñe de azul.
            _stunned = true;
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour != this) behaviour.enabled = false;
            }
            if (TryGetComponent(out Rigidbody2D body)) body.linearVelocity = Vector2.zero;
            TryGetComponent(out Animator animator);
            if (animator != null) animator.speed = 0f;
            TryGetComponent(out SpriteRenderer sprite);
            if (sprite != null) sprite.color = StunColor;

            yield return new WaitForSeconds(duration);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                behaviour.enabled = true;
            }
            if (animator != null) animator.speed = 1f;
            if (sprite != null) sprite.color = Color.white;
            _stunned = false;
        }
    }
}
