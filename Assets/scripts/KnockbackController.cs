using UnityEngine;

/// <summary>
/// Standalone Knockback Controller for Unity 2D/3D
/// 
/// DESIGN PRINCIPLES:
/// - Decoupled from damage: knockback is orthogonal to health/damage systems
/// - Immediate application: knockback is applied at hit detection, not in damage callbacks
/// - Fresh direction per hit: direction is computed/stored only for the lifetime of one knockback event
/// - Explicit state reset: direction and velocity are reset every time a new knockback is applied
/// 
/// USAGE:
/// 1. Add this component to any character that can be knocked back
/// 2. In your hit/collision handler, call: knockbackController.ApplyHitKnockback(direction, strength, duration, options)
/// 3. Call this BEFORE applying damage (knockback is independent)
/// 4. Movement system checks knockbackController.IsKnockbackActive() to suppress player input during knockback
/// 
/// EXAMPLE:
///     void OnEnemyHit(Collider2D enemy, Vector2 hitPos)
///     {
///         Vector2 knockbackDir = (transform.position - hitPos).normalized;
///         knockbackController.ApplyHitKnockback(knockbackDir, strength: 8f, duration: 0.25f, options: default);
///         
///         // Apply damage separately (can be 0)
///         health.TakeDamage(5);
///     }
/// </summary>
public class KnockbackController : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float defaultKnockbackDuration = 0.15f;
    [SerializeField] private float defaultKnockbackStrength = 5f;
    [SerializeField] private bool useGravity = true;  // If false, knockback ignores gravity
    [SerializeField] private bool drawDebugGizmo = false;
    [SerializeField] private Color debugGizmoColor = Color.yellow;

    [Header("Physics References")]
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Rigidbody rb3D;
    [SerializeField] private CharacterController characterController;

    // Knockback state (fresh per hit)
    private Vector2 knockbackDirection = Vector2.zero;
    private float knockbackStrength = 0f;
    private float knockbackTimeRemaining = 0f;
    private float knockbackTotalDuration = 0f;
    private bool isKnockbackActive = false;
    private KnockbackOptions currentOptions = default;

    // Safety cache for velocity backup (for CharacterController pattern)
    private Vector3 externalVelocity = Vector3.zero;

    private void Awake()
    {
        // Auto-detect physics component
        if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();
        if (rb3D == null) rb3D = GetComponent<Rigidbody>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // Validate at least one physics system is present
        if (rb2D == null && rb3D == null && characterController == null)
        {
            Debug.LogWarning($"[KnockbackController] {gameObject.name} has no Rigidbody2D, Rigidbody, or CharacterController. Knockback will not work.", this);
        }
    }

    private void FixedUpdate()
    {
        if (!isKnockbackActive) return;

        // Decrement timer
        knockbackTimeRemaining -= Time.fixedDeltaTime;

        if (knockbackTimeRemaining <= 0f)
        {
            // Knockback duration expired
            CancelKnockback();
            return;
        }

        // Optional: Compute easing factor (linear decay or custom curve)
        float easeFactor = 1f;
        if (knockbackTotalDuration > 0f)
        {
            easeFactor = Mathf.Clamp01(knockbackTimeRemaining / knockbackTotalDuration);
        }

        // Apply continuous knockback (impulse + optional decay)
        ApplyKnockbackPhysics(easeFactor);
    }

    /// <summary>
    /// Apply knockback immediately upon hit detection.
    /// Call this BEFORE applying damage.
    /// 
    /// CRITICAL LOGIC:
    /// - If knockback is active and allowOverride is false, ignore the new hit
    /// - ALWAYS reset direction, timer, and state when applying a new knockback
    /// - Never read from "previous knockback direction" for new hits
    /// </summary>
    public void ApplyHitKnockback(
        Vector2 direction,
        float strength,
        float duration = -1f,
        KnockbackOptions options = default)
    {
        // Guard against zero-vector direction
        if (direction == Vector2.zero)
        {
            Debug.LogWarning($"[KnockbackController] ApplyHitKnockback called with zero direction on {gameObject.name}. Ignoring.", this);
            return;
        }

        // Check if we should override active knockback
        if (isKnockbackActive && !options.allowOverride)
        {
            // Knockback already active and override disabled: ignore this hit
            return;
        }

        // Use defaults if not provided
        if (duration < 0f) duration = defaultKnockbackDuration;
        if (strength < 0f) strength = defaultKnockbackStrength;

        // RESET all state explicitly (this prevents direction "stickiness")
        knockbackDirection = direction.normalized;
        knockbackStrength = strength;
        knockbackTotalDuration = duration;
        knockbackTimeRemaining = duration;
        isKnockbackActive = true;
        currentOptions = options;

        // Apply initial impulse
        ApplyKnockbackPhysics(easeFactor: 1f);

        // Debug logging
        if (drawDebugGizmo)
        {
            Debug.Log($"[KnockbackController] {gameObject.name} hit knockback applied: dir={knockbackDirection}, str={knockbackStrength}, dur={duration}");
        }
    }

    /// <summary>
    /// Cancel any active knockback immediately.
    /// </summary>
    public void CancelKnockback()
    {
        if (!isKnockbackActive) return;

        isKnockbackActive = false;
        knockbackDirection = Vector2.zero;
        knockbackStrength = 0f;
        knockbackTimeRemaining = 0f;
        knockbackTotalDuration = 0f;
        currentOptions = default;
    }

    /// <summary>
    /// Check if knockback is currently active.
    /// </summary>
    public bool IsKnockbackActive()
    {
        return isKnockbackActive;
    }

    /// <summary>
    /// Get the current knockback direction (for debugging/UI).
    /// </summary>
    public Vector2 GetCurrentKnockbackDirection()
    {
        return knockbackDirection;
    }

    /// <summary>
    /// Get remaining knockback time.
    /// </summary>
    public float GetKnockbackTimeRemaining()
    {
        return knockbackTimeRemaining;
    }

    /// <summary>
    /// Internal: Apply knockback impulse to the appropriate physics system.
    /// Supports Rigidbody2D, Rigidbody, and CharacterController patterns.
    /// </summary>
    private void ApplyKnockbackPhysics(float easeFactor)
    {
        Vector3 knockbackImpulse = new Vector3(knockbackDirection.x, knockbackDirection.y, 0f) * knockbackStrength * easeFactor;

        if (rb2D != null)
        {
            // Rigidbody2D: Clear velocity and apply impulse
            rb2D.linearVelocity = knockbackImpulse;
        }
        else if (rb3D != null)
        {
            // Rigidbody (3D): Set velocity directly (you may prefer AddForce for physics-based)
            Vector3 currentVel = rb3D.linearVelocity;
            // Option 1: Replace velocity entirely
            rb3D.linearVelocity = new Vector3(knockbackImpulse.x, useGravity ? currentVel.y : knockbackImpulse.y, knockbackImpulse.z);
            // Option 2: Use AddForce (requires tuning mass)
            // rb3D.AddForce(knockbackImpulse * (1f / Time.fixedDeltaTime), ForceMode.VelocityChange);
        }
        else if (characterController != null)
        {
            // CharacterController: Add knockback to external velocity vector
            externalVelocity = knockbackImpulse;
            // The character controller movement script should integrate externalVelocity each frame
            // Example: characterController.Move(externalVelocity * Time.deltaTime);
        }
    }

    /// <summary>
    /// For CharacterController users: Call this each FixedUpdate to get the external knockback velocity.
    /// Integrate it into your character controller movement: controller.Move(GetExternalVelocity() * Time.deltaTime);
    /// </summary>
    public Vector3 GetExternalKnockbackVelocity()
    {
        return externalVelocity;
    }

    /// <summary>
    /// Debug visualization: Draw knockback vector as a gizmo.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmo || !isKnockbackActive) return;

        Vector3 endPoint = transform.position + new Vector3(knockbackDirection.x, knockbackDirection.y, 0f) * knockbackStrength;
        Gizmos.color = debugGizmoColor;
        Gizmos.DrawRay(transform.position, new Vector3(knockbackDirection.x, knockbackDirection.y, 0f) * knockbackStrength);
        Gizmos.DrawWireSphere(endPoint, 0.2f);

        // Draw timer indicator
        Gizmos.color = Color.white;
        float timerAlpha = knockbackTimeRemaining / knockbackTotalDuration;
        Gizmos.color = new Color(1f, 1f, 1f, timerAlpha);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, Vector3.one * 0.3f);
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Apply Knockback Right")]
    private void TestKnockbackRight()
    {
        ApplyHitKnockback(Vector2.right, strength: 5f, duration: 0.2f, options: default);
    }

    [ContextMenu("Test: Apply Knockback Left")]
    private void TestKnockbackLeft()
    {
        ApplyHitKnockback(Vector2.left, strength: 5f, duration: 0.2f, options: default);
    }

    [ContextMenu("Test: Cancel Knockback")]
    private void TestCancelKnockback()
    {
        CancelKnockback();
    }
#endif
}

/// <summary>
/// Options struct for fine-tuning knockback behavior.
/// </summary>
public struct KnockbackOptions
{
    /// <summary>
    /// If true, new hits override active knockback.
    /// If false, new hits are ignored while knockback is active (unless cancelled).
    /// Default: true (responsive, new hits always register)
    /// </summary>
    public bool allowOverride;

    /// <summary>
    /// If true, gravity still affects the character during knockback (Y velocity decays).
    /// If false, knockback is pure impulse in the given direction (ignores gravity).
    /// Default: true
    /// </summary>
    public bool applyGravity;

    /// <summary>
    /// Optional easing curve for knockback decay.
    /// Leave null for linear decay.
    /// </summary>
    public AnimationCurve easingCurve;

    /// <summary>
    /// If true, knockback is suppressed if the character is in an invulnerability window.
    /// Default: false (knockback applies regardless of invincibility)
    /// </summary>
    public bool respectsInvincibility;

    /// <summary>
    /// Create knockback options with custom values.
    /// </summary>
    public KnockbackOptions(bool allowOverride = true, bool applyGravity = true, bool respectsInvincibility = false)
    {
        this.allowOverride = allowOverride;
        this.applyGravity = applyGravity;
        this.respectsInvincibility = respectsInvincibility;
        this.easingCurve = null;
    }
}
