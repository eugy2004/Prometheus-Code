using TMPro;
using UnityEngine;

public class StatText : MonoBehaviour
{
    public PrometheusStats playerStats;
    public WeaponGeneralStats weaponGeneralStats;
    public TextMeshProUGUI playerStatsText;
    private void Update()
    {
        playerStatsText.text = "Stats Player: \nHeat: " + playerStats.currentHeat
            + "\nSpeed: " + playerStats.moveSpeed
            + "\nStats Gun: \nDamage: " + weaponGeneralStats.damage
            + "\nType: " + weaponGeneralStats.name
            + "\nCooldown hit: " + weaponGeneralStats.fireCooldown
            + "\nRange: " + weaponGeneralStats.bulletLifeTime
            + "\nProjectile Spd: " + weaponGeneralStats.bulletSpeed
            ;
    }
}
