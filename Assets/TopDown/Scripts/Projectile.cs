using UnityEngine;

namespace TopDown
{
    // Proyectil de un enemigo: vuela recto, quita corazones al jugador y se rompe contra árboles, rocas y cajas.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Velocidad, en tiles por segundo. El jugador camina a 5.")]
        [SerializeField, Range(1f, 20f)] private float _speed = 6f;

        [Tooltip("Corazones que quita al dar al jugador.")]
        [SerializeField, Range(1, 10)] private int _damage = 1;

        [Tooltip("Segundos que vuela antes de desaparecer solo.")]
        [SerializeField, Range(0.5f, 10f)] private float _lifetime = 4f;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;

        private void Start()
        {
            // Vuela hacia su derecha: quien lo dispara lo gira hacia donde apunta.
            _body.linearVelocity = transform.right * _speed;
            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
            else if (other.isTrigger) return; // atraviesa monedas, banderas y otros objetos que no son sólidos

            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(Projectile)}: falta el Rigidbody2D.", this);
        }
    }
}
