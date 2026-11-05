using UnityEngine;

namespace Shmup
{
    // Bala que vuela recto hacia donde apunta. Desaparece al salir de la pantalla o al dar en un blanco.
    public class Bullet : MonoBehaviour
    {
        private const float ScreenMargin = 1f;

        [Tooltip("Velocidad de la bala, en tiles por segundo.")]
        [SerializeField, Range(1f, 40f)] private float _speed = 18f;

        [Tooltip("Vida que quita al dar en el blanco.")]
        [SerializeField, Range(1, 10)] private int _damage = 1;

        [Tooltip("Activado: daña a los enemigos. Desactivado: daña al jugador.")]
        [SerializeField] private bool _fromPlayer = true;

        private void Update()
        {
            transform.position += transform.up * (_speed * Time.deltaTime);
            if (!CameraView.Contains(transform.position, ScreenMargin)) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_fromPlayer)
            {
                Enemy enemy = other.GetComponentInParent<Enemy>();
                if (enemy == null || !enemy.IsActive) return;
                enemy.TakeDamage(_damage);
            }
            else
            {
                PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
                if (player == null) return;
                player.TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
    }
}
