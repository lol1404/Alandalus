using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Efectos de Muerte")]
    public GameObject deathEffectPrefab;  // Efecto visual al morir (opcional)
    public AudioClip deathSound;          // Sonido al morir (opcional)

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0) damage = 0;
        
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        
        // Efecto visual
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Sonido
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Destruir el enemigo
        Destroy(gameObject);
    }
}
