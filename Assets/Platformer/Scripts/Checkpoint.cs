using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    // Bandera de control: después de tocarla, si el jugador muere reaparece aquí en vez de al principio del nivel.
    public class Checkpoint : MonoBehaviour
    {
        private static readonly int FlagOutState = Animator.StringToHash("FlagOut");
        private static readonly int FlagIdleState = Animator.StringToHash("FlagIdle");

        // Los pies de la bandera están 2 tiles por debajo de su centro; el centro del jugador, 1 tile por encima de sus pies.
        private static readonly Vector2 SpawnOffset = new(0f, -1f);

        // Última bandera tocada. Es static para que no se pierda al reiniciar el nivel.
        private static string _savedScene;
        private static Vector2 _savedPosition;

        [Tooltip("Sonido al tocar la bandera. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [SerializeField] private Animator _animator;

        private bool _reached;

        // Olvida la bandera guardada. También se llama al darle a Play: sin "Domain Reload" los static sobreviven entre partidas.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            _savedScene = null;
        }

        // Dónde debe aparecer el jugador si ya tocó una bandera de este nivel.
        public static bool TryGetRespawn(out Vector2 position)
        {
            position = _savedPosition + SpawnOffset;
            return _savedScene == SceneManager.GetActiveScene().path;
        }

        private void Start()
        {
            // La bandera en la que reaparece el jugador ya está levantada.
            if (_savedScene == gameObject.scene.path && _savedPosition == (Vector2)transform.position)
            {
                _reached = true;
                _animator.Play(FlagIdleState);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_reached) return;
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player == null || player.IsDead) return;

            _reached = true;
            _savedScene = gameObject.scene.path;
            _savedPosition = transform.position;
            _animator.Play(FlagOutState);
            Sound.Play(_sound);
        }

        private void OnValidate()
        {
            if (_animator == null) Debug.LogWarning($"[{name}] {nameof(Checkpoint)}: falta el Animator.", this);
        }
    }
}
