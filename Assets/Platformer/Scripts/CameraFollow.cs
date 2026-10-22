using UnityEngine;

namespace Platformer
{
    // Cámara que sigue al jugador con suavidad. Si no tiene objetivo, busca el objeto con el tag "Player".
    public class CameraFollow : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;

        [Tooltip("A quién sigue la cámara. Si se deja vacío, sigue al objeto con el tag Player.")]
        [SerializeField] private Transform _target;

        [Tooltip("Desplazamiento respecto al objetivo (en tiles). Y positivo = se ve más de lo que hay arriba.")]
        [SerializeField] private Vector2 _offset = new(0f, 1.5f);

        [Tooltip("Tiempo que tarda en alcanzar al objetivo. 0 = lo sigue sin retraso.")]
        [SerializeField, Range(0f, 1f)] private float _smoothTime = 0.15f;

        private PlayerHealth _targetHealth;
        private Vector3 _smoothPosition;
        private Vector3 _velocity;

        private void Start()
        {
            _smoothPosition = transform.position;
            if (_target != null) _targetHealth = _target.GetComponent<PlayerHealth>();
        }

        private void LateUpdate()
        {
            if (_target == null && !TryFindPlayer()) return;
            if (_targetHealth != null && _targetHealth.IsDead) return;

            Vector3 targetPosition = _target.position;
            Vector3 goal = targetPosition + (Vector3)_offset;
            goal.z = _smoothPosition.z;
            _smoothPosition = Vector3.SmoothDamp(_smoothPosition, goal, ref _velocity, _smoothTime);

            // Los sprites se dibujan en píxeles enteros. Se redondean la posición del objetivo y la distancia a él
            // por separado: así el jugador no vibra un píxel adelante y atrás en pantalla al correr.
            Vector2 snapped = SnapToPixel(targetPosition) + SnapToPixel(_smoothPosition - targetPosition);
            transform.position = new Vector3(snapped.x, snapped.y, _smoothPosition.z);
        }

        private static Vector2 SnapToPixel(Vector2 value)
        {
            return new Vector2(Mathf.Round(value.x * PixelsPerUnit), Mathf.Round(value.y * PixelsPerUnit)) / PixelsPerUnit;
        }

        private bool TryFindPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return false;

            _target = player.transform;
            _targetHealth = player.GetComponent<PlayerHealth>();
            return true;
        }
    }
}
