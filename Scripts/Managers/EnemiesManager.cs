using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager instance;

    [Header("Hoplites")]
    [SerializeField]
    private GameObject hoplitePrefab;
    [SerializeField]
    private int hoplitePoolSize;

    [Header("Minotaurs")]
    [SerializeField]
    private GameObject minotaurPrefab;
    [SerializeField]
    private int minotaurPoolSize;

    [Header("Cyclopses")]
    [SerializeField]
    private GameObject cyclopsPrefab;
    [SerializeField]
    private int cyclopsPoolSize;

    private List<Enemy> hoplites;
    private List<Enemy> minotaurs;
    private List<Enemy> cyclopses;

    private EnemyRoomInfo currentRoomInfo;

    [SerializeField]
    private List<Enemy> enemies;
    private int currentNumberOfEnemies;
    private int enemyWavesSpawned;
    public int CurrentNumberOfEnemies { get => currentNumberOfEnemies; set => currentNumberOfEnemies = value; }

    private void Awake()
    {
        instance = this;

        hoplites = new ();
        minotaurs = new ();
        cyclopses = new ();
        enemies = new ();

        PopulateEnemyList(hoplites, hoplitePrefab, hoplitePoolSize);
        PopulateEnemyList(minotaurs, minotaurPrefab, minotaurPoolSize);
        PopulateEnemyList(cyclopses, cyclopsPrefab, cyclopsPoolSize);
    }

    private void PopulateEnemyList(List<Enemy> enemies, GameObject enemyPrefab, int poolSize)
    {
        for (int i = 0; i < poolSize; i++)         // costruzione della lista di proiettili
        {
            Enemy enemy = Instantiate(enemyPrefab).GetComponentInChildren<Enemy>();
            enemies.Add(enemy);
            enemy.parent.SetActive(false);
        }
    }

    public void StartEnemiesSpawn(EnemyRoomInfo room)
    {
        DeactivateEnemies();
        ResetEnemies();
        currentRoomInfo = room;
        SpawnEnemyWave();
    }

    private void SpawnEnemy(EnemyRoomInfo room, List<Enemy> list, int numberOfSpawns)
    {
        Enemy enemy;
        Vector3 spawnPosition;
        int numberOfSpawnPositions = room.enemySpawnPositions.Length;

        for (int i = 0; i < numberOfSpawns; i++)
        {
            int randomIndex = Random.Range(0, numberOfSpawnPositions);
            spawnPosition = room.enemySpawnPositions[randomIndex].transform.position;

            enemy = GetPooledEnemy(list);
            currentNumberOfEnemies++;
            enemies.Add(enemy);
            enemy.transform.position = spawnPosition;
            enemy.parent.SetActive(true);
        }
    }

    public void SpawnEnemyWave()
    {
        enemies.Clear();
        enemyWavesSpawned++;
        int numberOfWaves = currentRoomInfo.numberOfWaves;
        int hoplitesToSpawn = Random.Range(currentRoomInfo.minNumberOfHoplites / numberOfWaves, 1 + currentRoomInfo.maxNumberOfHoplites / numberOfWaves);
        int minotaursToSpawn = Random.Range(currentRoomInfo.minNumberOfMinotaurs / numberOfWaves, 1 + currentRoomInfo.maxNumberOfMinotaurs / numberOfWaves);
        int cyclopsesToSpawn = Random.Range(currentRoomInfo.minNumberOfCyclopses / numberOfWaves, 1 + currentRoomInfo.maxNumberOfCyclopses / numberOfWaves);
        SpawnEnemy(currentRoomInfo, hoplites, hoplitesToSpawn);
        SpawnEnemy(currentRoomInfo, minotaurs, minotaursToSpawn);
        SpawnEnemy(currentRoomInfo, cyclopses, cyclopsesToSpawn);
    }

    private Enemy GetPooledEnemy(List<Enemy> enemies)      // restituisce un proiettile inattivo dalla lista
    {
        for (int i = 0; i < enemies.Count(); i++)
        {
            if (!enemies[i].parent.activeInHierarchy)
            {
                enemies[i].parent.SetActive(true);
                enemies[i].gameObject.SetActive(true);
                enemies[i].enabled = true;
                return enemies[i];
            }
        }
        if (enemies == minotaurs)
        {
            Enemy newMinotaur = Instantiate(minotaurPrefab).GetComponent<Enemy>();
            minotaurs.Add(newMinotaur);
            return newMinotaur;
        }
        if (enemies == cyclopses)
        {
            Enemy newCyclops = Instantiate(cyclopsPrefab).GetComponent<Enemy>();
            cyclopses.Add(newCyclops);
            return newCyclops;
        }
        return null;
    }

    public void ResetEnemies()
    {
        currentNumberOfEnemies = 0;
        enemies.Clear();
    }

    private void DeactivateEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.parent.SetActive(false);
        }
    }

    public void OnGameEnd()
    {
        DeactivateEnemies();
    }

    public void DecreaseEnemyCount()
    {
        currentNumberOfEnemies--;
        if (currentNumberOfEnemies == 0)
        {
            if (enemyWavesSpawned < currentRoomInfo.numberOfWaves)
                SpawnEnemyWave();
            else if (Player.instance.GetCurrentHeat() >= 0)
            {
                enemyWavesSpawned = 0;
                GameManager.instance.OnRoomClear();
            }
        }
    }
}
