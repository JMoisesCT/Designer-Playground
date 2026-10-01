using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un TransformEventChannel.
public class TransformEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private TransformEventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Recibe el Transform del evento. Úsalo con funciones 'Dynamic Transform'.")]
    [SerializeField] private UnityEvent<Transform> _onEventRaised = new UnityEvent<Transform>();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(Transform value)
    {
        _onEventRaised.Invoke(value);
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(TransformEventListener)} sin canal asignado.", this);
    }
}
