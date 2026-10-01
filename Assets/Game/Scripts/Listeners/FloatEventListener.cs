using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un FloatEventChannel.
public class FloatEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private FloatEventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Recibe el número decimal del evento. Úsalo con funciones 'Dynamic float'.")]
    [SerializeField] private UnityEvent<float> _onEventRaised = new UnityEvent<float>();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(float value)
    {
        _onEventRaised.Invoke(value);
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(FloatEventListener)} sin canal asignado.", this);
    }
}
