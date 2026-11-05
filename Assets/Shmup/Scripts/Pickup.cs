using UnityEngine;

namespace Shmup
{
    // Objeto que la nave recoge al tocarlo: mejora el arma o recupera una vida.
    public class Pickup : MonoBehaviour
    {
        public enum PickupKind { Weapon, Health }

        [Tooltip("Weapon: sube un nivel el arma. Health: recupera una vida.")]
        [SerializeField] private PickupKind _kind;

        [Tooltip("Sonido al recogerlo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player == null || player.IsDead) return;

            if (_kind == PickupKind.Weapon) player.GetComponent<PlayerController>().UpgradeWeapon();
            else player.Heal(1);

            Sound.Play(_sound);
            Destroy(gameObject);
        }
    }
}
