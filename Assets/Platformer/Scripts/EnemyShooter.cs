using UnityEngine;

namespace Platformer
{
    // Enemigo que mira al jugador y le dispara cada cierto tiempo cuando está cerca.
    public class EnemyShooter : MonoBehaviour
    {
        [Tooltip("Segundos entre disparos.")]
        [SerializeField, Range(0.3f, 6f)] private float _fireInterval = 2f;

        [Tooltip("Solo dispara si el jugador está a menos de esta distancia, en tiles (círculo naranja).")]
        [SerializeField, Range(1f, 30f)] private float _range = 10f;

        [Tooltip("Activado: apunta directo al jugador (también en diagonal). Desactivado: dispara recto hacia los lados.")]
        [SerializeField] private bool _aimAtPlayer;

        [Tooltip("Sonido de cada disparo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _shotSound;

        [Header("Referencias")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private SpriteRenderer _sprite;

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

            // Los personajes de Pixel Adventure miran a la derecha: flipX = mira a la izquierda.
            _sprite.flipX = toPlayer.x < 0f;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = _fireInterval;

            Vector2 direction = _aimAtPlayer ? toPlayer.normalized : new Vector2(Mathf.Sign(toPlayer.x), 0f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Instantiate(_projectilePrefab, transform.position, Quaternion.Euler(0f, 0f, angle));
            Sound.Play(_shotSound);
        }

        private void OnValidate()
        {
            if (_projectilePrefab == null || _sprite == null)
                Debug.LogWarning($"[{name}] {nameof(EnemyShooter)}: falta el proyectil o el SpriteRenderer.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}
