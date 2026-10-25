using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    // Movimiento del jugador visto desde arriba: camina en 8 direcciones y mira hacia donde se mueve.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        // Estados del Animator, en el orden de las columnas de las hojas del pack: abajo, arriba, izquierda, derecha.
        private static readonly int[] IdleStates = Hashes("IdleDown", "IdleUp", "IdleLeft", "IdleRight");
        private static readonly int[] WalkStates = Hashes("WalkDown", "WalkUp", "WalkLeft", "WalkRight");

        [Header("Movimiento")]
        [Tooltip("Velocidad al caminar, en tiles por segundo.")]
        [SerializeField, Range(1f, 15f)] private float _moveSpeed = 5f;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Animator _animator;
        [SerializeField] private InputActionReference _moveAction;

        private Vector2 _moveInput;
        private int _direction; // 0 abajo, 1 arriba, 2 izquierda, 3 derecha
        private int _currentState;

        private void OnEnable()
        {
            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            _body.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            _moveInput = Vector2.ClampMagnitude(_moveAction.action.ReadValue<Vector2>(), 1f);

            // Mira hacia el eje en el que más se mueve (en diagonal, gana el horizontal).
            if (_moveInput.sqrMagnitude > 0.01f)
            {
                if (Mathf.Abs(_moveInput.x) >= Mathf.Abs(_moveInput.y)) _direction = _moveInput.x < 0f ? 2 : 3;
                else _direction = _moveInput.y < 0f ? 0 : 1;
            }
            PlayAnimation(_moveInput.sqrMagnitude > 0.01f ? WalkStates : IdleStates);
        }

        private void FixedUpdate()
        {
            _body.linearVelocity = _moveInput * _moveSpeed;
        }

        // Deja de responder a los controles y se queda quieto (por ejemplo, al llegar a la meta).
        public void Stop()
        {
            enabled = false;
            PlayAnimation(IdleStates);
        }

        private void PlayAnimation(int[] states)
        {
            int state = states[_direction];
            if (state == _currentState) return;
            _currentState = state;
            _animator.Play(state);
        }

        private static int[] Hashes(params string[] names) => System.Array.ConvertAll(names, Animator.StringToHash);

        private void OnValidate()
        {
            if (_body == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerController)}: faltan referencias (Rigidbody2D o Animator).", this);
            if (_moveAction == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerController)}: falta la acción de mover.", this);
        }
    }
}
