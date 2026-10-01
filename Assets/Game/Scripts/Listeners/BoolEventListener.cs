using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un BoolEventChannel.
public class BoolEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private BoolEventChannel _channel;

    [Header("Respuesta")]
    [Tooltip("Recibe el valor del evento (true/false). Úsalo con funciones 'Dynamic bool', p. ej. GameObject.SetActive.")]
    [SerializeField] private UnityEvent<bool> _onEventRaised = new UnityEvent<bool>();

    [Tooltip("Solo cuando el valor es true.")]
    [SerializeField] private UnityEvent _onTrue = new UnityEvent();

    [Tooltip("Solo cuando el valor es false.")]
    [SerializeField] private UnityEvent _onFalse = new UnityEvent();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(bool value)
    {
        _onEventRaised.Invoke(value);
        if (value) _onTrue.Invoke();
        else _onFalse.Invoke();
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(BoolEventListener)} sin canal asignado.", this);
    }
}
