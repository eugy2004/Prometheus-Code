using UnityEngine;

public class PowerUpShotgun : WeaponPowerUp
{
    private void Awake()
    {
        assignedWeapon = new Shotgun();
        InitializeWeaponStats();
        assignedWeapon.InstantiateWeapon(assignedWeaponStats);
    }
}
