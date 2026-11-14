using System.Collections.Generic;
using UnityEngine;

namespace TopDown
{
    // Placa en el suelo: al pisarla (el jugador o una caja) enciende o apaga los objetos de su lista.
    // Un objeto apagado desaparece: sirve para abrir bloques, quitar trampas o hacer aparecer puentes y teletransportes.
    public class Switch : MonoBehaviour
    {
        [Tooltip("Objetos que cambia: los encendidos se apagan y los apagados se encienden. Arrástralos desde la Hierarchy.")]
        [SerializeField] private GameObject[] _targets = new GameObject[0];

        [Tooltip("Activado: al bajarse del interruptor, todo vuelve a como estaba. Desactivado: se queda pulsado para siempre.")]
        [SerializeField] private bool _holdToKeep;

        [Tooltip("Sonido al pulsarlo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        [Header("Referencias")]
        [SerializeField] private SpriteRenderer _sprite;
        [Tooltip("Dibujo sin pulsar.")]
        [SerializeField] private Sprite _upSprite;
        [Tooltip("Dibujo pulsado.")]
        [SerializeField] private Sprite _downSprite;

        private readonly List<Collider2D> _overlaps = new();
        private ContactFilter2D _filter;
        private bool _pressed;

        private void Awake()
        {
            _filter = new ContactFilter2D { useTriggers = false }; // solo cuerpos sólidos: pies del jugador, cajas...
        }

        private void FixedUpdate()
        {
            if (_pressed && !_holdToKeep) return; // ya se quedó pulsado para siempre

            bool somethingOnTop = IsSomethingOnTop();
            if (somethingOnTop == _pressed) return;
            _pressed = somethingOnTop;

            _sprite.sprite = _pressed ? _downSprite : _upSprite;
            if (_pressed) Sound.Play(_sound);
            foreach (GameObject target in _targets)
            {
                if (target != null) target.SetActive(!target.activeSelf);
            }
        }

        // Algo que se mueve (con Rigidbody2D dinámico) está encima de la placa.
        private bool IsSomethingOnTop()
        {
            Physics2D.OverlapBox(transform.position, new Vector2(0.6f, 0.6f), 0f, _filter, _overlaps);
            foreach (Collider2D overlap in _overlaps)
            {
                Rigidbody2D body = overlap.attachedRigidbody;
                if (body != null && body.bodyType == RigidbodyType2D.Dynamic) return true;
            }
            return false;
        }

        private void OnValidate()
        {
            if (_sprite == null || _upSprite == null || _downSprite == null)
                Debug.LogWarning($"[{name}] {nameof(Switch)}: faltan referencias.", this);
        }

        // Una línea hasta cada objeto que cambia.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (GameObject target in _targets)
            {
                if (target == null) continue;
                Gizmos.DrawLine(transform.position, target.transform.position);
                Gizmos.DrawWireCube(target.transform.position, Vector3.one * 0.9f);
            }
        }
    }
}
