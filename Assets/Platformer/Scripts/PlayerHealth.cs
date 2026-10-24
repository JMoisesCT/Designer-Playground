using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer
{
    // Muerte del jugador: al tocar un peligro (pinchos, enemigos) o caer al vacío se reinicia el nivel.
    public class PlayerHealth : MonoBehaviour
    {
        private static readonly int HitState = Animator.StringToHash("Hit");

        [Header("Muerte")]
        [Tooltip("Si el jugador cae por debajo de esta altura (Y), muere. Se ve como una línea roja en la escena.")]
        [SerializeField] private float _deathHeight = -10f;

        [Tooltip("Segundos desde que muere hasta que se reinicia el nivel.")]
        [SerializeField, Range(0f, 3f)] private float _restartDelay = 1f;

        [Tooltip("Pequeño salto que da el personaje al morir.")]
        [SerializeField, Range(0f, 20f)] private float _deathJump = 10f;

        [Header("Sonido")]
        [Tooltip("Sonido al morir. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _deathSound;

        [Header("Referencias")]
        [SerializeField] private PlayerController _controller;
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;

        private readonly List<ContactPoint2D> _contacts = new();

        public bool IsDead { get; private set; }

        private void Awake()
        {
            // Si ya tocó una bandera de control, empieza en ella.
            if (Checkpoint.TryGetRespawn(out Vector2 respawn)) transform.position = respawn;
        }

        private void Update()
        {
            if (!IsDead && transform.position.y < _deathHeight) Die();
        }

        // Los tiles de peligro (HazardTile) forman parte del Tilemap: se mira qué tile hay en cada punto de contacto.
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (IsDead) return;
            Tilemap tilemap = collision.collider.GetComponent<Tilemap>();
            if (tilemap == null) return;

            collision.GetContacts(_contacts);
            foreach (ContactPoint2D contact in _contacts)
            {
                Vector2 offset = contact.normal * 0.05f;
                if (IsHazardAt(tilemap, contact.point + offset) || IsHazardAt(tilemap, contact.point - offset))
                {
                    Die();
                    return;
                }
            }
        }

        public void Die()
        {
            if (IsDead) return;
            IsDead = true;

            _controller.enabled = false;
            _collider.enabled = false;
            _body.linearVelocity = new Vector2(0f, _deathJump);
            _animator.Play(HitState);
            Sound.Play(_deathSound);
            StartCoroutine(RestartLevel());
        }

        private IEnumerator RestartLevel()
        {
            yield return new WaitForSeconds(_restartDelay);
            LevelLoader.Restart();
        }

        private static bool IsHazardAt(Tilemap tilemap, Vector2 point)
        {
            return tilemap.GetTile(tilemap.WorldToCell(point)) is HazardTile;
        }

        private void OnValidate()
        {
            if (_controller == null || _body == null || _collider == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerHealth)}: faltan referencias.", this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3(transform.position.x, _deathHeight, 0f);
            Gizmos.DrawLine(center + Vector3.left * 200f, center + Vector3.right * 200f);
        }
    }
}
