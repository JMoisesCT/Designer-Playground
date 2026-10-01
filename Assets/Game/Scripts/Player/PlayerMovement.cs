using System;
using UnityEngine;

// Movimiento de plataformas sobre un Rigidbody2D dinámico: carrera, salto variable, coyote, buffer, doble salto y pared.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private const float MinMoveSpeed = 0.01f;

    [Header("Configuración")]
    [Tooltip("Asset con todo el 'feel' del movimiento. Cambia de preset arrastrando otro (Preciso, Floaty, Pesado...).")]
    [SerializeField] private PlayerMovementConfigSO _config;

    [Header("Referencias")]
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private Collider2D _collider;

    [Header("Detección")]
    [Tooltip("Capas que cuentan como suelo (por defecto Ground y OneWayPlatform).")]
    [SerializeField] private LayerMask _groundLayers;

    [Tooltip("Capas en las que se puede deslizar y hacer wall jump (por defecto solo Ground).")]
    [SerializeField] private LayerMask _wallLayers;

    private ContactFilter2D _groundFilter;
    private ContactFilter2D _leftWallFilter;
    private ContactFilter2D _rightWallFilter;

    private bool _controlEnabled = true;
    private bool _jumpRequested;
    private bool _isJumping;
    private int _airJumpsLeft;
    private int _lastWallDirection;
    private float _jumpBufferTimer;
    private float _coyoteTimer;
    private float _wallCoyoteTimer;
    private float _controlLockTimer;

    public event Action Jumped;
    public event Action AirJumped;
    public event Action WallJumped;
    public event Action Landed;

    public PlayerMovementConfigSO Config => _config;
    public Vector2 Velocity => _body.linearVelocity;
    public bool IsGrounded { get; private set; }
    public bool IsWallSliding { get; private set; }
    public int WallDirection { get; private set; }
    public int FacingDirection { get; private set; } = 1;

    private void Reset()
    {
        _input = GetComponent<PlayerInputReader>();
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundLayers = LayerMask.GetMask(GameLayers.Physics.Ground, GameLayers.Physics.OneWayPlatform);
        _wallLayers = LayerMask.GetMask(GameLayers.Physics.Ground);
    }

    private void Awake()
    {
        // La gravedad la aplica este script para que salto, caída y gizmo usen las mismas fórmulas.
        _body.gravityScale = 0f;

        // Las normales de contacto apuntan hacia el jugador: suelo = 90°, pared a la izquierda = 0°, a la derecha = 180°.
        _groundFilter = CreateFilter(_groundLayers, 45f, 135f, false);
        _leftWallFilter = CreateFilter(_wallLayers, 30f, 330f, true);
        _rightWallFilter = CreateFilter(_wallLayers, 150f, 210f, false);
    }

    private void OnEnable()
    {
        if (_input != null) _input.JumpPressed += HandleJumpPressed;
    }

    private void OnDisable()
    {
        if (_input != null) _input.JumpPressed -= HandleJumpPressed;
    }

    private void FixedUpdate()
    {
        if (_config == null) return;

        float dt = Time.fixedDeltaTime;
        float moveX = _controlEnabled && _input != null ? _input.MoveX : 0f;
        bool jumpHeld = _controlEnabled && _input != null && _input.JumpHeld;
        Vector2 velocity = _body.linearVelocity;

        UpdateContacts(velocity, moveX);
        UpdateTimers(dt);

        velocity.x = ComputeHorizontalVelocity(velocity.x, moveX, dt);
        velocity.y = ComputeVerticalVelocity(velocity.y, jumpHeld, dt);
        velocity = TryJump(velocity);

        if (_controlLockTimer <= 0f && moveX != 0f) FacingDirection = moveX > 0f ? 1 : -1;

        _body.linearVelocity = velocity;
    }

    // Empuje externo (daño, trampolines). Bloquea el control horizontal durante lockTime segundos.
    public void ApplyImpulse(Vector2 newVelocity, float lockTime)
    {
        _body.linearVelocity = newVelocity;
        _isJumping = false;
        _controlLockTimer = Mathf.Max(_controlLockTimer, lockTime);
    }

    public void SetControlEnabled(bool isEnabled)
    {
        _controlEnabled = isEnabled;
        if (!isEnabled) _jumpRequested = false;
    }

    private void HandleJumpPressed()
    {
        if (!_controlEnabled || _config == null) return;
        _jumpRequested = true;
        _jumpBufferTimer = _config.JumpBufferTime;
    }

    private void UpdateContacts(Vector2 velocity, float moveX)
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = velocity.y <= MinMoveSpeed && _body.IsTouching(_groundFilter);

        WallDirection = 0;
        if (!IsGrounded && _config.WallMovementEnabled)
        {
            if (_body.IsTouching(_leftWallFilter)) WallDirection = -1;
            else if (_body.IsTouching(_rightWallFilter)) WallDirection = 1;
        }

        IsWallSliding = WallDirection != 0 && moveX == WallDirection && velocity.y <= 0f;

        if (IsGrounded)
        {
            _coyoteTimer = _config.CoyoteTime;
            _airJumpsLeft = _config.AirJumps;
            if (!wasGrounded) Landed?.Invoke();
        }

        if (IsWallSliding)
        {
            _wallCoyoteTimer = _config.WallCoyoteTime;
            _lastWallDirection = WallDirection;
            if (_config.WallRefreshesAirJumps) _airJumpsLeft = _config.AirJumps;
        }
    }

    private void UpdateTimers(float dt)
    {
        if (!IsGrounded) _coyoteTimer -= dt;
        if (!IsWallSliding) _wallCoyoteTimer -= dt;
        _controlLockTimer -= dt;
    }

    private float ComputeHorizontalVelocity(float currentX, float moveX, float dt)
    {
        if (_controlLockTimer > 0f) return currentX;
        return StepHorizontal(_config, currentX, moveX, IsGrounded, dt);
    }

    private float ComputeVerticalVelocity(float currentY, bool jumpHeld, float dt)
    {
        if (currentY <= 0f) _isJumping = false;

        float y = StepVertical(_config, currentY, _isJumping && !jumpHeld, dt);
        if (IsWallSliding) y = Mathf.Max(y, -_config.WallSlideMaxSpeed);
        return y;
    }

    private Vector2 TryJump(Vector2 velocity)
    {
        if (!_jumpRequested) return velocity;

        bool touchingWall = WallDirection != 0 || _wallCoyoteTimer > 0f;
        if (IsGrounded || _coyoteTimer > 0f)
        {
            velocity.y = _config.JumpVelocity;
            _coyoteTimer = 0f;
            ConsumeJump();
            Jumped?.Invoke();
        }
        else if (touchingWall && _config.WallMovementEnabled)
        {
            int wallDirection = WallDirection != 0 ? WallDirection : _lastWallDirection;
            velocity = new Vector2(-wallDirection * _config.WallJumpPushSpeed, _config.WallJumpVelocity);
            FacingDirection = -wallDirection;
            _controlLockTimer = _config.WallJumpControlLockTime;
            _wallCoyoteTimer = 0f;
            if (_config.WallRefreshesAirJumps) _airJumpsLeft = _config.AirJumps;
            ConsumeJump();
            WallJumped?.Invoke();
        }
        else if (_airJumpsLeft > 0)
        {
            velocity.y = _config.AirJumpVelocity;
            _airJumpsLeft--;
            ConsumeJump();
            AirJumped?.Invoke();
        }
        else
        {
            // Se mantiene la petición mientras dure el buffer; al menos se evalúa un paso aunque el buffer sea 0.
            _jumpBufferTimer -= Time.fixedDeltaTime;
            if (_jumpBufferTimer <= 0f) _jumpRequested = false;
        }

        return velocity;
    }

    private void ConsumeJump()
    {
        _jumpRequested = false;
        _isJumping = true;
    }

    // Pasos de integración compartidos con PlayerJumpArc para que el gizmo coincida con el juego.
    public static float StepHorizontal(PlayerMovementConfigSO config, float currentX, float moveX, bool grounded, float dt)
    {
        float target = moveX * config.MaxRunSpeed;
        bool accelerating = Mathf.Abs(target) > MinMoveSpeed
                            && (Mathf.Abs(currentX) < MinMoveSpeed || Mathf.Sign(target) == Mathf.Sign(currentX))
                            && Mathf.Abs(currentX) <= config.MaxRunSpeed;

        float rate = grounded
            ? (accelerating ? config.GroundAcceleration : config.GroundDeceleration)
            : (accelerating ? config.AirAcceleration : config.AirDeceleration);

        return Mathf.MoveTowards(currentX, target, rate * dt);
    }

    public static float StepVertical(PlayerMovementConfigSO config, float currentY, bool jumpCut, float dt)
    {
        float multiplier = 1f;
        if (currentY < 0f) multiplier = config.FallGravityMultiplier;
        else if (jumpCut) multiplier = config.JumpCutGravityMultiplier;

        float y = currentY - config.Gravity * multiplier * dt;
        return Mathf.Max(y, -config.MaxFallSpeed);
    }

    private static ContactFilter2D CreateFilter(LayerMask layers, float minAngle, float maxAngle, bool outside)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(layers);
        filter.SetNormalAngle(minAngle, maxAngle);
        filter.useOutsideNormalAngle = outside;
        return filter;
    }

    private void OnValidate()
    {
        if (_config == null) Debug.LogWarning($"[{name}] {nameof(PlayerMovement)} sin {nameof(PlayerMovementConfigSO)} asignado.", this);
        if (_input == null) Debug.LogWarning($"[{name}] {nameof(PlayerMovement)} sin {nameof(PlayerInputReader)}.", this);
        if (_body == null) Debug.LogWarning($"[{name}] {nameof(PlayerMovement)} sin Rigidbody2D asignado.", this);
        if (_collider == null) Debug.LogWarning($"[{name}] {nameof(PlayerMovement)} sin Collider2D asignado.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (_config == null || _collider == null) return;

        Bounds bounds = _collider.bounds;
        var feet = new Vector2(bounds.center.x, bounds.min.y);
        int direction = Application.isPlaying ? FacingDirection : 1;
        PlayerJumpArc.DrawGizmo(_config, feet, direction, bounds.size.x);
    }
}
