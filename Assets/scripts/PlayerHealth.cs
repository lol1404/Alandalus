using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Invulnerabilidad - iFrames")]
    [SerializeField] private float invincibility_time = 1.5f;
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;

    [Header("Parpadeo Progresivo - Blink")]
    [SerializeField] private float blink_max_brightness = 1f;  // 0 = normal, 1 = fully white
    [SerializeField] private float blink_speed = 2f;            // cycles per second
    [SerializeField] private float blink_min_alpha = 0.3f;      // Minimum alpha during blink (0.0 = invisible, 1.0 = opaque)
    [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // Smooth easing

    [Header("Screen Shake")]
    [SerializeField] private bool shakeOnDamage = true;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.15f;
    [SerializeField] private float shakeFrequency = 25f;

    [Header("Screen Dimmer")]
    [SerializeField] private bool enableScreenDimmer = true;
    [SerializeField] private float dimMaxAlpha = 0.45f;
    [SerializeField] private float dimFrequencyMultiplier = 1f; // multiplier of blink_speed to control dim speed

    // Original color cache for each sprite
    private Color[] originalColors;
    private SpriteRenderer[] spriteRenderers;
    private KnockbackController knockbackController;
    private BloodTearsManager tearsManager; // Referencia al gestor de lágrimas

    private Rigidbody2D rb;

    [Header("UI - Velas")]
    public Animator[] velaAnimators;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        knockbackController = GetComponentInChildren<KnockbackController>();
        tearsManager = GetComponent<BloodTearsManager>();
        
        // Obtain SpriteRenderer from the child object (default: "Square")
        // First try to find the "Square" child, then fallback to all children
        Transform spriteChild = transform.Find("Square");
        if (spriteChild != null && spriteChild.GetComponent<SpriteRenderer>() != null)
        {
            spriteRenderers = new SpriteRenderer[] { spriteChild.GetComponent<SpriteRenderer>() };
            Debug.Log("[PlayerHealth] Found SpriteRenderer in 'Square' child.");
        }
        else
        {
            // Fallback: get all SpriteRenderers from this object and children
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length == 0)
            {
                Debug.LogWarning("[PlayerHealth] No SpriteRenderer found in Player or children. Blink effect will not work.", this);
            }
            else
            {
                Debug.Log($"[PlayerHealth] Found {spriteRenderers.Length} SpriteRenderer(s) in children (fallback).");
            }
        }
        
        // Cache original colors for all sprite renderers
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                originalColors[i] = spriteRenderers[i].color;
                Debug.Log($"[PlayerHealth] Cached original color for sprite {i}: {originalColors[i]}");
            }
        }
        
        for (int i = 0; i < velaAnimators.Length; i++)
        {
            if (velaAnimators[i] != null)
            {
                velaAnimators[i].Play("vidaon", 0, Random.Range(0f, 1f));
            }
        }

        // Ensure ScreenShake instance exists on a persistent GameObject
        if (shakeOnDamage && ScreenShake.Instance == null)
        {
            GameObject go = new GameObject("ScreenShake");
            go.AddComponent<ScreenShake>();
        }

        // Ensure ScreenDimmer exists if requested
        if (enableScreenDimmer && ScreenDimmer.Instance == null)
        {
            GameObject go = new GameObject("ScreenDimmer");
            go.AddComponent<ScreenDimmer>();
        }
    }

    // Versión simple (sin knockback externo)
    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0)
            return;

        currentHealth -= damage;
        Debug.Log("Player took damage! Health: " + currentHealth);

        // Apagar vela
        if (currentHealth >= 0 && currentHealth < velaAnimators.Length)
        {
            Animator vela = velaAnimators[currentHealth];
            if (vela != null && vela.gameObject != null)
            {
                vela.SetTrigger("Apagar");
            }
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    /// <summary>
    /// New unified damage entry that follows the requested TIMELINE:
    /// - Checks invincibility/knockback active -> ignore
    /// - Apply damage
    /// - If death -> trigger death WITHOUT knockback
    /// - Else -> apply knockback (from sourcePosition), start iFrames + blink
    /// Returns true if damage was applied, false if ignored.
    /// </summary>
    public bool ReceiveDamageFromSource(int damage, Vector2 sourcePosition, float knockbackStrength, float knockbackDuration, KnockbackOptions options = default)
    {
        // PRE-CHECKS: invincibility or already dead
        if (isInvincible || currentHealth <= 0)
            return false;

        // PRE-CHECK: knockback active -> ignore new damage (player is already being handled)
        if (knockbackController != null && knockbackController.IsKnockbackActive())
            return false;

        // Si el jugador está canalizando curación, interrumpirlo
        if (tearsManager != null && tearsManager.IsChanneling)
        {
            tearsManager.HandleHitDuringChannel();
        }

        // Validate source (avoid zero-distance which would give NaN direction)
        if (float.IsNaN(sourcePosition.x) || float.IsNaN(sourcePosition.y))
            return false;

        // APPLY DAMAGE
        currentHealth -= damage;
        Debug.Log($"Player took damage from source! Health: {currentHealth}");

        // Update UI candles safely
        if (currentHealth >= 0 && currentHealth < velaAnimators.Length)
        {
            Animator vela = velaAnimators[currentHealth];
            if (vela != null && vela.gameObject != null)
            {
                vela.SetTrigger("Apagar");
            }
        }

        // If died: trigger death and DO NOT apply knockback
        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
            return true;
        }

        // APPLY KNOCKBACK (compute direction from source -> player)
        if (knockbackController != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - sourcePosition).normalized;
            if (knockbackDir != Vector2.zero)
            {
                knockbackController.ApplyHitKnockback(knockbackDir, knockbackStrength, knockbackDuration, options);
            }
        }

        // Start invincibility + progressive blink
        StartCoroutine(InvincibilityCoroutine());

        // Trigger screen shake if configured and alive
        if (shakeOnDamage && ScreenShake.Instance != null)
        {
            ScreenShake.Instance.Shake(shakeDuration, shakeMagnitude, shakeFrequency);
        }

        // Trigger screen dimmer synchronized with blink
        if (enableScreenDimmer && ScreenDimmer.Instance != null)
        {
            float dimFreq = Mathf.Max(0.1f, blink_speed * dimFrequencyMultiplier);
            ScreenDimmer.Instance.Flash(invincibility_time, dimMaxAlpha, dimFreq);
        }

        return true;
    }

    /// <summary>
    /// Cura al jugador una cantidad de vida y actualiza la UI.
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth || amount <= 0) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // "Encender" las velas correspondientes
        for (int i = oldHealth; i < currentHealth; i++)
        {
            if (i < velaAnimators.Length && velaAnimators[i] != null)
            {
                // Usamos un trigger o simplemente volvemos a poner el estado "vidaon"
                velaAnimators[i].Play("vidaon", 0, Random.Range(0f, 1f));
            }
        }
        Debug.Log($"Player se ha curado {amount} de vida. Vida actual: {currentHealth}");
    }

    private IEnumerator DieWithDelay()
    {
        Debug.Log("Player murió!");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        // Progressive blink: interpolate alpha (transparency) for visible effect
        float elapsedTime = 0f;
        
        while (elapsedTime < invincibility_time)
        {
            // Compute sine-wave brightness oscillation
            // Using sin for smooth, continuous cycling
            float cycleTime = elapsedTime * blink_speed;  // How many cycles have elapsed
            float sinValue = Mathf.Sin(cycleTime * Mathf.PI * 2f);  // Range [-1, 1]
            float blinkFactor = (sinValue + 1f) * 0.5f;  // Normalize to [0, 1]
            
            // Interpolate alpha from blink_min_alpha to 1.0 (fully opaque)
            float targetAlpha = Mathf.Lerp(blink_min_alpha, 1f, blinkFactor);
            
            // Apply to all sprite renderers
            if (spriteRenderers != null && spriteRenderers.Length > 0)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null && i < originalColors.Length)
                    {
                        // Keep original color, only modify alpha
                        Color newColor = originalColors[i];
                        newColor.a = targetAlpha;
                        spriteRenderers[i].color = newColor;
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;  // Wait for next frame for smooth animation
        }
        
        // Restore original colors immediately after invincibility ends
        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null && i < originalColors.Length)
                {
                    spriteRenderers[i].color = originalColors[i];
                    Debug.Log($"[PlayerHealth] Blink ended - Sprite {i} restored to original alpha: {originalColors[i].a}");
                }
            }
        }
        
        isInvincible = false;
        Debug.Log("[PlayerHealth] Invincibility ended.");
    }
}
