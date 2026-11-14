using System.Collections;
using UnityEngine;

namespace Platformer
{
    // Estalactita de hielo: cuelga del techo, tiembla cuando el jugador pasa por debajo y cae. Mata al tocarla (Hazard).
    [RequireComponent(typeof(Rigidbody2D))]
    public class Icicle : MonoBehaviour
    {
        private const float FallGravity = 4f;
        private const float MaxZoneHeight = 20f;
        private const float ShakeOffset = 1f / 16f; // un píxel

        [Tooltip("Ancho de la zona que la hace caer, en tiles (rectángulo rojo de debajo).")]
        [SerializeField, Range(0.5f, 8f)] private float _triggerWidth = 2f;

        [Tooltip("Segundos que tiembla antes de caer: el tiempo que tiene el jugador para apartarse.")]
        [SerializeField, Range(0f, 2f)] private float _warningTime = 0.4f;

        [Header("Sonido")]
        [Tooltip("Sonido al empezar a caer. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _fallSound;

        [Tooltip("Sonido al romperse contra el suelo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _breakSound;

        [Header("Referencias")]
        [SerializeField] private Rigidbody2D _body;

        [Tooltip("Capas que la detienen (el suelo).")]
        [SerializeField] private LayerMask _groundLayers;

        private bool _triggered;
        private bool _falling;

        private void FixedUpdate()
        {
            if (_triggered) return;

            Rect zone = GetTriggerZone();
            if (Physics2D.OverlapBox(zone.center, zone.size, 0f, LayerMask.GetMask("Player")) != null)
                StartCoroutine(Fall());
        }

        private IEnumerator Fall()
        {
            _triggered = true;
            Vector2 start = _body.position;
            for (float time = 0f; time < _warningTime; time += Time.fixedDeltaTime)
            {
                // Tiembla un píxel a cada lado.
                _body.MovePosition(start + Vector2.right * (Mathf.Repeat(time * 20f, 2f) < 1f ? ShakeOffset : -ShakeOffset));
                yield return new WaitForFixedUpdate();
            }
            _body.position = start;

            Sound.Play(_fallSound);
            _falling = true;
            _body.gravityScale = FallGravity;
            Destroy(gameObject, 5f); // por si cae a un foso sin fondo
        }

        // Se rompe al tocar el suelo (el techo del que cuelga no cuenta: aún no estaba cayendo).
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_falling || (_groundLayers.value & (1 << other.gameObject.layer)) == 0) return;
            Sound.Play(_breakSound);
            Destroy(gameObject);
        }

        // Zona bajo la estalactita: desde ella hasta el primer suelo.
        private Rect GetTriggerZone()
        {
            Vector2 top = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(top + Vector2.down * 0.5f, Vector2.down, MaxZoneHeight, _groundLayers);
            float height = hit.collider != null ? hit.distance + 0.5f : MaxZoneHeight;
            return new Rect(top.x - _triggerWidth / 2f, top.y - height, _triggerWidth, height);
        }

        private void OnValidate()
        {
            if (_body == null) Debug.LogWarning($"[{name}] {nameof(Icicle)}: falta el Rigidbody2D.", this);
        }

        private void OnDrawGizmos()
        {
            if (_triggered) return;
            Rect zone = GetTriggerZone();
            Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
            Gizmos.DrawWireCube(zone.center, zone.size);
        }
    }
}
