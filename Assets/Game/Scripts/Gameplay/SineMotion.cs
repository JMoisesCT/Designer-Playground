using UnityEngine;

/// <summary>
/// Movimiento de vaivén suave (flotar, balancearse). Útil para decoración,
/// coleccionables que flotan o plataformas simples sin ruta.
/// </summary>
public class SineMotion : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Dirección y distancia máxima del vaivén, en unidades (1 unidad = 1 tile de 16 px).")]
    [SerializeField] private Vector2 _amplitude = new Vector2(0f, 0.25f);

    [Tooltip("Ciclos completos por segundo. 0.5 = un vaivén cada 2 segundos.")]
    [SerializeField, Min(0f)] private float _frequency = 0.5f;

    [Tooltip("Desfase del ciclo (0-1). Úsalo para que varios objetos iguales no se muevan sincronizados.")]
    [SerializeField, Range(0f, 1f)] private float _phase;

    private Vector3 _startLocalPosition;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float wave = Mathf.Sin((Time.time * _frequency + _phase) * Mathf.PI * 2f);
        transform.localPosition = _startLocalPosition + (Vector3)(_amplitude * wave);
    }

    private void OnDrawGizmosSelected()
    {
        Transform parent = transform.parent;
        Vector3 localCenter = Application.isPlaying ? _startLocalPosition : transform.localPosition;
        Vector3 center = parent != null ? parent.TransformPoint(localCenter) : localCenter;
        Vector3 offset = parent != null ? parent.TransformVector(_amplitude) : (Vector3)_amplitude;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center - offset, center + offset);
        Gizmos.DrawWireSphere(center - offset, 0.1f);
        Gizmos.DrawWireSphere(center + offset, 0.1f);
    }
}
