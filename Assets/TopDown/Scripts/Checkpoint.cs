using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDown
{
    // Bandera de control: después de tocarla, si el jugador pierde todos los corazones reaparece aquí.
    public class Checkpoint : MonoBehaviour
    {
        private static readonly int OnState = Animator.StringToHash("On");

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
            position = _savedPosition;
            return _savedScene == SceneManager.GetActiveScene().path;
        }

        private void Start()
        {
            // La bandera en la que reaparece el jugador ya está activada.
            if (_savedScene == gameObject.scene.path && _savedPosition == (Vector2)transform.position)
            {
                _reached = true;
                _animator.Play(OnState);
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
            _animator.Play(OnState);
            Sound.Play(_sound);
        }

        private void OnValidate()
        {
            if (_animator == null) Debug.LogWarning($"[{name}] {nameof(Checkpoint)}: falta el Animator.", this);
        }
    }
}
