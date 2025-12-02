using UnityEngine;

/// <summary>
/// ScriptableObject que contiene todos los atributos configurables para un tipo de arco.
/// Permite crear diferentes arcos desde el editor de Unity.
/// </summary>
[CreateAssetMenu(fileName = "NewBowData", menuName = "Weapons/Bow Data")]
public class BowData : ScriptableObject
{
    [Header("Identificación")]
    public string bowName = "Arco Básico";

    [Header("Disparo y Cooldown")]
    [Tooltip("Tiempo en segundos que el jugador debe mantener pulsado antes de que la flecha se dispare.")]
    public float drawTime = 0.5f;
    [Tooltip("Tiempo de espera entre disparos.")]
    public float cooldown = 1.0f;

    [Header("Coste de Recurso")]
    [Tooltip("Cantidad de 'Lágrimas de Sangre' que consume cada disparo.")]
    public float tearCost = 15f;

    [Header("Atributos de la Flecha")]
    [Tooltip("El prefab de la flecha que se va a disparar.")]
    public GameObject arrowPrefab;
    [Tooltip("Daño base del impacto de la flecha.")]
    public int baseDamage = 10;
    [Tooltip("Velocidad inicial de la flecha.")]
    public float arrowSpeed = 20f;
    [Tooltip("Distancia máxima que puede recorrer la flecha antes de clavarse.")]
    public float maxRange = 15f;

    [Header("Golpe Crítico")]
    [Tooltip("Probabilidad de golpe crítico (0.0 a 1.0).")]
    [Range(0f, 1f)]
    public float critChance = 0.1f;
    [Tooltip("Multiplicador de daño cuando ocurre un golpe crítico (ej: 2.0 para daño doble).")]
    public float critMultiplier = 2.0f;

    [Header("Efectos Visuales y Sonoros")]
    public AudioClip shootSound;
    public AudioClip groundImpactSound;

    // --- Preparado para Mejoras Futuras ---
    [Header("Mejoras (Estructura Futura)")]
    [Tooltip("Modificador para el coste de lágrimas (1.0 = normal, 0.8 = 20% más barato).")]
    [Range(0f, 1f)]
    public float tearCostModifier = 1.0f;

    [Tooltip("Modificador para el cooldown (1.0 = normal, 0.8 = 20% más rápido).")]
    [Range(0f, 1f)]
    public float cooldownModifier = 1.0f;

    [Tooltip("Número de flechas que atraviesa antes de romperse (0 = no atraviesa).")]
    public int piercingCount = 0;

    [Tooltip("Número de flechas adicionales a disparar en un abanico.")]
    public int multiShotCount = 0;

    [Tooltip("Ángulo del abanico para el multidisparo.")]
    public float multiShotAngle = 15f;
}