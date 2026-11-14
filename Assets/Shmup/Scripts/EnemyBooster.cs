using UnityEngine;

namespace Shmup
{
    // Nave de apoyo: mientras vive, los enemigos dentro de su círculo morado disparan y vuelan más rápido (y se ven morados).
    [RequireComponent(typeof(Enemy))]
    public class EnemyBooster : MonoBehaviour
    {
        [Tooltip("Radio de la zona que potencia, en tiles (círculo morado en la escena).")]
        [SerializeField, Range(1f, 15f)] private float _radius = 6f;

        [Tooltip("Cuánto más rápido disparan los enemigos potenciados: 2 = el doble.")]
        [SerializeField, Range(1f, 4f)] private float _fireRate = 2f;

        [Tooltip("Cuánto más rápido vuelan los enemigos potenciados: 1,5 = un 50 % más.")]
        [SerializeField, Range(1f, 3f)] private float _speed = 1.5f;

        private Enemy _enemy;

        private void Start()
        {
            _enemy = GetComponent<Enemy>();
        }

        // Cada frame potencia a los que están cerca: al destruirlo, el efecto se acaba enseguida.
        private void Update()
        {
            if (!_enemy.IsActive) return;
            foreach (Enemy other in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (other != _enemy && other.IsActive && Vector2.Distance(other.transform.position, transform.position) <= _radius)
                    other.Boost(_fireRate, _speed);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.3f, 1f);
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
