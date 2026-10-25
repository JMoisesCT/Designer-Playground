using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TopDown
{
    // Cámara que sigue al jugador con suavidad. Si no tiene objetivo, busca el objeto con el tag "Player".
    // Opcionalmente no enseña nada fuera de un rectángulo (los límites del nivel).
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("A quién sigue la cámara. Si se deja vacío, sigue al objeto con el tag Player.")]
        [SerializeField] private Transform _target;

        [Tooltip("Desplazamiento respecto al objetivo (en tiles). Normalmente (0, 0): el jugador en el centro.")]
        [SerializeField] private Vector2 _offset;

        [Tooltip("Tiempo que tarda en alcanzar al objetivo. 0 = lo sigue sin retraso.")]
        [SerializeField, Range(0f, 1f)] private float _smoothTime = 0.15f;

        [Header("Límites")]
        [Tooltip("Si está activo, la cámara nunca enseña nada fuera del rectángulo verde. Arrastra sus esquinas en la escena.")]
        [SerializeField] private bool _useBounds;

        [Tooltip("Esquina de abajo a la izquierda del rectángulo verde.")]
        [SerializeField] private Vector2 _boundsMin = new(-12f, -4f);

        [Tooltip("Esquina de arriba a la derecha del rectángulo verde.")]
        [SerializeField] private Vector2 _boundsMax = new(21f, 14f);

        private Camera _camera;
        private PixelPerfectCamera _pixelPerfect;
        private PlayerHealth _targetHealth;
        private Vector3 _smoothPosition;
        private Vector3 _velocity;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            _pixelPerfect = GetComponent<PixelPerfectCamera>();
            _smoothPosition = transform.position;
            if (_target != null) _targetHealth = _target.GetComponent<PlayerHealth>();

            // Empieza ya sobre el jugador: si reaparece en una bandera, la cámara no cruza el nivel.
            if (_target != null || TryFindPlayer()) _smoothPosition = GetGoal();
        }

        private void LateUpdate()
        {
            if (_target == null && !TryFindPlayer()) return;
            if (_targetHealth != null && _targetHealth.IsDead) return;

            Vector3 targetPosition = _target.position;
            _smoothPosition = Vector3.SmoothDamp(_smoothPosition, GetGoal(), ref _velocity, _smoothTime);

            // Los sprites se dibujan en píxeles de pantalla enteros. Se redondean la posición del objetivo y la distancia
            // a él por separado: así el jugador no vibra un píxel adelante y atrás en pantalla al correr.
            Vector2 snapped = SnapToPixel(targetPosition) + SnapToPixel(_smoothPosition - targetPosition);
            if (_useBounds) snapped = SnapToPixel(ClampToBounds(snapped));
            transform.position = new Vector3(snapped.x, snapped.y, _smoothPosition.z);
        }

        private Vector3 GetGoal()
        {
            Vector2 goal = (Vector2)_target.position + _offset;
            if (_useBounds) goal = ClampToBounds(goal);
            return new Vector3(goal.x, goal.y, _smoothPosition.z);
        }

        // Mueve el centro de la cámara para que la vista quede dentro del rectángulo.
        private Vector2 ClampToBounds(Vector2 center)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            return new Vector2(
                ClampAxis(center.x, _boundsMin.x, _boundsMax.x, halfWidth),
                ClampAxis(center.y, _boundsMin.y, _boundsMax.y, halfHeight));
        }

        // Si el rectángulo es más pequeño que la vista, la cámara se queda en su centro.
        private static float ClampAxis(float value, float min, float max, float halfSize)
        {
            if (max - min < halfSize * 2f) return (min + max) / 2f;
            return Mathf.Clamp(value, min + halfSize, max - halfSize);
        }

        // Usa la misma rejilla que la Pixel Perfect Camera: píxel de pantalla (o de dibujo si tiene Pixel Snapping).
        private Vector2 SnapToPixel(Vector2 value)
        {
            return _pixelPerfect != null ? (Vector2)_pixelPerfect.RoundToPixel(value) : value;
        }

        private bool TryFindPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return false;

            _target = player.transform;
            _targetHealth = player.GetComponent<PlayerHealth>();
            return true;
        }

        private void OnDrawGizmos()
        {
            if (!_useBounds) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((_boundsMin + _boundsMax) / 2f, _boundsMax - _boundsMin);
        }
    }
}
