using UnityEngine;

public class WeaponPlaceholderManager : MonoBehaviour
{
    private BaseWeapon currentWeapon;
    private GameObject currentPlaceholder;

    public GameObject emptyPlaceholder;
    public GameObject shotgunPlaceholder;
    public GameObject SMGPlaceholder;
    public GameObject SniperPlaceholder;

    private void Start()
    {
        currentWeapon = Player.instance.EquippedWeapon;
        currentPlaceholder = emptyPlaceholder;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.instance && currentWeapon != Player.instance.EquippedWeapon)
        {
            BaseWeapon newWeapon = Player.instance.EquippedWeapon;
            currentWeapon = newWeapon;
            if (newWeapon is Shotgun)
            {
                shotgunPlaceholder.SetActive(true);
                currentPlaceholder.SetActive(false);
                currentPlaceholder = shotgunPlaceholder;
            }
            else if (newWeapon is SMG)
            {
                SMGPlaceholder.SetActive(true);
                currentPlaceholder.SetActive(false);
                currentPlaceholder = SMGPlaceholder;
            }
            else if (newWeapon is Sniper)
            {
                SniperPlaceholder.SetActive(true);
                currentPlaceholder.SetActive(false);
                currentPlaceholder = SniperPlaceholder;
            }
        }
    }
}
