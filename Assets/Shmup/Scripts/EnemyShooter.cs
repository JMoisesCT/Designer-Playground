using UnityEngine;

namespace Shmup
{
    // Hace que un enemigo dispare cada cierto tiempo mientras está en pantalla.
    [RequireComponent(typeof(Enemy))]
    public class EnemyShooter : MonoBehaviour
    {
        private const float ShotVolume = 0.3f;

        [Header("Disparo")]
        [Tooltip("Segundos entre disparos.")]
        [SerializeField, Range(0.2f, 6f)] private float _fireInterval = 1.5f;

        [Tooltip("Balas que salen en cada disparo.")]
        [SerializeField, Range(1, 9)] private int _bulletCount = 1;

        [Tooltip("Ángulo del abanico cuando sale más de una bala, en grados.")]
        [SerializeField, Range(0f, 360f)] private float _spreadAngle = 40f;

        [Tooltip("Activado: apunta al jugador. Desactivado: dispara hacia donde mira el enemigo.")]
        [SerializeField] private bool _aimAtPlayer = true;

        [Tooltip("Sonido de cada disparo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _shotSound;

        [Header("Referencias")]
        [SerializeField] private Bullet _bulletPrefab;

        private Enemy _enemy;
        private Transform _player;
        private float _timer;

        private void Start()
        {
            _enemy = GetComponent<Enemy>();
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _player = player.transform;
            _timer = _fireInterval / 2f;
        }

        private void Update()
        {
            if (!_enemy.IsActive || !CameraView.Contains(transform.position, 0f)) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = _fireInterval;
            Fire();
        }

        private void Fire()
        {
            Sound.Play(_shotSound, ShotVolume);

            Quaternion aim = transform.rotation;
            if (_aimAtPlayer && _player != null)
                aim = Quaternion.LookRotation(Vector3.forward, _player.position - transform.position);

            for (int i = 0; i < _bulletCount; i++)
            {
                float angle = _bulletCount == 1 ? 0f : Mathf.Lerp(-_spreadAngle, _spreadAngle, i / (_bulletCount - 1f)) / 2f;
                Instantiate(_bulletPrefab, transform.position, aim * Quaternion.Euler(0f, 0f, angle));
            }
        }

        private void OnValidate()
        {
            if (_bulletPrefab == null) Debug.LogWarning($"[{name}] {nameof(EnemyShooter)}: falta la bala.", this);
        }
    }
}
