using System.Collections;
using UnityEngine;

namespace TopDown
{
    // Trampa que se enciende y se apaga sola (pinchos que salen del suelo). Solo hace daño mientras está encendida.
    public class TimedHazard : MonoBehaviour
    {
        [Header("Tiempos")]
        [Tooltip("Segundos encendida: hace daño.")]
        [SerializeField, Range(0.2f, 5f)] private float _onTime = 1f;

        [Tooltip("Segundos apagada: se puede pasar.")]
        [SerializeField, Range(0.2f, 5f)] private float _offTime = 1.5f;

        [Tooltip("Segundos que espera apagada al empezar el nivel. Con retrasos distintos en varias trampas seguidas, se encienden en ola.")]
        [SerializeField, Range(0f, 5f)] private float _startDelay;

        [Header("Referencias")]
        [Tooltip("Collider que hace daño (con Is Trigger). Solo está activo mientras la trampa está encendida.")]
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sprite;
        [Tooltip("Dibujo encendida.")]
        [SerializeField] private Sprite _onSprite;
        [Tooltip("Dibujo apagada.")]
        [SerializeField] private Sprite _offSprite;

        private IEnumerator Start()
        {
            SetOn(false);
            yield return new WaitForSeconds(_startDelay);
            while (true)
            {
                SetOn(true);
                yield return new WaitForSeconds(_onTime);
                SetOn(false);
                yield return new WaitForSeconds(_offTime);
            }
        }

        private void SetOn(bool on)
        {
            _collider.enabled = on;
            _sprite.sprite = on ? _onSprite : _offSprite;
        }

        private void OnValidate()
        {
            if (_collider == null || _sprite == null || _onSprite == null || _offSprite == null)
                Debug.LogWarning($"[{name}] {nameof(TimedHazard)}: faltan referencias.", this);
        }
    }
}
