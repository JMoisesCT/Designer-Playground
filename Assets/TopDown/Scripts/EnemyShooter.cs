using UnityEngine;

namespace TopDown
{
    // Hace que un enemigo dispare al jugador cada cierto tiempo cuando está cerca.
    public class EnemyShooter : MonoBehaviour
    {
        [Tooltip("Segundos entre disparos.")]
        [SerializeField, Range(0.3f, 6f)] private float _fireInterval = 2f;

        [Tooltip("Solo dispara si el jugador está a menos de esta distancia, en tiles (círculo naranja).")]
        [SerializeField, Range(1f, 20f)] private float _range = 6f;

        [Tooltip("Proyectiles en cada disparo. Con más de uno salen en abanico.")]
        [SerializeField, Range(1, 8)] private int _projectileCount = 1;

        [Tooltip("Sonido de cada disparo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _shotSound;

        [Header("Referencias")]
        [SerializeField] private Projectile _projectilePrefab;

        private const float SpreadAngle = 20f; // grados entre proyectiles del abanico

        private Transform _player;
        private float _timer;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _player = player.transform;
            _timer = _fireInterval / 2f;
        }

        private void Update()
        {
            if (_player == null) return;

            Vector2 toPlayer = _player.position - transform.position;
            if (toPlayer.magnitude > _range) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = _fireInterval;

            float aim = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = aim + (i - (_projectileCount - 1) / 2f) * SpreadAngle;
                Instantiate(_projectilePrefab, transform.position, Quaternion.Euler(0f, 0f, angle));
            }
            Sound.Play(_shotSound);
        }

        private void OnValidate()
        {
            if (_projectilePrefab == null) Debug.LogWarning($"[{name}] {nameof(EnemyShooter)}: falta el proyectil.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}
