using UnityEngine;

namespace TopDown
{
    // Pone la animación de caminar o de estar quieto mirando hacia donde se mueve el objeto (para enemigos).
    public class WalkAnimator : MonoBehaviour
    {
        // Estados del Animator, en el orden de las columnas de las hojas del pack: abajo, arriba, izquierda, derecha.
        private static readonly int[] IdleStates = Hashes("IdleDown", "IdleUp", "IdleLeft", "IdleRight");
        private static readonly int[] WalkStates = Hashes("WalkDown", "WalkUp", "WalkLeft", "WalkRight");

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Animator _animator;

        private int _direction; // 0 abajo, 1 arriba, 2 izquierda, 3 derecha
        private int _currentState;

        private void Update()
        {
            Vector2 velocity = _body.linearVelocity;
            bool moving = velocity.sqrMagnitude > 0.01f;

            // Mira hacia el eje en el que más se mueve (en diagonal, gana el horizontal).
            if (moving)
            {
                if (Mathf.Abs(velocity.x) >= Mathf.Abs(velocity.y)) _direction = velocity.x < 0f ? 2 : 3;
                else _direction = velocity.y < 0f ? 0 : 1;
            }

            int state = (moving ? WalkStates : IdleStates)[_direction];
            if (state == _currentState) return;
            _currentState = state;
            _animator.Play(state);
        }

        private static int[] Hashes(params string[] names) => System.Array.ConvertAll(names, Animator.StringToHash);

        private void OnValidate()
        {
            if (_body == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(WalkAnimator)}: faltan referencias (Rigidbody2D o Animator).", this);
        }
    }
}
