using UnityEngine;

public class PowerUpSMG : WeaponPowerUp
{
    private void Awake()
    {
        assignedWeapon = new SMG();
        InitializeWeaponStats();
        assignedWeapon.InstantiateWeapon(assignedWeaponStats);
    }
}
