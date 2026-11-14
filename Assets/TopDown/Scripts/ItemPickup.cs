using UnityEngine;

namespace TopDown
{
    // Objeto que el jugador recoge al tocarlo: una llave (abre una puerta) o el pulso EMP (se usa con E).
    public class ItemPickup : MonoBehaviour
    {
        public enum Kind
        {
            [InspectorName("Llave")] Key,
            [InspectorName("Pulso EMP")] Emp,
        }

        [Tooltip("Qué objeto es.")]
        [SerializeField] private Kind _kind;

        [Tooltip("Sonido al recogerlo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerItems items = other.GetComponentInParent<PlayerItems>();
            if (items == null) return;
            // Solo se puede llevar un pulso EMP a la vez.
            if (_kind == Kind.Emp && items.HasEmp) return;

            if (_kind == Kind.Key) items.Keys++;
            else items.HasEmp = true;
            Sound.Play(_sound);
            Destroy(gameObject);
        }
    }
}
