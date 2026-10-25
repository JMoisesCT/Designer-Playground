using System.Collections;
using UnityEngine;

namespace TopDown
{
    // Cofre del final: al tocarlo se abre, el jugador se detiene, sale un mensaje y se carga el siguiente nivel.
    public class Goal : MonoBehaviour
    {
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
        [SerializeField] private SpriteRenderer _sprite;
        [Tooltip("Dibujo del cofre abierto.")]
        [SerializeField] private Sprite _openSprite;

        private bool _reached;

        // El cofre es sólido: se abre al chocar con él.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_reached) return;
            PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();
            if (player == null || player.IsDead) return;

            _reached = true;
            player.GetComponent<PlayerController>().Stop();
            _sprite.sprite = _openSprite;
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
            if (_sprite == null || _openSprite == null) Debug.LogWarning($"[{name}] {nameof(Goal)}: faltan referencias.", this);
        }
    }
}
