using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.15f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
            KnockbackController knockbackController = other.GetComponentInChildren<KnockbackController>();
            
            if (playerHealth != null)
            {
                // Calcular dirección de knockback (dirección contraria: del enemigo hacia el jugador)
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                
                // PASO 1: Aplicar knockback SOLO si el jugador NO está atacando
                if (knockbackController != null && (playerAttack == null || !playerAttack.IsAttackFreezing))
                {
                    knockbackController.ApplyHitKnockback(
                        direction: knockbackDir,
                        strength: knockbackForce,
                        duration: knockbackDuration,
                        options: default
                    );
                }
                
                // PASO 2: Aplicar daño (operación separada)
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
