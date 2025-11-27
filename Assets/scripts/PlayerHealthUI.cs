using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Prefabs y UI")]
    public GameObject candlePrefab;             // Prefab de la vela
    public Transform healthContainer;           // Contenedor en UI para las velas

    private PlayerHealth playerHealth;
    private List<Animator> candleAnimators = new List<Animator>();

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth no encontrado en el mismo GameObject.");
            return;
        }

        GenerateCandles(playerHealth.maxHealth);
    }

    public void GenerateCandles(int amount)
    {
        // Limpiar las velas anteriores
        foreach (Transform child in healthContainer)
        {
            Destroy(child.gameObject);
        }

        candleAnimators.Clear();

        // Crear nuevas velas
        for (int i = 0; i < amount; i++)
        {
            GameObject candle = Instantiate(candlePrefab, healthContainer);
            Animator anim = candle.GetComponent<Animator>();

            if (anim != null)
            {
                anim.Play("vidaon", 0, Random.Range(0f, 1f)); // Desincronización
                candleAnimators.Add(anim);
            }
        }

        // Sincronizar con PlayerHealth
        playerHealth.velaAnimators = candleAnimators.ToArray();
    }

    public void UpdateCandleVisuals(int currentHealth)
    {
        for (int i = 0; i < candleAnimators.Count; i++)
        {
            if (candleAnimators[i] == null) continue;

            if (i < currentHealth)
            {
                candleAnimators[i].Play("vidaon");
            }
            else
            {
                candleAnimators[i].SetTrigger("Apagar");
            }
        }
    }
}
