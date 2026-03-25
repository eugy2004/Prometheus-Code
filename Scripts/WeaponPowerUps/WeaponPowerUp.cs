using System.Collections;
using UnityEngine;

public abstract class WeaponPowerUp : MonoBehaviour
{
    [SerializeField]
    protected WeaponGeneralStats assignedWeaponStats;
    [SerializeField]
    protected WeaponGeneralStats baseWeaponStats;

    protected BaseWeapon assignedWeapon;
    private Collider col;

    private void Start()
    {
        StartCoroutine(ActivateColliderCO());
        col = GetComponent<Collider>();
    }

    private IEnumerator ActivateColliderCO()
    {
        yield return new WaitForSeconds(2);
        if (col)
            col.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (player.EquippedWeapon.GetType() != assignedWeapon.GetType())
            {
                player.EquippedWeapon = assignedWeapon;
                player.EquippedWeapon.weaponStats = assignedWeaponStats;
            }
            else
            {
                assignedWeapon.LevelUpWeapon();
            }
            GameManager.instance.UnlockNextRoom();
        }
    }

    protected void InitializeWeaponStats()
    {
        assignedWeaponStats.damage = baseWeaponStats.damage;
        assignedWeaponStats.bulletLifeTime = baseWeaponStats.bulletLifeTime;
        assignedWeaponStats.projectileSpread = baseWeaponStats.projectileSpread;
        assignedWeaponStats.bulletSpeed = baseWeaponStats.bulletSpeed;
        assignedWeaponStats.bulletSize = baseWeaponStats.bulletSize;
        assignedWeaponStats.fireCooldown = baseWeaponStats.fireCooldown;
        assignedWeaponStats.projectilesPerShot = baseWeaponStats.projectilesPerShot;
        assignedWeaponStats.pushBackForce = baseWeaponStats.pushBackForce;
    }
}
