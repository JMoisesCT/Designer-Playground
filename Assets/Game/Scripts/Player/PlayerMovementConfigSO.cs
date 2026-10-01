using UnityEngine;

// Parámetros de movimiento del jugador. Duplica el asset para crear otro "feel".
[CreateAssetMenu(menuName = "Designer Playground/Jugador/Movement Config", fileName = "PlayerMovementConfig")]
public class PlayerMovementConfigSO : ScriptableObject
{
    [Header("Carrera")]
    [Tooltip("Velocidad horizontal máxima (tiles por segundo).")]
    [SerializeField, Min(0.1f)] private float _maxRunSpeed = 8f;

    [Tooltip("Segundos en suelo para pasar de quieto a velocidad máxima. 0 = instantáneo.")]
    [SerializeField, Range(0f, 1f)] private float _groundAccelerationTime = 0.05f;

    [Tooltip("Segundos en suelo para frenar desde velocidad máxima (también al dar la vuelta). 0 = instantáneo.")]
    [SerializeField, Range(0f, 1f)] private float _groundDecelerationTime = 0.04f;

    [Tooltip("Segundos en el aire para alcanzar la velocidad máxima. Más alto = menos control en el aire.")]
    [SerializeField, Range(0f, 1f)] private float _airAccelerationTime = 0.08f;

    [Tooltip("Segundos en el aire para frenar al soltar la dirección. Más alto = conserva más la inercia.")]
    [SerializeField, Range(0f, 1f)] private float _airDecelerationTime = 0.12f;

    [Header("Salto")]
    [Tooltip("Altura máxima del salto manteniendo el botón (tiles).")]
    [SerializeField, Range(0.5f, 10f)] private float _jumpHeight = 3.2f;

    [Tooltip("Segundos que tarda en llegar al punto más alto. Más alto = salto más lento y flotante.")]
    [SerializeField, Range(0.1f, 1.5f)] private float _timeToApex = 0.36f;

    [Tooltip("Multiplicador de gravedad al caer. >1 = caída más rápida que la subida (más 'peso').")]
    [SerializeField, Range(0.5f, 5f)] private float _fallGravityMultiplier = 1.8f;

    [Tooltip("Multiplicador de gravedad al soltar el botón mientras sube. Más alto = saltos cortos más cortos.")]
    [SerializeField, Range(1f, 6f)] private float _jumpCutGravityMultiplier = 3f;

    [Tooltip("Velocidad máxima de caída (tiles por segundo).")]
    [SerializeField, Min(1f)] private float _maxFallSpeed = 16f;

    [Tooltip("Segundos tras salir de un borde en los que todavía se puede saltar (coyote time).")]
    [SerializeField, Range(0f, 0.3f)] private float _coyoteTime = 0.1f;

    [Tooltip("Segundos que se recuerda el botón de salto pulsado antes de tocar el suelo (jump buffer).")]
    [SerializeField, Range(0f, 0.3f)] private float _jumpBufferTime = 0.12f;

    [Header("Doble salto")]
    [Tooltip("Saltos extra en el aire. 0 = sin doble salto, 1 = doble salto, 2 = triple...")]
    [SerializeField, Range(0, 3)] private int _airJumps = 1;

    [Tooltip("Altura de cada salto en el aire (tiles), medida desde donde se pulsa.")]
    [SerializeField, Range(0.5f, 10f)] private float _airJumpHeight = 2.2f;

    [Header("Pared")]
    [Tooltip("Permite deslizar por paredes y saltar desde ellas.")]
    [SerializeField] private bool _wallMovementEnabled = true;

    [Tooltip("Velocidad máxima de caída deslizando por una pared (se activa empujando hacia ella).")]
    [SerializeField, Min(0f)] private float _wallSlideMaxSpeed = 3f;

    [Tooltip("Altura del salto desde la pared (tiles).")]
    [SerializeField, Range(0.5f, 10f)] private float _wallJumpHeight = 2.8f;

    [Tooltip("Velocidad horizontal con la que el salto de pared empuja hacia fuera.")]
    [SerializeField, Min(0f)] private float _wallJumpPushSpeed = 8f;

    [Tooltip("Segundos sin control horizontal tras saltar de la pared. Bajo (~0.15) permite escalar una sola pared; alto (>0.3) obliga a usar dos paredes.")]
    [SerializeField, Range(0f, 0.6f)] private float _wallJumpControlLockTime = 0.15f;

    [Tooltip("Segundos tras soltarse de la pared en los que todavía se puede hacer wall jump.")]
    [SerializeField, Range(0f, 0.3f)] private float _wallCoyoteTime = 0.1f;

    [Tooltip("Agarrarse a una pared o saltar desde ella recarga los saltos en el aire.")]
    [SerializeField] private bool _wallRefreshesAirJumps = true;

    public float MaxRunSpeed => _maxRunSpeed;
    public float GroundAcceleration => RateFromTime(_groundAccelerationTime);
    public float GroundDeceleration => RateFromTime(_groundDecelerationTime);
    public float AirAcceleration => RateFromTime(_airAccelerationTime);
    public float AirDeceleration => RateFromTime(_airDecelerationTime);

    public float JumpHeight => _jumpHeight;
    public float FallGravityMultiplier => _fallGravityMultiplier;
    public float JumpCutGravityMultiplier => _jumpCutGravityMultiplier;
    public float MaxFallSpeed => _maxFallSpeed;
    public float CoyoteTime => _coyoteTime;
    public float JumpBufferTime => _jumpBufferTime;

    public int AirJumps => _airJumps;
    public float AirJumpHeight => _airJumpHeight;

    public bool WallMovementEnabled => _wallMovementEnabled;
    public float WallSlideMaxSpeed => _wallSlideMaxSpeed;
    public float WallJumpPushSpeed => _wallJumpPushSpeed;
    public float WallJumpControlLockTime => _wallJumpControlLockTime;
    public float WallCoyoteTime => _wallCoyoteTime;
    public bool WallRefreshesAirJumps => _wallRefreshesAirJumps;

    // Gravedad derivada de altura y tiempo (h = g·t²/2); las velocidades se ajustan para que la altura sea exacta.
    public float Gravity => 2f * _jumpHeight / (_timeToApex * _timeToApex);
    public float JumpVelocity => VelocityForHeight(_jumpHeight);
    public float AirJumpVelocity => VelocityForHeight(_airJumpHeight);
    public float WallJumpVelocity => VelocityForHeight(_wallJumpHeight);

    // Con pasos fijos la altura real es v²/2g + v·dt/2; se resuelve v para que coincida con la del Inspector.
    private float VelocityForHeight(float height)
    {
        float halfStep = Gravity * Time.fixedDeltaTime * 0.5f;
        return Mathf.Sqrt(halfStep * halfStep + 2f * Gravity * height) - halfStep;
    }

    private float RateFromTime(float seconds)
    {
        return seconds <= 0f ? float.PositiveInfinity : _maxRunSpeed / seconds;
    }
}
