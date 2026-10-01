using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un IntEventChannel.
public class IntEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private IntEventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Recibe el número del evento. Úsalo con funciones 'Dynamic int'.")]
    [SerializeField] private UnityEvent<int> _onEventRaised = new UnityEvent<int>();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(int value)
    {
        _onEventRaised.Invoke(value);
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(IntEventListener)} sin canal asignado.", this);
    }
}
