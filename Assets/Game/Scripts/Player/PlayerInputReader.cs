using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Lee Move y Jump del Input System y los expone al resto del jugador. Se silencia al pausar o al desactivar el input.
public class PlayerInputReader : MonoBehaviour
{
    [Header("Acciones")]
    [Tooltip("Acción Player/Move de InputSystem_Actions.")]
    [SerializeField] private InputActionReference _moveAction;

    [Tooltip("Acción Player/Jump de InputSystem_Actions.")]
    [SerializeField] private InputActionReference _jumpAction;

    [Tooltip("Inclinación mínima del stick para empezar a correr. El movimiento es digital: siempre a velocidad máxima.")]
    [SerializeField, Range(0.05f, 0.9f)] private float _moveDeadZone = 0.3f;

    [Header("Listener Events")]
    [Tooltip("false = el jugador deja de responder (cinemáticas, transiciones de cámara).")]
    [SerializeField] private BoolEventChannel _eventPlayerInputEnabled;

    [Tooltip("true = juego en pausa: se ignora el input.")]
    [SerializeField] private BoolEventChannel _eventGamePaused;

    private bool _inputEnabled = true;
    private bool _paused;

    public event Action JumpPressed;

    public float MoveX { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool IsActive => _inputEnabled && !_paused;

    private void OnEnable()
    {
        if (_eventPlayerInputEnabled != null) _eventPlayerInputEnabled.OnEventRaised += HandleInputEnabled;
        if (_eventGamePaused != null) _eventGamePaused.OnEventRaised += HandleGamePaused;

        // No se desactivan en OnDisable: el asset puede ser el de acciones globales del proyecto.
        if (_moveAction != null) _moveAction.action.Enable();
        if (_jumpAction != null)
        {
            _jumpAction.action.Enable();
            _jumpAction.action.performed += HandleJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (_eventPlayerInputEnabled != null) _eventPlayerInputEnabled.OnEventRaised -= HandleInputEnabled;
        if (_eventGamePaused != null) _eventGamePaused.OnEventRaised -= HandleGamePaused;
        if (_jumpAction != null) _jumpAction.action.performed -= HandleJumpPerformed;
        ClearState();
    }

    private void Update()
    {
        if (!IsActive)
        {
            ClearState();
            return;
        }

        float x = _moveAction != null ? _moveAction.action.ReadValue<Vector2>().x : 0f;
        MoveX = Mathf.Abs(x) >= _moveDeadZone ? Mathf.Sign(x) : 0f;
        JumpHeld = _jumpAction != null && _jumpAction.action.IsPressed();
    }

    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        if (!IsActive) return;
        JumpHeld = true;
        JumpPressed?.Invoke();
    }

    private void HandleInputEnabled(bool isEnabled) => _inputEnabled = isEnabled;

    private void HandleGamePaused(bool isPaused) => _paused = isPaused;

    private void ClearState()
    {
        MoveX = 0f;
        JumpHeld = false;
    }

    private void OnValidate()
    {
        if (_moveAction == null) Debug.LogWarning($"[{name}] {nameof(PlayerInputReader)} sin acción Move asignada.", this);
        if (_jumpAction == null) Debug.LogWarning($"[{name}] {nameof(PlayerInputReader)} sin acción Jump asignada.", this);
        if (_eventPlayerInputEnabled == null) Debug.LogWarning($"[{name}] Falta el canal EventPlayerInputEnabled.", this);
        if (_eventGamePaused == null) Debug.LogWarning($"[{name}] Falta el canal EventGamePaused.", this);
    }
}
