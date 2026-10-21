using UnityEngine;

namespace Platformer
{
    // Cámara que sigue al jugador con suavidad. Si no tiene objetivo, busca el objeto con el tag "Player".
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("A quién sigue la cámara. Si se deja vacío, sigue al objeto con el tag Player.")]
        [SerializeField] private Transform _target;

        [Tooltip("Desplazamiento respecto al objetivo (en tiles). Y positivo = se ve más de lo que hay arriba.")]
        [SerializeField] private Vector2 _offset = new(0f, 1.5f);

        [Tooltip("Tiempo que tarda en alcanzar al objetivo. 0 = lo sigue sin retraso.")]
        [SerializeField, Range(0f, 1f)] private float _smoothTime = 0.15f;

        private PlayerHealth _targetHealth;
        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (_target == null && !TryFindPlayer()) return;
            if (_targetHealth != null && _targetHealth.IsDead) return;

            Vector3 goal = _target.position + (Vector3)_offset;
            goal.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
        }

        private bool TryFindPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return false;

            _target = player.transform;
            _targetHealth = player.GetComponent<PlayerHealth>();
            return true;
        }

        private void Start()
        {
            if (_target != null) _targetHealth = _target.GetComponent<PlayerHealth>();
        }
    }
}
