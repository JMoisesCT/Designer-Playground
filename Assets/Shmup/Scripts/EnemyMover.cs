using UnityEngine;

namespace Shmup
{
    // Mueve a un enemigo hacia donde apunta su morro, en línea recta u ondulando. Gira el objeto para cambiar la dirección.
    [RequireComponent(typeof(Enemy))]
    public class EnemyMover : MonoBehaviour
    {
        private const float GizmoLength = 12f;

        [Header("Movimiento")]
        [Tooltip("Velocidad de avance, en tiles por segundo.")]
        [SerializeField, Range(0f, 15f)] private float _speed = 5f;

        [Tooltip("Cuánto se aparta de la línea recta al ondular, en tiles. 0 = vuela recto.")]
        [SerializeField, Range(0f, 6f)] private float _waveHeight;

        [Tooltip("Segundos que tarda en hacer una onda completa.")]
        [SerializeField, Range(0.2f, 6f)] private float _waveTime = 2f;

        [Header("Quedarse en pantalla")]
        [Tooltip("Si está activo, no se va: al llegar a su sitio se queda en pantalla hasta que lo destruyan. Ideal para jefes.")]
        [SerializeField] private bool _stayOnScreen;

        [Tooltip("Distancia al borde derecho de la pantalla a la que se queda, en tiles.")]
        [SerializeField, Range(1f, 20f)] private float _screenDistance = 5f;

        private Enemy _enemy;
        private Vector2 _center; // punto de la línea recta; la onda se suma encima
        private float _waveClock;

        private void Start()
        {
            _enemy = GetComponent<Enemy>();
            _center = transform.position;
        }

        private void Update()
        {
            if (!_enemy.IsActive) return;

            _center += (Vector2)transform.up * (_speed * _enemy.SpeedMultiplier * Time.deltaTime);
            if (_stayOnScreen) _center.x = Mathf.Max(_center.x, CameraView.RightEdge - _screenDistance);

            _waveClock += Time.deltaTime;
            transform.position = _center + (Vector2)transform.right * Wave(_waveClock);
        }

        private float Wave(float time)
        {
            return Mathf.Sin(time / _waveTime * Mathf.PI * 2f) * _waveHeight;
        }

        // Línea amarilla: el camino que seguirá al entrar en pantalla.
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying || _speed <= 0f) return;

            Gizmos.color = Color.yellow;
            Vector3 previous = transform.position;
            for (float distance = 0.25f; distance <= GizmoLength; distance += 0.25f)
            {
                Vector3 next = transform.position + transform.up * distance + transform.right * Wave(distance / _speed);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
