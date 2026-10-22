using UnityEngine;

namespace Platformer
{
    // Enemigo: mata al jugador al tocarlo, salvo que el jugador le caiga encima; entonces el enemigo muere y el jugador rebota.
    public class StompableEnemy : MonoBehaviour
    {
        private static readonly int HitState = Animator.StringToHash("Hit");

        [Header("Pisotón")]
        [Tooltip("Si está activo, el jugador puede eliminarlo saltándole encima.")]
        [SerializeField] private bool _canBeStomped = true;

        [Tooltip("Impulso hacia arriba que recibe el jugador al pisarlo.")]
        [SerializeField, Range(0f, 30f)] private float _bounceForce = 12f;

        [Header("Referencias")]
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;
        [SerializeField] private PatrolMover _mover;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player == null || player.IsDead) return;

            // Pisotón = el jugador cae y sus pies están por encima de la mitad del enemigo.
            // "Acaba de rebotar" también vale: si pisa dos enemigos a la vez, el primero ya lo ha lanzado hacia arriba.
            PlayerController controller = player.GetComponent<PlayerController>();
            bool falling = other.attachedRigidbody != null && other.attachedRigidbody.linearVelocity.y < 0f;
            bool fromAbove = other.bounds.min.y > _collider.bounds.center.y;

            if (_canBeStomped && (falling || controller.JustBounced) && fromAbove)
            {
                controller.Bounce(_bounceForce);
                Defeat();
            }
            else
            {
                player.Die();
            }
        }

        private void Defeat()
        {
            _collider.enabled = false;
            if (_mover != null) _mover.enabled = false;
            if (_animator != null) _animator.Play(HitState);
            Destroy(gameObject, 0.4f);
        }

        private void OnValidate()
        {
            if (_collider == null) Debug.LogWarning($"[{name}] {nameof(StompableEnemy)}: falta el Collider2D.", this);
        }
    }
}
