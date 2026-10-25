using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TopDown
{
    // Vida del jugador en corazones. Cada golpe quita uno y da unos instantes de invulnerabilidad (el personaje parpadea).
    // Sin corazones, el nivel se reinicia desde la última bandera.
    public class PlayerHealth : MonoBehaviour
    {
        private const float BlinkInterval = 0.1f;
        private static readonly int DeadState = Animator.StringToHash("Dead");

        [Header("Vida")]
        [Tooltip("Corazones con los que empieza el jugador.")]
        [SerializeField, Range(1, 10)] private int _maxHealth = 3;

        [Tooltip("Segundos sin recibir daño después de un golpe.")]
        [SerializeField, Range(0f, 3f)] private float _invulnerableTime = 1f;

        [Tooltip("Segundos desde que pierde el último corazón hasta que se reinicia el nivel.")]
        [SerializeField, Range(0f, 3f)] private float _restartDelay = 1.5f;

        [Header("Sonido")]
        [Tooltip("Sonido al recibir un golpe. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _hurtSound;

        [Tooltip("Sonido al perder el último corazón. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _deathSound;

        [Header("Referencias")]
        [SerializeField] private PlayerController _controller;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Animator _animator;

        private Tilemap[] _tilemaps;
        private float _invulnerableUntil;

        public int Health { get; private set; }
        public int MaxHealth => _maxHealth;
        public bool IsDead => Health <= 0;

        private void Awake()
        {
            Health = _maxHealth;
            // Si ya tocó una bandera, empieza en ella.
            if (Checkpoint.TryGetRespawn(out Vector2 respawn)) transform.position = respawn;
        }

        private void Start()
        {
            _tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            RefreshHud();
        }

        // Los pinchos (HazardTile) se pintan en el suelo: se mira qué tile hay bajo los pies.
        private void FixedUpdate()
        {
            Vector2 feet = _collider.bounds.center;
            foreach (Tilemap tilemap in _tilemaps)
            {
                if (tilemap.GetTile(tilemap.WorldToCell(feet)) is HazardTile)
                {
                    TakeDamage(1);
                    return;
                }
            }
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

        private void Die()
        {
            _controller.enabled = false;
            _collider.enabled = false;
            _sprite.enabled = true;
            _animator.Play(DeadState);
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
            _sprite.enabled = true;
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
            if (_controller == null || _collider == null || _sprite == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerHealth)}: faltan referencias.", this);
        }
    }
}
