using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSelectorUI : MonoBehaviour
{
    [Header("UI Referencias")]
    public Image weaponIconImage;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponStatsText;

    // Se puede almacenar la lista si la necesitas más tarde
    private WeaponData[] weaponList;

    public void SetWeaponList(WeaponData[] weapons)
    {
        weaponList = weapons;
    }

    public void UpdateDisplay(WeaponData weapon)
    {
        if (weapon == null) return;

        if (weaponIconImage != null)
            weaponIconImage.sprite = weapon.icon;

        if (weaponNameText != null)
            weaponNameText.text = weapon.weaponName;

        if (weaponStatsText != null)
        {
            weaponStatsText.text =
                $"Daño: {weapon.damage}\n" +
                $"Rango: {weapon.attackRange}\n" +
                $"Radio: {weapon.attackRadius}\n" +
                $"Empuje: {weapon.knockbackForce}\n" +
                $"Cooldown: {weapon.attackCooldown:0.00}s";
        }
    }
}
