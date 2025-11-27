using UnityEngine;
using UnityEngine.InputSystem; // Necesario para Mouse

public class PlayerAttack : MonoBehaviour
{
    [Header("Armas")]
    public WeaponData[] weapons;
    private int currentWeaponIndex = 0;

    private WeaponAttack weaponAttack;

    [Header("Interfaz")]
    public WeaponSelectorUI weaponSelectorUI; // Referencia al selector UI

    private float attackFreezeTimer = 0f;
    public bool IsAttackFreezing => attackFreezeTimer > 0f;

    private float attackCooldownTimer = 0f;
    public bool IsAttackOnCooldown => attackCooldownTimer > 0f;

    // Para UI: obtener progreso del cooldown (0 a 1)
    public float GetCooldownProgress()
    {
        if (weapons.Length == 0 || currentWeaponIndex >= weapons.Length || weapons[currentWeaponIndex] == null)
            return 0f;

        float maxCooldown = weapons[currentWeaponIndex].attackCooldown;
        if (maxCooldown <= 0f) return 0f;

        // Retorna 1 cuando está lleno (cooldown completo), 0 cuando está vacío (listo para atacar)
        return Mathf.Clamp01(attackCooldownTimer / maxCooldown);
    }

    // Para UI: obtener tiempo restante en segundos
    public float GetCooldownTimeRemaining()
    {
        return Mathf.Max(0f, attackCooldownTimer);
    }

    void Start()
    {
        weaponAttack = GetComponent<WeaponAttack>();

        if (weapons.Length > 0)
        {
            EquipWeapon(currentWeaponIndex);

            // Inicializar interfaz si existe
            if (weaponSelectorUI != null)
            {
                weaponSelectorUI.SetWeaponList(weapons);
                weaponSelectorUI.UpdateDisplay(weapons[currentWeaponIndex]);
            }

            // Mensaje inicial
            Debug.Log("Arma inicial equipada: " + FormatWeaponStats(weapons[currentWeaponIndex]));
        }
    }

    void Update()
    {
        if (attackFreezeTimer > 0f)
            attackFreezeTimer -= Time.deltaTime;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        // Poder atacar si: NO estás congelado Y NO estás en cooldown
        bool canAttack = !IsAttackFreezing && !IsAttackOnCooldown;

        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack)
        {
            weaponAttack.Attack();
            if (weapons.Length > 0 && currentWeaponIndex < weapons.Length && weapons[currentWeaponIndex] != null)
            {
                attackFreezeTimer = weapons[currentWeaponIndex].attackFreezeDuration;
                attackCooldownTimer = weapons[currentWeaponIndex].attackCooldown;
                Debug.Log($"Ataque ejecutado. Cooldown: {weapons[currentWeaponIndex].attackCooldown}s");
            }
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0) NextWeapon();
        else if (scroll < 0) PreviousWeapon();
    }

    void EquipWeapon(int index)
    {
        if (weapons.Length == 0 || index >= weapons.Length) return;

        weaponAttack.EquipWeapon(weapons[index]);

        if (weaponSelectorUI != null)
        {
            weaponSelectorUI.UpdateDisplay(weapons[index]);
        }

        Debug.Log("Arma equipada: " + FormatWeaponStats(weapons[index]));
    }

    void NextWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        EquipWeapon(currentWeaponIndex);
    }

    void PreviousWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
        EquipWeapon(currentWeaponIndex);
    }

    string FormatWeaponStats(WeaponData weapon)
    {
        return $"{weapon.weaponName} [Daño: {weapon.damage}, Cooldown: {weapon.attackCooldown}, Rango: {weapon.attackRange}, Radio: {weapon.attackRadius}, Empuje: {weapon.knockbackForce}]";
    }
}
