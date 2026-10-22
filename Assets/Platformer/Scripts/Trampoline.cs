using UnityEngine;

namespace Platformer
{
    // Trampolín: el jugador que cae encima sale disparado hacia arriba.
    public class Trampoline : MonoBehaviour
    {
        private static readonly int JumpState = Animator.StringToHash("Jump");

        [Tooltip("Impulso hacia arriba. Con el trampolín seleccionado, la línea verde muestra hasta dónde sube el jugador.")]
        [SerializeField, Range(5f, 50f)] private float _bounceForce = 25f;

        [Header("Referencias")]
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            PlayerController player = collision.collider.GetComponentInParent<PlayerController>();
            if (player == null) return;

            // Solo rebota si los pies del jugador están encima (no al chocar de lado).
            if (collision.collider.bounds.min.y < _collider.bounds.max.y - 0.1f) return;

            player.Bounce(_bounceForce);
            _animator.Play(JumpState, 0, 0f);
        }

        private void OnValidate()
        {
            if (_collider == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(Trampoline)}: faltan referencias.", this);
        }

        // Línea con la altura del rebote, calculada con la gravedad del jugador de la escena.
        private void OnDrawGizmosSelected()
        {
            if (_collider == null) return;
            PlayerController player = FindFirstObjectByType<PlayerController>();
            float gravity = Mathf.Abs(Physics2D.gravity.y) * (player != null ? player.Gravity : 4f);
            float height = _bounceForce * _bounceForce / (2f * gravity);

            Vector3 top = new Vector3(_collider.bounds.center.x, _collider.bounds.max.y, 0f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(top, top + Vector3.up * height);
            Gizmos.DrawWireSphere(top + Vector3.up * height, 0.2f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(top + Vector3.up * (height + 0.6f), $"Rebote: {height:0.0} tiles");
#endif
        }
    }
}
