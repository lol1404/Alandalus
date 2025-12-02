using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Gestiona el recurso "Lágrimas de Sangre", su obtención y su uso para curación canalizada.
/// Debe estar en el objeto del Jugador.
/// </summary>
public class BloodTearsManager : MonoBehaviour
{
    [Header("Estado del Recurso")]
    [SerializeField] private float maxTears = 100f;
    [SerializeField] private float currentTears;

    [Header("Configuración de Curación")]
    [SerializeField] private float tearsPerHeal = 20f; // Coste para 1 corazón (100 max / 5 curaciones)
    [SerializeField] private float healChannelTime = 1.5f; // Segundos para canalizar una curación
    [SerializeField] private int heartsToHeal = 1; // Corazones a curar por ciclo

    // Estado de la canalización
    private bool isChannelingHeal = false;
    private Coroutine healCoroutine;

    // Referencias a otros componentes del jugador
    private PlayerHealth playerHealth;
    private InputSystem_Actions controls;

    // Evento para notificar a la UI que las lágrimas han cambiado
    public System.Action<float, float> OnTearsChanged;

    public bool IsChanneling => isChannelingHeal;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        controls = new InputSystem_Actions();

        // Input para curación
        controls.Player.Heal.performed += _ => StartHealChannel();
        controls.Player.Heal.canceled += _ => StopHealChannel();
    }

    void Start()
    {
        currentTears = 0; // Empezar con 0 lágrimas
        OnTearsChanged?.Invoke(currentTears, maxTears);
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    /// <summary>
    /// Añade una cantidad de lágrimas, sin exceder el máximo.
    /// </summary>
    public void AddTears(float amount)
    {
        if (amount <= 0) return;

        currentTears = Mathf.Min(currentTears + amount, maxTears);
        Debug.Log($"[Tears] Ganadas {amount}. Total: {currentTears}/{maxTears}");
        OnTearsChanged?.Invoke(currentTears, maxTears);
        // Aquí podrías disparar un evento para una animación de "ganar recurso" en la UI.
    }

    /// <summary>
    /// Gasta una cantidad de lágrimas. Devuelve true si se pudo gastar, false si no.
    /// </summary>
    public bool SpendTears(float amount)
    {
        if (currentTears < amount)
        {
            Debug.LogWarning($"[Tears] Intento de gastar {amount} pero solo hay {currentTears}.");
            return false;
        }

        currentTears -= amount;
        Debug.Log($"[Tears] Gastadas {amount}. Restante: {currentTears}");
        OnTearsChanged?.Invoke(currentTears, maxTears);
        // Aquí podrías disparar un evento para una animación de "gastar recurso" en la UI.
        return true;
    }

    /// <summary>
    /// Inicia el proceso de curación si se cumplen las condiciones.
    /// </summary>
    public void StartHealChannel()
    {
        // No se puede curar si la vida está al máximo o no hay suficientes lágrimas
        if (playerHealth.currentHealth >= playerHealth.maxHealth)
        {
            Debug.Log("[Tears] Vida al máximo, no se puede curar.");
            return;
        }
        if (currentTears < tearsPerHeal)
        {
            Debug.Log($"[Tears] No hay suficientes lágrimas para iniciar curación (necesitas {tearsPerHeal}).");
            return;
        }

        isChannelingHeal = true;
        healCoroutine = StartCoroutine(HealChannelCoroutine());
        Debug.Log("[Tears] Iniciando canalización de curación...");
        // Aquí podrías disparar un evento para una animación de "canalizando" en la UI.
    }

    /// <summary>
    /// Detiene la canalización de curación (llamado al soltar el botón).
    /// </summary>
    public void StopHealChannel()
    {
        if (isChannelingHeal)
        {
            isChannelingHeal = false;
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
            }
            Debug.Log("[Tears] Canalización de curación detenida por el jugador.");
            // Aquí podrías disparar un evento para detener la animación de "canalizando".
        }
    }

    /// <summary>
    /// Maneja la interrupción de la canalización al recibir daño.
    /// </summary>
    public void HandleHitDuringChannel()
    {
        if (!isChannelingHeal) return;

        Debug.Log("[Tears] ¡Golpe recibido durante la canalización!");
        // Penalización: gastar la mitad de lágrimas y no curar
        SpendTears(tearsPerHeal / 2);
        StopHealChannel(); // Detener el proceso
    }

    private IEnumerator HealChannelCoroutine()
    {
        while (isChannelingHeal)
        {
            yield return new WaitForSeconds(healChannelTime);

            // Volver a comprobar si la canalización sigue activa y si hay vida que curar
            if (isChannelingHeal && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                // Si tenemos suficientes lágrimas para una curación completa
                if (SpendTears(tearsPerHeal))
                {
                    playerHealth.Heal(heartsToHeal);
                    Debug.Log($"[Tears] Curación completada. Vida: {playerHealth.currentHealth}");
                }
                else
                {
                    // No hay suficientes lágrimas, detener
                    Debug.Log("[Tears] No quedan suficientes lágrimas para continuar curando.");
                    StopHealChannel();
                }
            }
            else
            {
                // Si la vida ya está al máximo, detener
                if (playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    Debug.Log("[Tears] Vida al máximo, se detiene la curación.");
                    StopHealChannel();
                }
            }
        }
    }
}