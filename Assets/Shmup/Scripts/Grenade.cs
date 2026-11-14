using UnityEngine;

namespace Shmup
{
    // Granada: vuela como una bala (Bullet) y, al cabo de un rato, estalla en fragmentos que salen en círculo.
    [RequireComponent(typeof(Bullet))]
    public class Grenade : MonoBehaviour
    {
        private const float GizmoLength = 1.5f;

        [Tooltip("Segundos que vuela hasta estallar.")]
        [SerializeField, Range(0.2f, 5f)] private float _fuseTime = 1.2f;

        [Tooltip("Fragmentos que salen al estallar, repartidos en círculo (líneas rojas en la escena).")]
        [SerializeField, Range(2, 24)] private int _fragmentCount = 8;

        [Tooltip("Sonido al estallar. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [Tooltip("Bala que sale en cada fragmento.")]
        [SerializeField] private Bullet _fragmentPrefab;
        [SerializeField] private GameObject _explosion;

        private void Start()
        {
            Invoke(nameof(Explode), _fuseTime);
        }

        private void Explode()
        {
            for (int i = 0; i < _fragmentCount; i++)
            {
                float angle = 360f * i / _fragmentCount;
                Instantiate(_fragmentPrefab, transform.position, Quaternion.Euler(0f, 0f, angle));
            }
            Instantiate(_explosion, transform.position, Quaternion.identity);
            Sound.Play(_sound);
            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (_fragmentPrefab == null || _explosion == null)
                Debug.LogWarning($"[{name}] {nameof(Grenade)}: faltan referencias.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _fragmentCount; i++)
            {
                Vector3 direction = Quaternion.Euler(0f, 0f, 360f * i / _fragmentCount) * Vector3.up;
                Gizmos.DrawLine(transform.position, transform.position + direction * GizmoLength);
            }
        }
    }
}
