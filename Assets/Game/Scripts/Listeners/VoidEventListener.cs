using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un VoidEventChannel.
public class VoidEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private VoidEventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Acciones que se ejecutan cuando se lanza el evento.")]
    [SerializeField] private UnityEvent _onEventRaised = new UnityEvent();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond()
    {
        _onEventRaised.Invoke();
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(VoidEventListener)} sin canal asignado.", this);
    }
}
