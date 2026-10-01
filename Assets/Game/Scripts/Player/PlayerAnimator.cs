using UnityEngine;

// Traduce el estado del movimiento y la vida a parámetros del Animator, aplica el personaje elegido y gira el sprite.
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int VelocityYId = Animator.StringToHash("VelocityY");
    private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
    private static readonly int IsWallSlidingId = Animator.StringToHash("IsWallSliding");
    private static readonly int IsDeadId = Animator.StringToHash("IsDead");
    private static readonly int DoubleJumpId = Animator.StringToHash("DoubleJump");
    private static readonly int HitId = Animator.StringToHash("Hit");

    [Header("Personaje")]
    [Tooltip("Personaje que se muestra. Solo cambia el aspecto, no el movimiento.")]
    [SerializeField] private CharacterSkinSO _skin;

    [Header("Referencias")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerHealth _health;

    public CharacterSkinSO Skin => _skin;

    private void Awake()
    {
        ApplySkin();
    }

    private void OnEnable()
    {
        if (_movement != null) _movement.AirJumped += HandleAirJumped;
        if (_health != null)
        {
            _health.Damaged += HandleDamaged;
            _health.Revived += HandleRevived;
        }
    }

    private void OnDisable()
    {
        if (_movement != null) _movement.AirJumped -= HandleAirJumped;
        if (_health != null)
        {
            _health.Damaged -= HandleDamaged;
            _health.Revived -= HandleRevived;
        }
    }

    private void Update()
    {
        if (_movement == null || _animator == null || _animator.runtimeAnimatorController == null) return;

        Vector2 velocity = _movement.Velocity;
        _animator.SetFloat(SpeedId, Mathf.Abs(velocity.x));
        _animator.SetFloat(VelocityYId, velocity.y);
        _animator.SetBool(IsGroundedId, _movement.IsGrounded);
        _animator.SetBool(IsWallSlidingId, _movement.IsWallSliding);

        // En la pared se orienta hacia ella (el dibujo ya tiene la espalda contra la pared); el resto, hacia donde avanza.
        int facing = _movement.IsWallSliding ? _movement.WallDirection : _movement.FacingDirection;
        if (_spriteRenderer != null) _spriteRenderer.flipX = facing < 0;
    }

    public void SetSkin(CharacterSkinSO skin)
    {
        _skin = skin;
        ApplySkin();
    }

    private void ApplySkin()
    {
        if (_skin == null) return;
        if (_animator != null && _animator.runtimeAnimatorController != _skin.AnimatorController)
            _animator.runtimeAnimatorController = _skin.AnimatorController;
        if (!Application.isPlaying && _spriteRenderer != null && _skin.PreviewSprite != null)
            _spriteRenderer.sprite = _skin.PreviewSprite;
    }

    private void HandleAirJumped() => _animator.SetTrigger(DoubleJumpId);

    private void HandleDamaged()
    {
        _animator.SetBool(IsDeadId, _health.IsDead);
        _animator.SetTrigger(HitId);
    }

    private void HandleRevived() => _animator.SetBool(IsDeadId, false);

    private void OnValidate()
    {
        if (_skin == null) Debug.LogWarning($"[{name}] {nameof(PlayerAnimator)} sin personaje (Character Skin).", this);
        if (_animator == null) Debug.LogWarning($"[{name}] {nameof(PlayerAnimator)} sin Animator.", this);
        if (_movement == null) Debug.LogWarning($"[{name}] {nameof(PlayerAnimator)} sin {nameof(PlayerMovement)}.", this);
        ApplySkin();
    }
}
