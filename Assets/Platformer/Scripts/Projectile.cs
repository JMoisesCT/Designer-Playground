using UnityEngine;

namespace Platformer
{
    // Proyectil de un enemigo: vuela recto, mata al jugador al tocarlo y se rompe al chocar con el terreno.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Velocidad, en tiles por segundo.")]
        [SerializeField, Range(1f, 30f)] private float _speed = 8f;

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
            if (player != null) player.Die();
            else if (other.isTrigger) return; // atraviesa frutas, banderas y otros objetos que no son sólidos

            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(Projectile)}: falta el Rigidbody2D.", this);
        }
    }
}
