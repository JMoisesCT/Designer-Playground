using UnityEngine;

namespace Shmup
{
    // Borra el objeto al cabo de un tiempo (por ejemplo, una explosión al terminar su animación).
    public class AutoDestroy : MonoBehaviour
    {
        [Tooltip("Segundos que dura el objeto.")]
        [SerializeField, Range(0.1f, 5f)] private float _lifetime = 0.5f;

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}
