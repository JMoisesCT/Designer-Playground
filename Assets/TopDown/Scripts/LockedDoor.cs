using UnityEngine;

namespace TopDown
{
    // Puerta cerrada: es sólida hasta que el jugador llega con una llave. La llave se gasta y la puerta desaparece.
    public class LockedDoor : MonoBehaviour
    {
        [Tooltip("Sonido al abrirse. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _openSound;

        [Tooltip("Sonido al chocar sin llave. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _lockedSound;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            PlayerItems items = collision.collider.GetComponentInParent<PlayerItems>();
            if (items == null) return;

            if (items.Keys == 0)
            {
                Sound.Play(_lockedSound);
                return;
            }

            items.Keys--;
            Sound.Play(_openSound);
            Destroy(gameObject);
        }
    }
}
