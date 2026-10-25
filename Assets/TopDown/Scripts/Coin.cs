using UnityEngine;

namespace TopDown
{
    // Moneda que el jugador recoge al tocarla. Suma uno al contador de la pantalla.
    public class Coin : MonoBehaviour
    {
        [Tooltip("Sonido al recogerla. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null) return;

            Sound.Play(_sound);
            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.AddCoin();
            Destroy(gameObject);
        }
    }
}
