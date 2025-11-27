using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    public float DashCooldownTimeLeft { get; private set; } = 0f;
    public string dashAnimationName = "IsDashing";  // Nombre del parámetro bool en el Animator

    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private Vector2 lastMoveDirection;

    private Rigidbody2D rb;
    private InputSystem_Actions controls;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isDashing = false;
    private bool canDash = true;

    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private KnockbackController knockbackController;

    [Header("Audio")]
    public AudioClip[] dashSounds;
    public AudioSource audioSource;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx =>
        {
            if (!isDashing)
            {
                movementInput = ctx.ReadValue<Vector2>();
                if (movementInput != Vector2.zero)
                    lastMoveDirection = movementInput.normalized;
            }
        };

        controls.Player.Move.canceled += ctx =>
        {
            if (!isDashing)
                movementInput = Vector2.zero;
        };

        controls.Player.Dash.performed += ctx => TryDash();
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        knockbackController = GetComponentInChildren<KnockbackController>();
        Transform spriteChild = transform.Find("Square");
        animator = spriteChild.GetComponent<Animator>();
        spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (DashCooldownTimeLeft > 0)
            DashCooldownTimeLeft -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Si el jugador está siendo golpeado hacia atrás, salta el movimiento
        if (knockbackController != null && knockbackController.IsKnockbackActive())
        {
            return;
        }
        // Si el jugador está congelado por ataque (ha atacado, pero no golpeado hacia atrás), salta la lógica de movimiento
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

    void TryDash()
    {
        if (canDash && DashCooldownTimeLeft <= 0f && lastMoveDirection != Vector2.zero)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        DashCooldownTimeLeft = dashCooldown;

        // 🎵 Reproducir sonido aleatorio
        if (dashSounds.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, dashSounds.Length);
            audioSource.PlayOneShot(dashSounds[index]);
        }

        // 🎬 Activar animación del dash
        if (animator != null)
        {
            animator.SetBool(dashAnimationName, true);
        }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.MovePosition(rb.position + lastMoveDirection * dashSpeed * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 🎬 Desactivar animación del dash
        if (animator != null)
        {
            animator.SetBool(dashAnimationName, false);
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
