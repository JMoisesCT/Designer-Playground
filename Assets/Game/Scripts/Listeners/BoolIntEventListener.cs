using UnityEngine;
using UnityEngine.Events;

// Ejecuta las acciones del Inspector cuando se lanza un BoolIntEventChannel.
// Con el filtro por id sirve para enlazar interruptores y puertas que comparten el mismo número.
public class BoolIntEventListener : MonoBehaviour
{
    [Header("Listener Events")]
    [Tooltip("Canal que se escucha. El objeto debe estar activo para escuchar.")]
    [SerializeField] private BoolIntEventChannel _channel;

    [Header("Filtro")]
    [Tooltip("Si está activo, solo responde cuando el número del evento coincide con 'Id'.")]
    [SerializeField] private bool _filterById;

    [Tooltip("Número que debe traer el evento para responder (p. ej. el id del interruptor).")]
    [SerializeField] private int _id;

    [Header("Respuesta")]
    [Tooltip("Recibe el bool y el número del evento.")]
    [SerializeField] private UnityEvent<bool, int> _onEventRaised = new UnityEvent<bool, int>();

    [Tooltip("Solo cuando el bool es true.")]
    [SerializeField] private UnityEvent _onTrue = new UnityEvent();

    [Tooltip("Solo cuando el bool es false.")]
    [SerializeField] private UnityEvent _onFalse = new UnityEvent();

    private void OnEnable()
    {
        if (_channel != null) _channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (_channel != null) _channel.OnEventRaised -= Respond;
    }

    private void Respond(bool boolValue, int intValue)
    {
        if (_filterById && intValue != _id) return;

        _onEventRaised.Invoke(boolValue, intValue);
        if (boolValue) _onTrue.Invoke();
        else _onFalse.Invoke();
    }

    private void OnValidate()
    {
        if (_channel == null) Debug.LogWarning($"[{name}] {nameof(BoolIntEventListener)} sin canal asignado.", this);
    }
}
