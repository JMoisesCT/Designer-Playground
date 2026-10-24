using System.Collections;
using UnityEngine;

namespace Platformer
{
    // Meta del nivel: al tocarla, el jugador se detiene, sale un mensaje y se carga el siguiente nivel.
    public class Goal : MonoBehaviour
    {
        private static readonly int PressedState = Animator.StringToHash("Pressed");

        [Header("Meta")]
        [Tooltip("Nivel que se carga al llegar. Arrastra aquí una escena. Si lo dejas vacío, se repite este nivel.")]
        [SerializeField, ScenePath] private string _nextLevel;

        [Tooltip("Mensaje que aparece en pantalla al llegar.")]
        [SerializeField] private string _message = "¡Nivel completado!";

        [Tooltip("Segundos que se ve el mensaje antes de cambiar de nivel.")]
        [SerializeField, Range(0f, 5f)] private float _delay = 2f;

        [Tooltip("Música al llegar. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [SerializeField] private Animator _animator;

        private bool _reached;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_reached) return;
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player == null || player.IsDead) return;

            _reached = true;
            player.GetComponent<PlayerController>().Stop();
            if (_animator != null) _animator.Play(PressedState);
            Sound.Play(_sound);

            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.ShowMessage(_message);

            StartCoroutine(LoadNextLevel());
        }

        private IEnumerator LoadNextLevel()
        {
            yield return new WaitForSeconds(_delay);
            Checkpoint.Clear(); // el siguiente nivel (o este, si se repite) empieza desde el principio
            if (string.IsNullOrEmpty(_nextLevel)) LevelLoader.Restart();
            else LevelLoader.Load(_nextLevel);
        }

        private void OnValidate()
        {
            if (_animator == null) Debug.LogWarning($"[{name}] {nameof(Goal)}: falta el Animator.", this);
        }
    }
}
