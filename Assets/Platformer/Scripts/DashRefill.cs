using UnityEngine;

namespace Platformer
{
    // Objeto que recarga el dash del jugador al tocarlo. Puede volver a aparecer al cabo de un rato.
    public class DashRefill : MonoBehaviour
    {
        [Tooltip("Segundos hasta que vuelve a aparecer. 0 = no vuelve.")]
        [SerializeField, Range(0f, 10f)] private float _respawnTime = 3f;

        [Tooltip("Sonido al recogerlo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sprite;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Si el jugador ya tiene todas las cargas, no se gasta.
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null || player.DashCharges >= player.MaxDashCharges) return;

            player.RefillDash();
            Sound.Play(_sound);
            if (_respawnTime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            SetVisible(false);
            Invoke(nameof(Respawn), _respawnTime);
        }

        private void Respawn()
        {
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            _collider.enabled = visible;
            _sprite.enabled = visible;
        }

        private void OnValidate()
        {
            if (_collider == null || _sprite == null)
                Debug.LogWarning($"[{name}] {nameof(DashRefill)}: faltan referencias.", this);
        }
    }
}
