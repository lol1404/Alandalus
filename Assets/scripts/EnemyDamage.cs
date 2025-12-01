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
            // Use the PlayerHealth unified receiver which follows the timeline:
            // - pre-checks (iFrames/knockback active)
            // - apply damage
            // - if alive, apply knockback; if dead, call death without knockback
            if (playerHealth != null)
            {
                // Source position is this enemy's position (knockback direction will be computed inside PlayerHealth)
                Vector2 sourcePos = transform.position;

                // Try to deliver damage following the timeline
                playerHealth.ReceiveDamageFromSource(damage, sourcePos, knockbackForce, knockbackDuration, options: default);
            }
        }
    }
}
