using System.Collections;
using UnityEngine;

namespace Shmup
{
    // Enemigo: espera quieto hasta entrar en pantalla, tiene vida, da puntos y duele al tocarlo.
    // Se mueve con EnemyMover y dispara con EnemyShooter.
    public class Enemy : MonoBehaviour
    {
        private const float WakeMargin = 2f; // se despierta justo antes de asomar por el borde
        private const float ExitMargin = 3f; // desaparece al alejarse de la pantalla
        private const float FlashTime = 0.06f;

        [Header("Vida")]
        [Tooltip("Balazos que aguanta.")]
        [SerializeField, Range(1, 200)] private int _health = 1;

        [Tooltip("Puntos que da al destruirlo.")]
        [SerializeField, Min(0)] private int _points = 100;

        [Tooltip("Objeto que suelta al destruirlo, por ejemplo una Mejora. Déjalo vacío para que no suelte nada.")]
        [SerializeField] private GameObject _drop;

        [Header("Sonido")]
        [Tooltip("Sonido al recibir un balazo sin morir. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _hitSound;

        [Tooltip("Sonido al explotar. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _deathSound;

        [Header("Referencias")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private GameObject _explosion;

        private const float BoostLinger = 0.2f;   // segundos que dura el potenciador tras alejarse
        private static readonly Color BoostColor = new(1f, 0.55f, 1f);

        private Color _baseColor;
        private bool _flashing;
        private float _boostedUntil = -1f;
        private float _boostFireRate = 1f;
        private float _boostSpeed = 1f;

        public bool IsActive { get; private set; }
        public bool IsBoosted => Time.time < _boostedUntil;
        public float FireRateMultiplier => IsBoosted ? _boostFireRate : 1f;
        public float SpeedMultiplier => IsBoosted ? _boostSpeed : 1f;

        private void Awake()
        {
            _baseColor = _sprite.color;
        }

        // Lo llama un Potenciador cercano en cada frame.
        public void Boost(float fireRate, float speed)
        {
            _boostedUntil = Time.time + BoostLinger;
            _boostFireRate = fireRate;
            _boostSpeed = speed;
        }

        private void Update()
        {
            if (!_flashing) _sprite.color = IsBoosted ? BoostColor : _baseColor;

            if (!IsActive)
            {
                IsActive = CameraView.Contains(transform.position, WakeMargin);
                return;
            }
            if (!CameraView.Contains(transform.position, ExitMargin)) Destroy(gameObject);
        }

        public void TakeDamage(int amount)
        {
            _health -= amount;
            if (_health <= 0)
            {
                Die();
                return;
            }
            Sound.Play(_hitSound);
            StartCoroutine(Flash());
        }

        private void Die()
        {
            Sound.Play(_deathSound);
            GameObject explosion = Instantiate(_explosion, transform.position, Quaternion.identity);
            explosion.transform.localScale = transform.localScale;
            if (_drop != null) Instantiate(_drop, transform.position, Quaternion.identity);

            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.AddScore(_points);
            Destroy(gameObject);
        }

        private IEnumerator Flash()
        {
            _flashing = true;
            _sprite.color = Color.red;
            yield return new WaitForSeconds(FlashTime);
            _flashing = false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!IsActive) return;
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(1);
        }

        private void OnValidate()
        {
            if (_sprite == null || _explosion == null)
                Debug.LogWarning($"[{name}] {nameof(Enemy)}: faltan referencias.", this);
        }
    }
}
