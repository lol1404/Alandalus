using UnityEngine;

/// <summary>
/// Un objeto recolectable que otorga una cantidad fija de Lágrimas de Sangre al jugador.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TearPickup : MonoBehaviour
{
    [SerializeField] private float tearAmount = 25f; // Cantidad de lágrimas que otorga

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BloodTearsManager tearsManager = other.GetComponent<BloodTearsManager>();
            if (tearsManager != null)
            {
                tearsManager.AddTears(tearAmount);
                Destroy(gameObject); // Destruir el objeto una vez recogido
            }
        }
    }
}