using System.Collections;
using UnityEngine;

namespace Shmup
{
    // Vida de la nave. Cada golpe quita una vida y da unos instantes de invulnerabilidad (la nave parpadea).
    // Sin vidas, explota y el nivel se reinicia.
    public class PlayerHealth : MonoBehaviour
    {
        private const float BlinkInterval = 0.1f;

        [Header("Vida")]
        [Tooltip("Vidas con las que empieza la nave.")]
        [SerializeField, Range(1, 10)] private int _maxHealth = 3;

        [Tooltip("Segundos sin recibir daño después de un golpe.")]
        [SerializeField, Range(0f, 3f)] private float _invulnerableTime = 1.5f;

        [Tooltip("Segundos desde que pierde la última vida hasta que se reinicia el nivel.")]
        [SerializeField, Range(0f, 3f)] private float _restartDelay = 2f;

        [Header("Sonido")]
        [Tooltip("Sonido al recibir un golpe. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _hurtSound;

        [Tooltip("Sonido al perder la última vida. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _deathSound;

        [Header("Referencias")]
        [SerializeField] private PlayerController _controller;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private GameObject _explosion;

        private float _invulnerableUntil;

        public int Health { get; private set; }
        public bool IsDead => Health <= 0;

        private void Awake()
        {
            Health = _maxHealth;
        }

        private void Start()
        {
            RefreshHud();
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || Time.time < _invulnerableUntil) return;

            Health = Mathf.Max(Health - amount, 0);
            RefreshHud();
            if (IsDead)
            {
                Die();
                return;
            }

            _invulnerableUntil = Time.time + _invulnerableTime;
            Sound.Play(_hurtSound);
            StartCoroutine(Blink());
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(Health + amount, _maxHealth);
            RefreshHud();
        }

        // Ya nada le hace daño (por ejemplo, al completar el nivel).
        public void Protect()
        {
            _invulnerableUntil = float.MaxValue;
        }

        private void Die()
        {
            _controller.enabled = false;
            _collider.enabled = false;
            _sprite.enabled = false;
            Instantiate(_explosion, transform.position, Quaternion.identity);
            Sound.Play(_deathSound);
            StartCoroutine(RestartLevel());
        }

        private IEnumerator Blink()
        {
            while (!IsDead && Time.time < _invulnerableUntil)
            {
                _sprite.enabled = !_sprite.enabled;
                yield return new WaitForSeconds(BlinkInterval);
            }
            _sprite.enabled = !IsDead;
        }

        private IEnumerator RestartLevel()
        {
            yield return new WaitForSeconds(_restartDelay);
            LevelLoader.Restart();
        }

        private void RefreshHud()
        {
            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.ShowHealth(Health, _maxHealth);
        }

        private void OnValidate()
        {
            if (_controller == null || _collider == null || _sprite == null || _explosion == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerHealth)}: faltan referencias.", this);
        }
    }
}
