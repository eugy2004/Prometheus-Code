using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletPoolingManager : MonoBehaviour
{
    public static BulletPoolingManager instance;

    [Header("Player")]
    [SerializeField]
    private int playerPoolSize = 15;
    [SerializeField]
    private GameObject playerBulletPrefab;
    [SerializeField]
    private int sniperPoolSize;
    [SerializeField]
    private GameObject sniperBulletPrefab;

    [Header("Enemy")]
    [SerializeField]
    private int enemyPoolSize = 20;
    [SerializeField]
    private GameObject enemyBulletPrefab;

    [Header("Turret")]
    [SerializeField]
    private int turretPoolSize = 3;
    [SerializeField]
    private GameObject turretBulletPrefab;

    private List<GameObject> playerBullets;
    private List<GameObject> sniperBullets;
    private List<GameObject> enemyBullets;
    private List<GameObject> turretBullets;

    private void Awake()
    {
        instance = this;

        playerBullets = new ();
        sniperBullets = new ();
        enemyBullets = new ();
        turretBullets = new ();

        PopulateBulletList(playerBullets, playerBulletPrefab, playerPoolSize);
        PopulateBulletList(sniperBullets, sniperBulletPrefab, sniperPoolSize);
        PopulateBulletList(enemyBullets, enemyBulletPrefab, enemyPoolSize);
        PopulateBulletList(turretBullets, turretBulletPrefab, turretPoolSize);
    }

    private GameObject GetPooledBullet(List<GameObject> bullets)      // restituisce un proiettile inattivo dalla lista
    {
        for (int i = 0; i < bullets.Count(); i++)
        {
            if (!bullets[i].activeInHierarchy)
            {
                return bullets[i];
            }
        }
        if (bullets == playerBullets)
        {
            GameObject newPlayerBullet = Instantiate(playerBulletPrefab);
            playerBullets.Add(newPlayerBullet);
            return newPlayerBullet;
        }
        if (bullets == sniperBullets)
        {
            GameObject newSniperBullet = Instantiate(sniperBulletPrefab);
            sniperBullets.Add(newSniperBullet);
            return newSniperBullet;
        }
        if (bullets == enemyBullets)
        {
            GameObject newEnemyBullet = Instantiate(enemyBulletPrefab);
            enemyBullets.Add(newEnemyBullet);
            return newEnemyBullet;
        }
        if (bullets == turretBullets)
        {
            GameObject newTurretBullet = Instantiate(turretBulletPrefab);
            turretBullets.Add(newTurretBullet);
            return newTurretBullet;
        }
        return null;
    }

    public GameObject GetPooledEnemyBullet()
    {
        return GetPooledBullet(enemyBullets);
    }

    public GameObject GetPooledPlayerBullet()
    {
        return GetPooledBullet(playerBullets);
    }

    public GameObject GetPooledSniperBullet()
    {
        return GetPooledBullet(sniperBullets);
    }

    public GameObject GetPooledTurretBullet()
    {
        return GetPooledBullet(turretBullets);
    }

    private void PopulateBulletList(List<GameObject> bullets, GameObject bulletPrefab, int poolSize)
    {
        for (int i = 0; i < poolSize; i++)         // costruzione della lista di proiettili
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullets.Add(bullet);
            bullet.SetActive(false);
        }
    }
}
