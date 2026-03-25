using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject startSpawnPlayerAnimation;
    [SerializeField]
    private GameObject endGameEagleAnimation;
    [SerializeField]
    private GameObject UI;
    [SerializeField]
    private GameObject pauseScreen;

    [Header("Room Information")]
    [SerializeField]
    private EnemyRoomInfo[] roomPrefabs;
    private EnemyRoomInfo currentRoomInfo;
    private int lastRoomIndex;
    private int roomsVisited;

    private bool playerLanded;
    public bool playerControl;
    public bool canPause;

    public EnemyRoomInfo CurrentRoomInfo => currentRoomInfo;

    [Header("Tutorial Room Info")]
    [SerializeField]
    private GeneralRoomInfo tutorialRoom;

    [Header("Safe Room Info")]
    [SerializeField]
    private GeneralRoomInfo safeRoom;
    [SerializeField]
    private int safeRoomFrequency;
    [SerializeField]
    private int hpRecoveryPercentage;
    private float hpRecovery;

    [Header("Pickups")]
    [SerializeField]
    private GameObject[] companionPickups;
    [SerializeField]
    private GameObject[] powerUpPickups;

    private GameObject companionPickUp;
    private GameObject weaponPickUp;

    private void Awake()
    {
        instance = this;
        canPause = true;
        playerLanded = false;
        playerControl = false;
        lastRoomIndex = -1;
    }

    private void Start()
    {
        StartCoroutine(WaitStartSpawnAnimationCO());
    }

    public void SetUpRoom()
    {
        roomsVisited++;
        if (roomsVisited % safeRoomFrequency == 0)
        {
            SetUpSafeRoom();
            return;
        }

        int randomIndex = Random.Range(0, roomPrefabs.Length);
        while (randomIndex == lastRoomIndex)
        {
            randomIndex = Random.Range(0, roomPrefabs.Length);
        }
        currentRoomInfo = roomPrefabs[randomIndex];
        lastRoomIndex = randomIndex;

        currentRoomInfo.door.GetComponent<Animator>().Play("Closed", 0, 0f);
        Camera.main.transform.position = currentRoomInfo.cameraTransform.position;
        CameraShake.instance.OriginalPos = currentRoomInfo.cameraTransform.position;
        Player.instance.transform.position = currentRoomInfo.playerSpawn.position;

        StartCoroutine(WaitForEnemiesSpawnCO());
    }

    IEnumerator WaitForEnemiesSpawnCO()
    {
        yield return new WaitForSeconds(2);
        EnemiesManager.instance.StartEnemiesSpawn(currentRoomInfo);
    }

    public void SetUpSafeRoom()
    {
        currentRoomInfo = null;
        safeRoom.door.GetComponent<Animator>().Play("Closed", 0, 0f);
        safeRoom.door.GetComponent<Door>().exitCol.enabled = false;

        Camera.main.transform.position = safeRoom.cameraTransform.position;

        Player.instance.transform.position = safeRoom.playerSpawn.position;

        float entryPlayerHp = Player.instance.CurrentStats.currentHeat;
        hpRecovery = Player.instance.CurrentStats.maxHeat * hpRecoveryPercentage / 100;
        float timer = 0.3f;
        for (int i = 0; i < hpRecovery / 5; i++)
        {
            StartCoroutine(HpRecoveryCO(timer, 5));
            timer += 0.3f;
        }
        safeRoom.door.GetComponentInChildren<Animator>().Play("Opening2", 0, 0f);
        safeRoom.door.GetComponent<Door>().exitCol.enabled = true;
    }

    private IEnumerator HpRecoveryCO(float timer, int recovery)
    {
        yield return new WaitForSeconds(timer);
        Player.instance.RestoreHeat(recovery);
    }

    public void SetUpTutorialRoom()
    {
        tutorialRoom.door.GetComponent<Animator>().Play("Closed", 0, 0f);
        tutorialRoom.door.GetComponent<Door>().exitCol.enabled = false;

        Camera.main.transform.position = tutorialRoom.cameraTransform.position;

        Player.instance.transform.position = tutorialRoom.playerSpawn.position;

        tutorialRoom.door.GetComponent<Animator>().Play("Opening2", 0, 0f);
        StartCoroutine(WaitDoorAnimationCO());
        tutorialRoom.door.GetComponent<Door>().exitCol.enabled = true;
    }

    public void OnRoomClear()
    {
        SpawnPowerUps();
    }

    public void OnGameEnd()
    {
        EnemiesManager.instance.OnGameEnd();
        canPause = false;
        StartCoroutine(WaitEagleAnimationCO());
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SpawnPowerUps()
    {
        int randomIndex = Random.Range(0, companionPickups.Length);
        companionPickUp = Instantiate(companionPickups[randomIndex]);
        companionPickUp.transform.position = currentRoomInfo.companionPickUpTr.position;

        randomIndex = Random.Range(0, powerUpPickups.Length);
        weaponPickUp = Instantiate(powerUpPickups[randomIndex]);
        weaponPickUp.transform.position = currentRoomInfo.weaponPowerUpPosition.position;
    }

    public void UnlockNextRoom()
    {
        if (companionPickUp)
        {
            Destroy(companionPickUp);
        }

        if (weaponPickUp)
        {
            Destroy(weaponPickUp);
        }

        currentRoomInfo.door.GetComponentInChildren<Animator>().Play("Opening2", 0, 0f);
        StartCoroutine(WaitDoorAnimationCO());
    }

    private IEnumerator WaitDoorAnimationCO()
    {
        yield return new WaitForSeconds(3.5f);

        currentRoomInfo.door.GetComponent<Door>().exitCol.enabled = true;
    }

    private IEnumerator WaitEagleAnimationCO()
    {
        UI.SetActive(false);
        endGameEagleAnimation.SetActive(false);
        yield return new WaitForSeconds(3.5f);
        endGameEagleAnimation.SetActive(true);
        yield return new WaitForSeconds(7);
        GameRestart();
    }

    private IEnumerator WaitStartSpawnAnimationCO()
    {
        yield return new WaitForSeconds(2);
        playerLanded = true;
    }

    public void GameStart(InputAction.CallbackContext context)
    {
        if (context.performed && playerLanded && !playerControl)
        {
            playerControl = true;
            startSpawnPlayerAnimation.SetActive(false);
            player.transform.position = tutorialRoom.playerSpawn.position;
            player.GetComponentInChildren<Player>().gameObject.transform.position = tutorialRoom.playerSpawn.position;
            Player.instance.anim.Play("GetUp");
        }
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.timeScale > 0 && canPause)
            {
                Time.timeScale = 0;
                pauseScreen.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                pauseScreen.SetActive(false);
            }
        }
    }
}