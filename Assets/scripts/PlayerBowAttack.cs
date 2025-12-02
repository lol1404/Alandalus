using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Gestiona el disparo del arco, el consumo de recursos y el cooldown.
/// Lee la configuración desde un ScriptableObject BowData.
/// </summary>
public class PlayerBowAttack : MonoBehaviour
{
    [Header("Configuración del Arco")]
    [Tooltip("Asigna aquí el ScriptableObject del arco que usará el jugador.")]
    public BowData currentBow;

    [Header("Referencias")]
    [Tooltip("Punto desde donde se dispararán las flechas.")]
    public Transform firePoint;
    [Tooltip("Animator del jugador para activar la animación de disparo.")]
    public Animator playerAnimator;

    // Referencias a otros sistemas
    private BloodTearsManager tearsManager;
    private InputSystem_Actions controls;
    private Rigidbody2D playerRb; // Referencia al Rigidbody del jugador

    // Estado interno
    private float cooldownTimer = 0f;

    public float CooldownProgress => (currentBow != null && currentBow.cooldown > 0) ? Mathf.Clamp01(cooldownTimer / currentBow.cooldown) : 0f;

    void Awake()
    {
        tearsManager = GetComponent<BloodTearsManager>();
        controls = new InputSystem_Actions();
        playerRb = GetComponent<Rigidbody2D>(); // Obtenemos la referencia

        // Registrar eventos de input
        controls.Player.ShootBow.performed += _ => TryShoot();
    }

    void Start()
    {
        if (currentBow == null)
        {
            Debug.LogError("[PlayerBowAttack] No se ha asignado un BowData. El sistema de arco no funcionará.", this);
            this.enabled = false;
        }
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    void Update()
    {
        // Actualizar cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void TryShoot()
    {
        // Comprobaciones para poder disparar
        if (cooldownTimer > 0 || currentBow == null) return;

        // Comprobar si hay suficientes lágrimas
        if (!tearsManager.SpendTears(currentBow.tearCost * currentBow.tearCostModifier))
        {
            Debug.Log("[PlayerBowAttack] No hay suficientes Lágrimas de Sangre para disparar.");
            // Aquí podrías añadir un sonido de "fallo"
            return;
        }

        // Si todo es correcto, disparar.
        Shoot();
    }

    private void Shoot()
    {
        cooldownTimer = currentBow.cooldown * currentBow.cooldownModifier;

        Debug.Log("[PlayerBowAttack] ¡Flecha disparada!");

        // Activar animación de disparo y desactivar la de carga
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsDrawingBow", false);
            playerAnimator.SetTrigger("ShootArrow"); // Trigger para la animación de soltar
        }

        // Sonido de disparo
        if (currentBow.shootSound != null)
        {
            AudioSource.PlayClipAtPoint(currentBow.shootSound, transform.position);
        }

        // Instanciar la flecha
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject arrow = Instantiate(currentBow.arrowPrefab, firePoint.position, Quaternion.identity); // Usamos Quaternion.identity por la auto-orientación
        arrow.GetComponent<ArrowProjectile>()?.Initialize(currentBow, direction, playerRb.linearVelocity); // Pasamos la velocidad del jugador
    }
}