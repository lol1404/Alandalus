using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAttack : MonoBehaviour
{
    public Transform attackPoint; // Asignado desde el inspector
    public LayerMask enemyLayers;

    private WeaponData currentWeapon;
    private BloodTearsManager tearsManager;

    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
    }

    public void Attack()
    {
        if (tearsManager == null)
        {
            tearsManager = GetComponent<BloodTearsManager>();
        }
        if (currentWeapon == null) return;

        // Obtener dirección hacia el mouse usando el nuevo Input System
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        Vector2 attackDir = (mousePos - transform.position).normalized;

        // Posición del centro del área de ataque
        Vector2 attackOrigin = (Vector2)transform.position + attackDir * currentWeapon.attackRange * 0.5f;

        // Ejecutar el ataque (usando OverlapBox para un área rectangular)
        Vector2 boxSize = new Vector2(currentWeapon.attackRange, currentWeapon.attackRadius);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackOrigin, boxSize, AngleFromVector(attackDir), enemyLayers);

        bool enemyHit = hitEnemies.Length > 0;

        foreach (var enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth == null) continue;

            // Otorgar lágrimas por golpe
            if (tearsManager != null)
                tearsManager.AddTears(enemyHealth.tearGainOnHit);

            // Calcular dirección de knockback individual para cada enemigo
            Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            
            // PASO 1: Aplicar knockback a enemigos INMEDIATAMENTE
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.AddForce(knockbackDir * currentWeapon.knockbackForce, ForceMode2D.Impulse);
            }
            
            // PASO 2: Hacer daño (operación separada)
            enemyHealth.TakeDamage(currentWeapon.damage, this.gameObject);
        }

        // Efectos visuales (SOLO si golpeaste algo)
        if (enemyHit && currentWeapon.attackEffectPrefab)
            Instantiate(currentWeapon.attackEffectPrefab, attackOrigin, Quaternion.identity);

        // Sonido (SOLO si golpeaste algo)
        if (enemyHit && currentWeapon.attackSound)
            AudioSource.PlayClipAtPoint(currentWeapon.attackSound, transform.position);

        // PASO 3: Aplicar knockback al jugador (recoil) - SOLO SI GOLPEASTE ALGO
        KnockbackController playerKnockback = GetComponentInChildren<KnockbackController>();
        if (playerKnockback != null)
        {
            if (enemyHit)
            {
                // Golpeaste algo: aplicar knockback de recoil en dirección OPUESTA al ataque
                if (currentWeapon.playerAttackKnockbackForce > 0f)
                {
                    Vector2 recoilDir = -attackDir; // Dirección opuesta a donde mirabas
                    playerKnockback.ApplyHitKnockback(
                        direction: recoilDir,
                        strength: currentWeapon.playerAttackKnockbackForce,
                        duration: currentWeapon.playerAttackKnockbackDuration,
                        options: default
                    );
                    Debug.Log($"[WeaponAttack] Golpeaste: aplicando knockback en dirección {recoilDir}");
                }
            }
            else
            {
                // No golpeaste nada: cancelar cualquier knockback anterior
                playerKnockback.CancelKnockback();
                Debug.Log($"[WeaponAttack] Ataque fallido: cancelando knockback anterior");
            }
        }
    }

    // Utilidad para calcular el ángulo de rotación hacia el mouse
    float AngleFromVector(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    // Para visualizar el área en el editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || currentWeapon == null || Camera.main == null || Mouse.current == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        Vector2 attackDir = (mousePos - transform.position).normalized;
        Vector2 attackOrigin = (Vector2)transform.position + attackDir * currentWeapon.attackRange * 0.5f;
        Vector2 boxSize = new Vector2(currentWeapon.attackRange, currentWeapon.attackRadius);
        float angle = AngleFromVector(attackDir);

        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackOrigin, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
