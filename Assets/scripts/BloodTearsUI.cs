using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Actualiza la barra/frasco visual de las Lágrimas de Sangre.
/// Asignar a un objeto Image de la UI con el tipo de imagen "Filled".
/// </summary>
[RequireComponent(typeof(Image))]
public class BloodTearsUI : MonoBehaviour
{
    private Image tearVesselImage;
    private BloodTearsManager tearsManager;

    void Start()
    {
        tearVesselImage = GetComponent<Image>();
        if (tearVesselImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("[BloodTearsUI] El tipo de imagen debería ser 'Filled' para un efecto de llenado.", this);
        }

        // Buscar el manager en la escena y suscribirse a su evento
        tearsManager = FindObjectOfType<BloodTearsManager>();
        if (tearsManager != null)
        {
            tearsManager.OnTearsChanged += UpdateTearsVisual;
        }
        else
        {
            Debug.LogError("[BloodTearsUI] No se encontró un BloodTearsManager en la escena.");
        }
    }

    private void OnDestroy()
    {
        if (tearsManager != null)
            tearsManager.OnTearsChanged -= UpdateTearsVisual;
    }

    private void UpdateTearsVisual(float current, float max)
    {
        if (max > 0)
        {
            tearVesselImage.fillAmount = current / max;
        }
    }
}