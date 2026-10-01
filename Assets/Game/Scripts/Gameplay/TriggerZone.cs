using UnityEngine;
using UnityEngine.Events;

// Zona invisible que lanza eventos y acciones cuando algo de las capas elegidas entra o sale.
[RequireComponent(typeof(BoxCollider2D))]
public class TriggerZone : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Capas que activan la zona. Por defecto, solo el jugador.")]
    [SerializeField] private LayerMask _detectedLayers;

    [Tooltip("Si está activo, la entrada y la salida solo se disparan la primera vez.")]
    [SerializeField] private bool _triggerOnce;

    [Header("Sender Events")]
    [Tooltip("(Opcional) Canal que se lanza al entrar.")]
    [SerializeField] private VoidEventChannel _eventOnEnter;

    [Tooltip("(Opcional) Canal que se lanza al salir.")]
    [SerializeField] private VoidEventChannel _eventOnExit;

    [Header("Respuesta")]
    [Tooltip("Acciones al entrar en la zona.")]
    [SerializeField] private UnityEvent _onEnter = new UnityEvent();

    [Tooltip("Acciones al salir de la zona.")]
    [SerializeField] private UnityEvent _onExit = new UnityEvent();

    private bool _enterFired;
    private bool _exitFired;

    private void Reset()
    {
        _detectedLayers = LayerMask.GetMask(GameLayers.Physics.Player);
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsDetected(other) || (_triggerOnce && _enterFired)) return;

        _enterFired = true;
        if (_eventOnEnter != null) _eventOnEnter.RaiseEvent();
        _onEnter.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsDetected(other) || (_triggerOnce && _exitFired)) return;

        _exitFired = true;
        if (_eventOnExit != null) _eventOnExit.RaiseEvent();
        _onExit.Invoke();
    }

    private bool IsDetected(Collider2D other)
    {
        return (_detectedLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private void OnValidate()
    {
        var zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider != null && !zoneCollider.isTrigger)
            Debug.LogWarning($"[{name}] El collider de {nameof(TriggerZone)} debe tener 'Is Trigger' activado.", this);
    }

    // Se dibuja siempre para que la zona sea visible en la escena.
    private void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        Gizmos.DrawWireCube(box.offset, box.size);
    }
}
