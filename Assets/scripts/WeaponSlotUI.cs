using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    public Image iconImage;
    public WeaponData weaponData;

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
            iconImage.sprite = icon;
    }
}
