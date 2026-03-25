using UnityEngine;

public class EnemyRoomInfo : GeneralRoomInfo
{
    [Header("Positions")]
    public Transform roomCenter;
    public Transform weaponPowerUpPosition;
    public Transform companionPickUpTr;

    [Header("Enemies")]
    public int minNumberOfHoplites;
    public int maxNumberOfHoplites;
    public int minNumberOfMinotaurs;
    public int maxNumberOfMinotaurs;
    public int minNumberOfCyclopses;
    public int maxNumberOfCyclopses;
    public GameObject[] enemySpawnPositions;

    [Header("Waves")]
    public int numberOfWaves;
}
    
