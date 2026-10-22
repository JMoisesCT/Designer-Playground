using UnityEngine;

namespace Platformer
{
    // Fruta que el jugador recoge al tocarla. Suma uno al contador de la pantalla.
    public class Fruit : MonoBehaviour
    {
        // Mismo orden que los estados del Animator y que la lista _sprites.
        public enum Kind
        {
            [InspectorName("Manzana")] Apple,
            [InspectorName("Plátano")] Bananas,
            [InspectorName("Cerezas")] Cherries,
            [InspectorName("Kiwi")] Kiwi,
            [InspectorName("Melón")] Melon,
            [InspectorName("Naranja")] Orange,
            [InspectorName("Piña")] Pineapple,
            [InspectorName("Fresa")] Strawberry,
        }

        private static readonly int CollectedState = Animator.StringToHash("Collected");
        private const float CollectedTime = 0.3f; // lo que dura la animación "Collected"

        [Tooltip("Qué fruta es. Solo cambia el dibujo.")]
        [SerializeField] private Kind _kind;

        [Header("Referencias")]
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;
        [Tooltip("Primer dibujo de cada fruta, para verla en la escena sin darle a Play.")]
        [SerializeField] private Sprite[] _sprites;

        private void Start()
        {
            _animator.Play(_kind.ToString());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null) return;

            _collider.enabled = false;
            _animator.Play(CollectedState);
            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.AddFruit();
            Destroy(gameObject, CollectedTime);
        }

        private void OnValidate()
        {
            if (_collider == null || _animator == null || _sprite == null)
                Debug.LogWarning($"[{name}] {nameof(Fruit)}: faltan referencias.", this);
            else if (_sprites != null && (int)_kind < _sprites.Length)
                _sprite.sprite = _sprites[(int)_kind];
        }
    }
}
