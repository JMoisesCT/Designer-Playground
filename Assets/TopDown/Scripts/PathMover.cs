using UnityEngine;

namespace TopDown
{
    // Mueve el objeto por una ruta de puntos que se arrastran en la escena (círculos amarillos).
    // Al llegar al último punto vuelve por el mismo camino o, si es un circuito, sigue hacia donde empezó.
    // Atraviesa árboles y rocas: la ruta es exactamente la que se dibuja.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PathMover : MonoBehaviour
    {
        [Header("Ruta")]
        [Tooltip("Puntos de la ruta, relativos a donde está colocado. Arrástralos en la escena (círculos amarillos). Con el botón 'Añadir punto' alargas la ruta.")]
        [SerializeField] private Vector2[] _points = { new(4f, 0f) };

        [Tooltip("Velocidad, en tiles por segundo. El jugador camina a 5.")]
        [SerializeField, Range(0.5f, 10f)] private float _speed = 2f;

        [Tooltip("Segundos que se queda quieto en cada punto.")]
        [SerializeField, Range(0f, 3f)] private float _waitTime = 0.5f;

        [Tooltip("Activado: del último punto va directo a donde empezó y da vueltas. Desactivado: vuelve por el mismo camino.")]
        [SerializeField] private bool _loop;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;

        private Vector2 _startPosition;
        private int _target = 1; // punto al que va: 0 es donde empieza, 1 el primero de la lista...
        private int _step = 1;
        private float _waitTimer;

        public int PointCount => _points.Length + 1;
        public bool Loop => _loop;
        private Vector2 StartPosition => Application.isPlaying ? _startPosition : (Vector2)transform.position;

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
            if (_points.Length == 0 || _waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                _body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 target = GetPoint(_target);
            Vector2 next = Vector2.MoveTowards(_body.position, target, _speed * Time.fixedDeltaTime);
            // Con velocidad (no con MovePosition) para que WalkAnimator sepa hacia dónde camina.
            _body.linearVelocity = (next - _body.position) / Time.fixedDeltaTime;

            if (next == target)
            {
                NextTarget();
                _waitTimer = _waitTime;
            }
        }

        // Punto de la ruta en el mundo. El 0 es donde empieza.
        public Vector2 GetPoint(int index)
        {
            return index == 0 ? StartPosition : StartPosition + _points[index - 1];
        }

        private void NextTarget()
        {
            if (_loop)
            {
                _target = (_target + 1) % PointCount;
                return;
            }

            // De ida y vuelta: al llegar a un extremo, cambia de sentido.
            if (_target + _step < 0 || _target + _step >= PointCount) _step = -_step;
            _target += _step;
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(PathMover)}: falta el Rigidbody2D.", this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < PointCount; i++)
            {
                Gizmos.DrawWireSphere(GetPoint(i), i == 0 ? 0.15f : 0.25f);
                if (i > 0) Gizmos.DrawLine(GetPoint(i - 1), GetPoint(i));
            }
            if (_loop && _points.Length > 1) Gizmos.DrawLine(GetPoint(_points.Length), GetPoint(0));
        }
    }
}
