# Knockback System: Quick Start Implementation

## 30-MINUTE INTEGRATION FOR YOUR CURRENT PROJECT

This guide walks you through integrating the new KnockbackController into your existing game in under 30 minutes.

---

## STEP 1: Add KnockbackController to Player (3 minutes)

1. **Locate your player prefab** at: `Assets/prefabs/` or in your scene hierarchy
2. **Create a new empty GameObject** as a child of the player, name it `KnockbackSystem`
3. **Attach KnockbackController.cs** (provided in this folder):
   - Select `KnockbackSystem` → Inspector → "Add Component" → KnockbackController
4. **Configure in Inspector**:
   - `Default Knockback Duration`: 0.15
   - `Default Knockback Strength`: 5.0
   - `Use Gravity`: true
   - `Draw Debug Gizmo`: true (for development only)
5. **Save prefab**

**Result**: Player now has knockback capabilities.

---

## STEP 2: Update EnemyDamage.cs (5 minutes)

Replace the entire `OnTriggerEnter2D` method in `Assets/scripts/EnemyDamage.cs`:

**OLD CODE (delete this):**
```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            playerHealth.TakeDamage(damage, knockbackDir, knockbackForce);
        }
    }
}
```

**NEW CODE (replace with this):**
```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        KnockbackController knockback = other.GetComponentInChildren<KnockbackController>();
        
        if (playerHealth != null)
        {
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            
            // IMMEDIATE KNOCKBACK (not waiting for damage callback)
            if (knockback != null)
            {
                knockback.ApplyHitKnockback(
                    direction: knockbackDir,
                    strength: knockbackForce,
                    duration: 0.15f,
                    options: default
                );
            }
            
            // DAMAGE (separate operation)
            playerHealth.TakeDamage(damage);
        }
    }
}
```

**Save file**.

---

## STEP 3: Update WeaponAttack.cs (5 minutes)

In `Assets/scripts/WeaponAttack.cs`, find the `Attack()` method and update the enemy knockback section:

**OLD CODE (find and replace this section):**
```csharp
foreach (var enemy in hitEnemies)
{
    Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
    
    enemy.GetComponent<EnemyHealth>()?.TakeDamage(currentWeapon.damage);

    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(knockbackDir * currentWeapon.knockbackForce, ForceMode2D.Impulse);
    }
}
```

**NEW CODE (replace with this):**
```csharp
foreach (var enemy in hitEnemies)
{
    Vector2 knockbackDir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
    
    // Knockback to enemies (immediate)
    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(knockbackDir * currentWeapon.knockbackForce, ForceMode2D.Impulse);
    }
    
    // Damage (separate)
    enemy.GetComponent<EnemyHealth>()?.TakeDamage(currentWeapon.damage);
}
```

(This part stays largely the same, but we're adding recoil to the player next)

**Also add recoil to the player.** Find the line:
```csharp
if (playerHealth != null && currentWeapon != null && currentWeapon.playerAttackKnockbackForce > 0f)
{
    Vector2 attackKnockbackDir = -attackDir;
    playerHealth.ApplyAttackKnockback(
        attackKnockbackDir,
        currentWeapon.playerAttackKnockbackForce,
        currentWeapon.playerAttackKnockbackDuration
    );
}
```

**Replace with:**
```csharp
if (currentWeapon != null && currentWeapon.playerAttackKnockbackForce > 0f)
{
    KnockbackController playerKnockback = GetComponentInChildren<KnockbackController>();
    if (playerKnockback != null)
    {
        Vector2 recoilDir = -attackDir;
        playerKnockback.ApplyHitKnockback(
            direction: recoilDir,
            strength: currentWeapon.playerAttackKnockbackForce,
            duration: currentWeapon.playerAttackKnockbackDuration,
            options: default
        );
    }
}
```

**Save file**.

---

## STEP 4: Update PlayerMovement.cs (5 minutes)

In `Assets/scripts/movement.cs`, update the `FixedUpdate()` method:

**OLD CODE (find this):**
```csharp
void FixedUpdate()
{
    // If the player is being knocked back (from damage or attack), skip movement
    if (playerHealth != null && playerHealth.IsAnyKnockback)
    {
        return;
    }
    // ... rest of method ...
}
```

**NEW CODE (replace with this):**
```csharp
private KnockbackController knockbackController;

void Start()
{
    rb = GetComponent<Rigidbody2D>();
    playerHealth = GetComponent<PlayerHealth>();
    playerAttack = GetComponent<PlayerAttack>();
    knockbackController = GetComponentInChildren<KnockbackController>();  // ADD THIS LINE
    
    Transform spriteChild = transform.Find("Square");
    animator = spriteChild.GetComponent<Animator>();
    spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
}

void FixedUpdate()
{
    // If the player is being knocked back, skip movement
    if (knockbackController != null && knockbackController.IsKnockbackActive())
    {
        return;
    }
    
    // If the player is attack frozen (has attacked, but not knocked back), skip movement logic
    if (playerAttack != null && playerAttack.IsAttackFreezing)
    {
        return;
    }

    if (isDashing)
    {
        rb.MovePosition(rb.position + lastMoveDirection * dashSpeed * Time.fixedDeltaTime);
        return;
    }

    currentVelocity = Vector2.Lerp(currentVelocity, movementInput * moveSpeed, acceleration * Time.fixedDeltaTime);
    rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

    float speedForAnimation = Mathf.Clamp(currentVelocity.magnitude, 0f, 1.5f);
    animator.SetFloat("Speed", speedForAnimation);

    if (movementInput.x > 0.01f) spriteRenderer.flipX = false;
    else if (movementInput.x < -0.01f) spriteRenderer.flipX = true;
}
```

**Save file**.

---

## STEP 5: Simplify PlayerHealth.cs (5 minutes)

In `Assets/scripts/PlayerHealth.cs`, delete the old knockback methods and variables.

**DELETE these lines (approximately lines 23-26, 92-119):**
```csharp
private float damageKnockbackTimer = 0f;
private float attackKnockbackTimer = 0f;
private Vector2 currentDamageKnockbackDir = Vector2.zero;
private Vector2 currentAttackKnockbackDir = Vector2.zero;
private float currentAttackKnockbackForce = 0f;
private float currentDamageKnockbackForce = 0f;
public bool IsBeingDamageKnockedBack => damageKnockbackTimer > 0f;
public bool IsBeingAttackKnockedBack => attackKnockbackTimer > 0f;
public bool IsAnyKnockback => IsBeingDamageKnockedBack || IsBeingAttackKnockedBack;
public void ApplyDamageKnockback(...) { }
public void ApplyAttackKnockback(...) { }
void Update() { /* knockback timer */ }
```

**Simplify TakeDamage() to:**
```csharp
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
```

**Save file**.

---

## STEP 6: Test (2 minutes)

1. **Enter Play Mode**
2. **Attack an enemy**: Enemy should be pushed immediately ✓
3. **Get hit by enemy**: You should be pushed immediately, health decreases ✓
4. **Look at scene view**: Yellow debug lines show knockback vectors ✓
5. **Enable gizmo visualization**: In KnockbackSystem Inspector, toggle `Draw Debug Gizmo` on/off

---

## STEP 7: Tune (5 minutes)

Adjust for feel:

1. **KnockbackController** (on player):
   - `Default Knockback Strength`: Increase for more dramatic knockback (try 8-10)
   - `Default Knockback Duration`: Increase for longer frozen time (try 0.2-0.3)

2. **EnemyDamage** (on enemies):
   - `knockbackForce`: Increase for stronger enemy knockback (try 10-15)

3. **WeaponAttack** weapon data:
   - `knockbackForce`: Increase for stronger player attack knockback
   - `playerAttackKnockbackForce`: Adjust recoil strength

**Tip**: Start with default values and increase/decrease based on feel during playtest.

---

## VERIFICATION CHECKLIST

- [ ] Player has KnockbackSystem child GameObject
- [ ] KnockbackSystem has KnockbackController component
- [ ] EnemyDamage calls `knockbackController.ApplyHitKnockback()` before `TakeDamage()`
- [ ] WeaponAttack calls knockback on player after hitting enemies
- [ ] PlayerMovement checks `knockbackController.IsKnockbackActive()` in FixedUpdate
- [ ] PlayerHealth has simplified TakeDamage (no old knockback code)
- [ ] Game runs without errors
- [ ] Attack hits enemy → enemy pushed immediately ✓
- [ ] Enemy hits you → you pushed immediately ✓
- [ ] Hitting same enemy multiple times → direction changes per hit (no sticking) ✓
- [ ] Debug gizmo shows yellow knockback vectors ✓

---

## TROUBLESHOOTING

| Issue | Solution |
|-------|----------|
| Knockback not working | Verify `Draw Debug Gizmo` is on. Check Console for errors. Ensure KnockbackController is on player. |
| Character doesn't move when hit | Verify Rigidbody2D is not set to kinematic. Check movement script is suppressing input during knockback. |
| Knockback direction "stuck" on old direction | Verify you replaced the old `ApplyDamageKnockback()` call in EnemyDamage.cs. |
| Enemy doesn't move when hit | Enemy should have Rigidbody2D. Direct Rigidbody2D.AddForce is used for enemies (not KnockbackController). |
| Knockback feels too weak/strong | Adjust `Default Knockback Strength` on KnockbackController or `knockbackForce` in EnemyDamage. |
| Player dashes during knockback | Check `PlayerMovement.FixedUpdate()` to ensure knockback check comes before dash logic. |

---

## FILES MODIFIED SUMMARY

| File | Changes |
|------|---------|
| `PlayerMovement.cs` | Added knockbackController check in FixedUpdate |
| `EnemyDamage.cs` | Call knockbackController.ApplyHitKnockback() before TakeDamage() |
| `WeaponAttack.cs` | Call knockbackController.ApplyHitKnockback() on player after attack |
| `PlayerHealth.cs` | Removed old knockback methods/variables, simplified TakeDamage() |
| `KnockbackController.cs` | NEW FILE - add to player prefab |

---

## NEXT STEPS

1. **Integrate in your game** (follow steps 1-7 above)
2. **Playtest** and tune knockback strength/duration for feel
3. **Optional**: Enable `Draw Debug Gizmo` to visualize knockback vectors
4. **Optional**: Adjust `Knockback Options` struct for advanced features (gravity, ease curves)
5. **Optional**: Add sound effects on hit for better feedback

---

## EXPECTED RESULTS AFTER INTEGRATION

✓ **Immediate Knockback**: Character pushed on hit, no delay  
✓ **Fresh Direction**: Each hit direction updates correctly, no sticking  
✓ **Independent System**: Knockback works without damage, zero-damage hits knock back  
✓ **Responsive Feel**: Game feels more responsive to hits  
✓ **Clean Codebase**: Knockback decoupled from health system  

---

**Total Time: ~30 minutes**

If stuck, check the provided documentation files for detailed explanations of each system.

