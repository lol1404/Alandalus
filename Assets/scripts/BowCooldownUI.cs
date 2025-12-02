using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra el cooldown del arco en una barra de UI.
/// </summary>
public class BowCooldownUI : MonoBehaviour
{
    [Header("Referencias")]
    private PlayerBowAttack playerBowAttack;
    private Image cooldownBar;

    [Header("Configuración")]
    public Color cooldownActiveColor = new Color(0.2f, 0.8f, 1f); // Azul claro
    public Color cooldownReadyColor = Color.white;

    void Start()
    {
        playerBowAttack = FindObjectOfType<PlayerBowAttack>();
        cooldownBar = GetComponent<Image>();

        if (playerBowAttack == null)
            Debug.LogError("[BowCooldownUI] No se encontró PlayerBowAttack en la escena.");

        if (cooldownBar == null)
            Debug.LogError("[BowCooldownUI] Este GameObject debe tener un componente Image.");
    }

    void Update()
    {
        if (playerBowAttack == null || cooldownBar == null) return;

        float progress = playerBowAttack.CooldownProgress;
        cooldownBar.fillAmount = progress;

        cooldownBar.color = (progress > 0) ? cooldownActiveColor : cooldownReadyColor;
    }
}