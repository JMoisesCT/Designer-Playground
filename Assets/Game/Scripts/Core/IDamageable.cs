using UnityEngine;

// Cualquier cosa que pueda recibir daño (jugador, cajas, enemigos). Las trampas solo conocen esta interfaz.
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 sourcePosition);
}
