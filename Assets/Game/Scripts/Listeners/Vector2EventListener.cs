using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un Vector2EventChannel.
public class Vector2EventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private Vector2EventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Recibe el Vector2 del evento. Úsalo con funciones 'Dynamic Vector2'.")]
    [SerializeField] private UnityEvent<Vector2> _onEventRaised = new UnityEvent<Vector2>();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(Vector2 value)
    {
        _onEventRaised.Invoke(value);
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(Vector2EventListener)} sin canal asignado.", this);
    }
}
