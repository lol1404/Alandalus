using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;

    [Header("Stats")]
    public int damage = 1;
    public float attackCooldown = 1f;
    public float attackRange = 1f;
    public float knockbackForce = 5f;
    public float attackRadius = 0.5f;
    public float attackFreezeDuration = 0.1f;

    [Header("Player Attacker Knockback")]
    public float playerAttackKnockbackForce = 0f;
    public float playerAttackKnockbackDuration = 0.08f;

    [Header("Visual/Audio")]
    public Sprite icon; // <- NUEVO: icono para el selector de armas
    public GameObject attackEffectPrefab;
    public AudioClip attackSound;
}
