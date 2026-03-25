using UnityEngine;

public class PowerUpSniper : WeaponPowerUp
{
    private void Awake()
    {
        assignedWeapon = new Sniper();
        InitializeWeaponStats();
        assignedWeapon.InstantiateWeapon(assignedWeaponStats);
    }
}
