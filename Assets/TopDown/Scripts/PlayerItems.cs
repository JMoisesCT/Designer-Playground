using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDown
{
    // Objetos que lleva el jugador: llaves (abren puertas) y el pulso EMP, que se usa con E y paraliza a los enemigos cercanos.
    public class PlayerItems : MonoBehaviour
    {
        [Header("Pulso EMP (tecla E)")]
        [Tooltip("Distancia a la que llega el pulso, en tiles (círculo azul alrededor del jugador).")]
        [SerializeField, Range(1f, 15f)] private float _empRadius = 5f;

        [Tooltip("Segundos que los enemigos se quedan paralizados.")]
        [SerializeField, Range(1f, 15f)] private float _empDuration = 4f;

        [Tooltip("Sonido al usar el pulso. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _empSound;

        [Header("Referencias")]
        [SerializeField] private InputActionReference _useAction;

        private bool _useHeld;

        public int Keys { get; set; }
        public bool HasEmp { get; set; }

        private void OnEnable()
        {
            _useAction.action.Enable();
        }

        private void Update()
        {
            bool usePressed = _useAction.action.IsPressed();
            if (usePressed && !_useHeld && HasEmp) UseEmp();
            _useHeld = usePressed;
        }

        // Paraliza a todos los enemigos dentro del círculo. Se gasta al usarlo.
        private void UseEmp()
        {
            HasEmp = false;
            Sound.Play(_empSound);
            foreach (Hazard hazard in FindObjectsByType<Hazard>(FindObjectsSortMode.None))
            {
                if (Vector2.Distance(hazard.transform.position, transform.position) <= _empRadius)
                    hazard.Stun(_empDuration);
            }
        }

        private void OnValidate()
        {
            if (_useAction == null) Debug.LogWarning($"[{name}] {nameof(PlayerItems)}: falta la acción de usar.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.6f, 1f);
            Gizmos.DrawWireSphere(transform.position, _empRadius);
        }
    }
}
