using UnityEngine;

namespace Platformer
{
    // Mueve el objeto de ida y vuelta entre donde está colocado y un punto final que se arrastra en la escena.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolMover : MonoBehaviour
    {
        [Header("Recorrido")]
        [Tooltip("Punto final del recorrido, relativo a donde está colocado. También puedes arrastrar el círculo amarillo en la escena.")]
        [SerializeField] private Vector2 _endOffset = new(4f, 0f);

        [Tooltip("Velocidad, en tiles por segundo.")]
        [SerializeField, Range(0.5f, 15f)] private float _speed = 2f;

        [Tooltip("Segundos que se queda quieto en cada extremo.")]
        [SerializeField, Range(0f, 3f)] private float _waitTime = 0f;

        [Tooltip("Gira el dibujo para que mire hacia donde camina.")]
        [SerializeField] private bool _flipSprite = true;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private SpriteRenderer _sprite;

        private Vector2 _startPosition;
        private bool _goingToEnd = true;
        private float _waitTimer;

        public Vector2 EndOffset => _endOffset;
        public Vector2 StartPosition => Application.isPlaying ? _startPosition : (Vector2)transform.position;

        private void Awake()
        {
            _startPosition = _body.position;
            _body.bodyType = RigidbodyType2D.Kinematic;
        }

        private void OnDisable()
        {
            _body.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                _body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 target = _goingToEnd ? _startPosition + _endOffset : _startPosition;
            Vector2 next = Vector2.MoveTowards(_body.position, target, _speed * Time.fixedDeltaTime);

            if (_flipSprite && _sprite != null && !Mathf.Approximately(target.x, _body.position.x))
                _sprite.flipX = target.x < _body.position.x;

            // Se mueve con velocidad (no con MovePosition) para que el jugador sepa a qué velocidad va
            // la plataforma móvil sobre la que está.
            _body.linearVelocity = (next - _body.position) / Time.fixedDeltaTime;

            if (next == target)
            {
                _goingToEnd = !_goingToEnd;
                _waitTimer = _waitTime;
            }
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(PatrolMover)}: falta el Rigidbody2D.", this);
        }

        private void OnDrawGizmos()
        {
            Vector2 start = StartPosition;
            Vector2 end = start + _endOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, 0.15f);
            Gizmos.DrawWireSphere(end, 0.3f);
        }
    }
}
