using UnityEngine;
using UnityEngine.UI;

public class AttackCooldownUI : MonoBehaviour
{
    [Header("Referencias")]
    private PlayerAttack playerAttack;
    private Image cooldownBar;  // Image que se llena/vacía

    [Header("Configuración")]
    public Color cooldownActiveColor = new Color(1f, 0.5f, 0f);  // Naranja cuando está en cooldown
    public Color cooldownReadyColor = new Color(0f, 1f, 0f);     // Verde cuando está listo
    public bool showOnlyWhenCooling = true;  // Solo mostrar la barra cuando hay cooldown

    private CanvasGroup canvasGroup;  // Para fade in/out

    void Start()
    {
        playerAttack = FindObjectOfType<PlayerAttack>();
        cooldownBar = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (playerAttack == null)
            Debug.LogError("[AttackCooldownUI] No se encontró PlayerAttack en la escena");
        
        if (cooldownBar == null)
            Debug.LogError("[AttackCooldownUI] Este GameObject debe tener un componente Image");
    }

    void Update()
    {
        if (playerAttack == null || cooldownBar == null) return;

        // Calcular progreso del cooldown (0 a 1, donde 1 = cooldown completo)
        float cooldownProgress = GetCooldownProgress();

        // Actualizar la barra: escala horizontal (0 = vacía, 1 = llena)
        if (cooldownBar.rectTransform != null)
            cooldownBar.rectTransform.localScale = new Vector3(cooldownProgress, 1f, 1f);

        // Cambiar color según estado
        if (playerAttack.IsAttackOnCooldown)
        {
            cooldownBar.color = cooldownActiveColor;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }
        else
        {
            cooldownBar.color = cooldownReadyColor;
            if (canvasGroup != null && showOnlyWhenCooling) canvasGroup.alpha = 0.3f;
        }
    }

    float GetCooldownProgress()
    {
        // Protección contra null reference
        if (playerAttack == null) return 0f;
        
        // Usa el método de PlayerAttack para obtener el progreso real (0 a 1)
        return playerAttack.GetCooldownProgress();
    }
}
