using System;
using UnityEngine;

// Vida del jugador: recibe daño, aplica empuje e invulnerabilidad y avisa de la muerte por EventPlayerDied.
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [Tooltip("Golpes que aguanta. 1 = muere al primer golpe.")]
    [SerializeField, Min(1)] private int _maxHealth = 1;

    [Tooltip("Segundos de invulnerabilidad (con parpadeo) tras un golpe que no mata.")]
    [SerializeField, Range(0f, 3f)] private float _invulnerabilityTime = 1f;

    [Tooltip("Segundos entre parpadeos durante la invulnerabilidad.")]
    [SerializeField, Range(0.02f, 0.5f)] private float _blinkInterval = 0.08f;

    [Header("Empuje al recibir daño")]
    [Tooltip("Velocidad del empuje: X se aleja del origen del daño, Y es hacia arriba.")]
    [SerializeField] private Vector2 _knockbackVelocity = new Vector2(6f, 8f);

    [Tooltip("Segundos sin control horizontal tras el empuje.")]
    [SerializeField, Range(0f, 1f)] private float _knockbackControlLockTime = 0.25f;

    [Header("Referencias")]
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Sender Events")]
    [SerializeField] private VoidEventChannel _eventPlayerDied;

    private float _invulnerableTimer;

    public event Action Damaged;
    public event Action Died;
    public event Action Revived;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => _maxHealth;
    public bool IsDead { get; private set; }
    public bool IsInvulnerable => _invulnerableTimer > 0f;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    private void OnDisable()
    {
        _invulnerableTimer = 0f;
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
    }

    private void Update()
    {
        if (_invulnerableTimer <= 0f) return;

        _invulnerableTimer -= Time.deltaTime;
        bool visible = _invulnerableTimer <= 0f || Mathf.FloorToInt(_invulnerableTimer / _blinkInterval) % 2 == 0;
        if (_spriteRenderer != null) _spriteRenderer.enabled = visible;
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsDead || IsInvulnerable || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        IsDead = CurrentHealth == 0;
        ApplyKnockback(sourcePosition);
        Damaged?.Invoke();

        if (!IsDead)
        {
            _invulnerableTimer = _invulnerabilityTime;
            return;
        }

        if (_movement != null) _movement.SetControlEnabled(false);
        Died?.Invoke();
        if (_eventPlayerDied != null) _eventPlayerDied.RaiseEvent();
    }

    // Para el respawn (fase 3): vida completa y control devuelto.
    public void Revive()
    {
        IsDead = false;
        CurrentHealth = _maxHealth;
        _invulnerableTimer = 0f;
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        if (_movement != null) _movement.SetControlEnabled(true);
        Revived?.Invoke();
    }

    [ContextMenu("Probar: recibir 1 de daño")]
    private void DebugTakeDamage()
    {
        int facing = _movement != null ? _movement.FacingDirection : 1;
        TakeDamage(1, (Vector2)transform.position + Vector2.right * facing);
    }

    private void ApplyKnockback(Vector2 sourcePosition)
    {
        if (_movement == null) return;

        float side = Mathf.Sign(transform.position.x - sourcePosition.x);
        if (Mathf.Approximately(transform.position.x, sourcePosition.x)) side = -_movement.FacingDirection;
        _movement.ApplyImpulse(new Vector2(_knockbackVelocity.x * side, _knockbackVelocity.y), _knockbackControlLockTime);
    }

    private void OnValidate()
    {
        if (_movement == null) Debug.LogWarning($"[{name}] {nameof(PlayerHealth)} sin {nameof(PlayerMovement)} (no habrá empuje).", this);
        if (_spriteRenderer == null) Debug.LogWarning($"[{name}] {nameof(PlayerHealth)} sin SpriteRenderer (no habrá parpadeo).", this);
        if (_eventPlayerDied == null) Debug.LogWarning($"[{name}] Falta el canal EventPlayerDied.", this);
    }
}
