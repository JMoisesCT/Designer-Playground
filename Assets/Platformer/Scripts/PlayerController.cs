using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer
{
    // Movimiento del jugador: correr y saltar. Velocidad, impulso de salto y gravedad se ajustan en el Inspector.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private const float CoyoteTime = 0.1f;      // margen para saltar justo después de salir de un borde
        private const float JumpBufferTime = 0.1f;  // margen para pulsar salto un poco antes de tocar el suelo
        private const float JumpCutMultiplier = 0.5f;
        private const float MaxFallSpeed = 20f;

        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int RunState = Animator.StringToHash("Run");
        private static readonly int JumpState = Animator.StringToHash("Jump");
        private static readonly int FallState = Animator.StringToHash("Fall");

        [Header("Movimiento")]
        [Tooltip("Velocidad al correr, en tiles por segundo.")]
        [SerializeField, Range(1f, 20f)] private float _moveSpeed = 7f;

        [Tooltip("Impulso hacia arriba al saltar. Más alto = salta más alto y más lejos.")]
        [SerializeField, Range(1f, 40f)] private float _jumpForce = 16f;

        [Tooltip("Gravedad del personaje. Más alto = cae más rápido y el salto es más bajo.")]
        [SerializeField, Range(0.5f, 10f)] private float _gravity = 4f;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Animator _animator;
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;

        [Tooltip("Capas que cuentan como suelo.")]
        [SerializeField] private LayerMask _groundLayers;

        private ContactFilter2D _groundFilter;
        private float _moveInput;
        private bool _jumpHeld;
        private bool _isJumping;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private int _currentState;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            // Solo cuenta como suelo lo que se toca por debajo (normal entre 45° y 135°).
            _groundFilter.SetLayerMask(_groundLayers);
            _groundFilter.SetNormalAngle(45f, 135f);
        }

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _jumpAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveInput = 0f;
            _jumpHeld = false;
        }

        private void Update()
        {
            _moveInput = _moveAction.action.ReadValue<Vector2>().x;
            bool jumpPressed = _jumpAction.action.IsPressed();
            if (jumpPressed && !_jumpHeld) _jumpBufferTimer = JumpBufferTime;
            _jumpHeld = jumpPressed;

            if (_moveInput != 0f) _sprite.flipX = _moveInput < 0f;
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _body.gravityScale = _gravity;
            Vector2 velocity = _body.linearVelocity;

            IsGrounded = velocity.y <= 0.01f && _body.IsTouching(_groundFilter);
            _coyoteTimer = IsGrounded ? CoyoteTime : _coyoteTimer - dt;
            _jumpBufferTimer -= dt;

            velocity.x = _moveInput * _moveSpeed;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                velocity.y = _jumpForce;
                _isJumping = true;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }

            // Soltar el botón mientras sube corta el salto: toque corto = salto bajo.
            if (_isJumping && !_jumpHeld && velocity.y > 0f)
            {
                velocity.y *= JumpCutMultiplier;
                _isJumping = false;
            }
            if (velocity.y <= 0f) _isJumping = false;

            velocity.y = Mathf.Max(velocity.y, -MaxFallSpeed);
            _body.linearVelocity = velocity;
        }

        // Rebote hacia arriba (por ejemplo, al pisar a un enemigo).
        public void Bounce(float force)
        {
            _body.linearVelocity = new Vector2(_body.linearVelocity.x, force);
            _isJumping = false;
        }

        private void UpdateAnimation()
        {
            int state;
            if (!IsGrounded) state = _body.linearVelocity.y > 0f ? JumpState : FallState;
            else state = Mathf.Abs(_moveInput) > 0.01f ? RunState : IdleState;

            if (state == _currentState) return;
            _currentState = state;
            _animator.Play(state);
        }

        private void OnValidate()
        {
            if (_body == null || _sprite == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerController)}: faltan referencias (Rigidbody2D, SpriteRenderer o Animator).", this);
            if (_moveAction == null || _jumpAction == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerController)}: faltan las acciones de mover o saltar.", this);
        }

        // Dibuja el arco del salto más largo posible: así se ve qué huecos y alturas se pueden superar.
        private void OnDrawGizmosSelected()
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y) * _gravity;
            if (gravity <= 0f) return;

            Collider2D bodyCollider = GetComponent<Collider2D>();
            Vector2 feet = bodyCollider != null
                ? new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.min.y)
                : (Vector2)transform.position;

            float airTime = 2f * _jumpForce / gravity;
            float height = _jumpForce * _jumpForce / (2f * gravity);
            float distance = _moveSpeed * airTime;

            Gizmos.color = Color.yellow;
            const int segments = 30;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 previous = feet;
                for (int i = 1; i <= segments; i++)
                {
                    float t = airTime * i / segments;
                    var point = new Vector2(feet.x + side * _moveSpeed * t, feet.y + _jumpForce * t - 0.5f * gravity * t * t);
                    Gizmos.DrawLine(previous, point);
                    previous = point;
                }
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(feet + Vector2.up * (height + 0.8f),
                $"Salto: {height:0.0} tiles de alto, {distance:0.0} de largo");
#endif
        }
    }
}
