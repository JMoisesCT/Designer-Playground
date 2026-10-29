using UnityEngine;

namespace TopDown
{
    // Enemigo que vigila una zona (círculo rojo): si el jugador entra, lo persigue; si sale, vuelve a su sitio.
    // Choca con árboles y rocas, así que el jugador puede escapar rodeándolos.
    [RequireComponent(typeof(Rigidbody2D))]
    public class ChaserEnemy : MonoBehaviour
    {
        [Header("Persecución")]
        [Tooltip("Radio de la zona que vigila, en tiles (círculo rojo en la escena). Persigue al jugador mientras esté dentro.")]
        [SerializeField, Range(1f, 15f)] private float _sightRadius = 4f;

        [Tooltip("Velocidad, en tiles por segundo. El jugador camina a 5.")]
        [SerializeField, Range(0.5f, 10f)] private float _speed = 3f;

        [Tooltip("Sonido al ver al jugador. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _alertSound;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;

        private Vector2 _home;
        private PlayerHealth _player;
        private bool _chasing;

        private Vector2 Home => Application.isPlaying ? _home : (Vector2)transform.position;

        private void Awake()
        {
            _home = _body.position;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerHealth>();
        }

        private void FixedUpdate()
        {
            bool seesPlayer = _player != null && !_player.IsDead &&
                              Vector2.Distance(_home, _player.transform.position) <= _sightRadius;
            if (seesPlayer && !_chasing) Sound.Play(_alertSound);
            _chasing = seesPlayer;

            Vector2 target = _chasing ? (Vector2)_player.transform.position : _home;
            Vector2 next = Vector2.MoveTowards(_body.position, target, _speed * Time.fixedDeltaTime);
            _body.linearVelocity = (next - _body.position) / Time.fixedDeltaTime;
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(ChaserEnemy)}: falta el Rigidbody2D.", this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Home, _sightRadius);
        }
    }
}
