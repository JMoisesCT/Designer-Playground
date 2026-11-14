using System.Collections;
using UnityEngine;

namespace Platformer
{
    // Plataforma que se apaga y cae poco después de que el jugador se suba. Puede volver a aparecer.
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingPlatform : MonoBehaviour
    {
        private const float FallAcceleration = 40f;
        private const float MaxFallSpeed = 20f;
        private const float FallDuration = 1.5f;

        private static readonly int OnState = Animator.StringToHash("On");
        private static readonly int OffState = Animator.StringToHash("Off");

        [Tooltip("Segundos desde que el jugador la pisa hasta que cae.")]
        [SerializeField, Range(0f, 3f)] private float _fallDelay = 0.5f;

        [Tooltip("Segundos hasta que vuelve a su sitio. 0 = no vuelve.")]
        [SerializeField, Range(0f, 10f)] private float _respawnTime = 3f;

        [Tooltip("Sonido al pisarla. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Animator _animator;

        private Vector2 _startPosition;
        private bool _triggered;

        private void Awake()
        {
            _startPosition = _body.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_triggered || collision.collider.GetComponentInParent<PlayerController>() == null) return;
            // Solo cuenta si el jugador está encima.
            if (collision.collider.bounds.min.y < _collider.bounds.max.y - 0.1f) return;

            StartCoroutine(Fall());
        }

        private IEnumerator Fall()
        {
            _triggered = true;
            _animator.Play(OffState); // se paran las hélices: aviso de que va a caer
            Sound.Play(_sound);
            yield return new WaitForSeconds(_fallDelay);

            // Cae acelerando. Se mueve con velocidad para que el jugador que está encima caiga con ella.
            for (float time = 0f; time < FallDuration; time += Time.fixedDeltaTime)
            {
                float speed = Mathf.Min(-_body.linearVelocity.y + FallAcceleration * Time.fixedDeltaTime, MaxFallSpeed);
                _body.linearVelocity = new Vector2(0f, -speed);
                yield return new WaitForFixedUpdate();
            }

            _body.linearVelocity = Vector2.zero;
            if (_respawnTime <= 0f)
            {
                Destroy(gameObject);
                yield break;
            }

            _sprite.enabled = false;
            _collider.enabled = false;
            yield return new WaitForSeconds(_respawnTime);

            _body.position = _startPosition;
            transform.position = _startPosition;
            _sprite.enabled = true;
            _collider.enabled = true;
            _animator.Play(OnState);
            _triggered = false;
        }

        private void OnValidate()
        {
            if (_body == null || _collider == null || _sprite == null || _animator == null)
                Debug.LogWarning($"[{name}] {nameof(FallingPlatform)}: faltan referencias.", this);
        }
    }
}
