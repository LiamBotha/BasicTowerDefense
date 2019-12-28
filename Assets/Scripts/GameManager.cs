using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private PlayerBase playerBase;
    private PlayerModel playerModel;

    int numOfEnemies; // the number of enemies in the round
    int numOfLanes; // the number of lanes enemies spawn from
    int numOfTypes;
    int spawnRemaining;
    int goldAmount = 15;

    float previousHealthAverage = 0;

    readonly int turretCost = 5; // Gold Cost for spawning a turret
    readonly int statUpgradeCost = 3;
    readonly int typeChangeCost = 10;

    static int waveNumber = 0; // the current wave
    readonly int numofWaves = 6;

    bool isGameOver = false;

    [SerializeField] GameObject[] spawnPoints;
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] GameObject frankensteinEnemyPrefab;
    [SerializeField] List<Turret> turrets;
    [SerializeField] List<Turret> leftTurrets;
    [SerializeField] List<Turret> rightTurrets;
    [SerializeField] List<Turret> topTurrets;
    [SerializeField] List<Turret> bottomTurrets;

    private List<BaseUnit> enemies = new List<BaseUnit>();
    private List<GameObject> deactivatedSpawns = new List<GameObject>(); 

    [SerializeField] GameObject turretPrefab;

    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI atkText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI descriptText;
    [SerializeField] Button atkButton;
    [SerializeField] Button speedButton;
    [SerializeField] Button multiModeButton;
    [SerializeField] Button grenadeModeButton;
    [SerializeField] GameObject GameWonPanel;
    [SerializeField] GameObject GameLostPanel;

    private Turret selectedTurret;
    private BaseUnit selectedUnit;

    //Sound
    private AudioSource audioSource;

    [SerializeField] AudioClip gameoverSound;
    [SerializeField] AudioClip victorySound;
    [SerializeField] AudioClip waveStartSound;
    [SerializeField] AudioClip waveEndSound;
    [SerializeField] AudioClip upgradeSound;

    public static event Action OneRemainingEnemy = delegate { };

    private void Start()
    {
        numOfEnemies = (waveNumber + 2) * 3;

        if (waveNumber == 0)
        {
            numOfLanes = 1;
            numOfTypes = 2;
        }
        else if( waveNumber < 3)
        {
            numOfLanes = 2;
            numOfTypes = 3;
        }
        else
        {
            numOfLanes = 4;
            numOfTypes = 4;
        }

        goldText.text = "Gold: " + goldAmount.ToString();

        audioSource.PlayOneShot(waveStartSound, 0.9f);

        StartCoroutine(WaveSetup());
    }

    private void Update()
    {
        MouseRaycast();

        if(Input.GetKeyDown(KeyCode.R) && isGameOver)
        {
            waveNumber = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(Input.GetKeyDown(KeyCode.Escape) && isGameOver)
        {
            Application.Quit();
        }

        DisplayInfo(selectedUnit);
    }

    IEnumerator WaveSetup()
    {
        spawnRemaining = numOfEnemies;
        for(int i = 0; i < numOfEnemies;++i)
        {
            if(spawnRemaining == (numOfEnemies / 2) + 1)
            {
                --spawnRemaining;
                SpawnFrankenstein(UnityEngine.Random.Range(0, numOfLanes));
                yield return new WaitForSeconds(4f);
            }
            else if(spawnRemaining == 1)
            {
                --spawnRemaining;
                SpawnFrankenstein(UnityEngine.Random.Range(0, numOfLanes));
                yield return new WaitForSeconds(4f);
            }
            else if(isGameOver == false) // start with 1 enemy type and increase with waves. 
            {
                --spawnRemaining;

                int enemyRand = playerModel.GetRange(numOfTypes);
                SpawnEnemy(enemyRand, UnityEngine.Random.Range(0, numOfLanes));

                //SpawnEnemy(UnityEngine.Random.Range(0, enemyPrefabs.Length), UnityEngine.Random.Range(0, numOfLanes));
                yield return new WaitForSeconds(2f);
            }
        }
    } // Used in the POE

    void SpawnEnemy(int val,int spawnPos)
    {
        int bestLane = GetLeastTurrets();

        //var spawnPoint = spawnPoints[spawnPos].transform;
        var spawnPoint = spawnPoints[bestLane].transform;
        var pos = spawnPoint.position + transform.forward * ( 1.5f * UnityEngine.Random.Range(-1,2));

        GameObject enemyObj = Instantiate(enemyPrefabs[val], pos, spawnPoint.rotation, GameObject.Find("Enemies").transform); ;
        BaseUnit enemy = enemyObj.GetComponent<BaseUnit>();
        enemy.currentLane = spawnPos;
        enemies.Add(enemy);
    } // Used in the POE

    void SpawnFrankenstein(int spawnPos)
    {
        int bestLane = GetLeastTurrets();

        var spawnPoint = spawnPoints[bestLane].transform;
        var pos = spawnPoint.position + transform.forward * (1.5f * UnityEngine.Random.Range(-1, 2));

        GameObject enemyObj = Instantiate(frankensteinEnemyPrefab, pos, spawnPoint.rotation, GameObject.Find("Enemies").transform);
        BaseUnit enemy = enemyObj.GetComponent<BaseUnit>();
        enemy.currentLane = spawnPos;
        enemies.Add(enemy);
    }

    private void MouseRaycast() //Checks if the player clicked on anything
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.collider != null && hit.transform.name.Contains("TurretSpawn_") && goldAmount >= turretCost)
                {
                    GoldSpent(turretCost);
                    CreateTurret(hit.transform.gameObject);
                }
            }
        }
    }

    private void CreateTurret(GameObject spawnPos)
    {
        Vector3 turretPos = new Vector3(spawnPos.transform.position.x, 0.5f, spawnPos.transform.position.z);

        Turret turret = GameObject.Instantiate(turretPrefab, turretPos, spawnPos.transform.rotation).GetComponent<Turret>();

        Lane lane;
        if (spawnPos.name.Contains("TurretSpawn_Left"))
        {
            lane = Lane.Left;
            leftTurrets.Add(turret);
        }
        else if (spawnPos.name.Contains("TurretSpawn_Right"))
        {
            lane = Lane.Right;
            rightTurrets.Add(turret);
        }
        else if (spawnPos.name.Contains("TurretSpawn_Top"))
        {
            lane = Lane.Top;
            topTurrets.Add(turret);
        }
        else if (spawnPos.name.Contains("TurretSpawn_Bottom"))
        {
            lane = Lane.Bottom;
            bottomTurrets.Add(turret);
        }
        else lane = default(Lane);


        turret.turretLane = lane;
        turret.SelectTurret();

        playerModel.numTurretsBuilt[0]++;

        deactivatedSpawns.Add(spawnPos);
        spawnPos.SetActive(false);
    }

    private void GoldGained(int val,BaseUnit enemy)
    {
        var difModifier = playerModel.GetDifficulty() / 2;
        goldAmount += val * difModifier;
        goldText.text = "Gold: " + goldAmount.ToString();
    }

    private void GoldSpent(int val)
    {
        goldAmount -= val;
        goldText.text = "Gold: " + goldAmount.ToString();
    }

    private void EnemyKilled(int val,BaseUnit enemy)
    {
        --numOfEnemies;
        enemies.Remove(enemy);

        playerModel.AddTime(enemy);

        goldAmount += UnityEngine.Random.Range(1, 2 * playerModel.GetDifficulty());

        if (enemies.Count == 1 && spawnRemaining <= 0)
        {
            OneRemainingEnemy();
        }
        else if(enemies.Count <= 0)
        {
            if(this != null)
            {
                StartCoroutine(WaveCompleted());
            }
        }
    }

    private IEnumerator WaveCompleted() // add gold gain and after wave finished
    {
        audioSource.PlayOneShot(waveEndSound, 0.3f);

        Debug.Log("Wave " + waveNumber + " Completed");
        GoldGained(5 * waveNumber,null);


        GetWaveData();

        RaycastHit hit;
        foreach(GameObject spawn in deactivatedSpawns)
        {
            if(spawn != null && !Physics.Raycast(spawn.transform.position, Vector3.up, out hit, 0.5f))
            {
                spawn.SetActive(true);
            }
        }

        yield return new WaitForSeconds(1f);
        if (waveNumber < numofWaves)
        {
            ++waveNumber;
            Start();
        }
        else EndGame();
    } // Tweaked for POE

    private void EndGame()
    {
        GameWonPanel.SetActive(true);
        isGameOver = true;

        audioSource.clip = victorySound;
        audioSource.Play();
    }

    private void GameLost()
    {
        foreach(var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        if(GameLostPanel != null)
        {
            GameLostPanel.SetActive(true);
            isGameOver = true;
        }

        audioSource.clip = gameoverSound;
        audioSource.Play();
    }

    private void TurretDestroyed(Turret turret)
    {
        turrets.Remove(turret);

        switch (turret.turretLane)
        {
            case Lane.Left:
                {
                    leftTurrets.Remove(turret);
                    if(leftTurrets.Count == 0)
                    {
                        playerBase.LaneEmpty(Lane.Left);
                    }
                }
                break;
            case Lane.Right:
                {
                    rightTurrets.Remove(turret);
                    if (rightTurrets.Count == 0)
                    {
                        playerBase.LaneEmpty(Lane.Right);
                    }
                }
                break;
            case Lane.Top:
                {
                    topTurrets.Remove(turret);
                    if (topTurrets.Count == 0)
                    {
                        playerBase.LaneEmpty(Lane.Top);
                    }
                }
                break;
            case Lane.Bottom:
                {
                    bottomTurrets.Remove(turret);
                    if (bottomTurrets.Count == 0)
                    {
                        playerBase.LaneEmpty(Lane.Bottom);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void DisplayInfo(BaseUnit unit)
    {
        if(unit != null)
        {
            selectedUnit = unit;
            var splitData = unit.ToString().Split(';');

            if (nameText.gameObject.activeSelf == false)
            {
                nameText.gameObject.SetActive(true);
                hpText.gameObject.SetActive(true);
                atkText.gameObject.SetActive(true);
                speedText.gameObject.SetActive(true);
                descriptText.gameObject.SetActive(true);
            }
            nameText.text = splitData[0];
            hpText.text = splitData[2];
            atkText.text = splitData[3];
            speedText.text = splitData[4];
            descriptText.text = splitData[1];

            if (unit.GetType().ToString() == "Turret")
            {
                selectedTurret = unit as Turret;

                atkButton.gameObject.SetActive(true);
                speedButton.gameObject.SetActive(true);
                multiModeButton.gameObject.SetActive(true);
                grenadeModeButton.gameObject.SetActive(true);
            }
            else if (atkButton.gameObject.activeSelf)
            {
                atkButton.gameObject.SetActive(false);
                speedButton.gameObject.SetActive(false);
                multiModeButton.gameObject.SetActive(false);
                grenadeModeButton.gameObject.SetActive(false);
            }
        }    
    }

    private void UpdateTurretInfo() // add health text
    {
        var turretData = selectedTurret.ToString().Split(';');

        nameText.text = turretData[0];

        hpText.text = turretData[1];
        atkText.text = turretData[2];
        speedText.text = turretData[3];
    }

    public void IncreaseStats(string stat)
    {
        if(selectedTurret != null && goldAmount >= statUpgradeCost)
        {
            audioSource.PlayOneShot(upgradeSound, 1);

            if (stat == "atk")
            {
                ++selectedTurret.Atk;
                atkText.text = "Atk: " + selectedTurret.Atk;
                GoldSpent(statUpgradeCost);
            }
            else if(stat == "speed")
            {
                ++selectedTurret.Speed;

                speedText.text = "Speed: " + selectedTurret.Speed;
                GoldSpent(statUpgradeCost);
            }
        }
    }

    public void ChangeSelectedTurretType(int typeVal)
    {
        if(goldAmount >= typeChangeCost)
        {
            if (typeVal == 1 && selectedTurret.Type != TurretType.Multi_Attack)
            {
                GoldSpent(typeChangeCost);
                selectedTurret.ChangeTurretType(TurretType.Multi_Attack);
                playerModel.numTurretsBuilt[1]++;
            }
            else if (typeVal == 2 && selectedTurret.Type != TurretType.Grenadier)
            {
                GoldSpent(typeChangeCost);
                selectedTurret.ChangeTurretType(TurretType.Grenadier);
            }
            playerModel.numUpgrades++;

            audioSource.PlayOneShot(upgradeSound, 1);

            UpdateTurretInfo();
        }
    }

    public int GetLeastTurrets()
    {
        int[] numTurrets = new int[4];

        numTurrets[0] = leftTurrets.Count;
        numTurrets[1] = rightTurrets.Count;
        numTurrets[2] = topTurrets.Count;
        numTurrets[3] = bottomTurrets.Count;

        Array.Sort(numTurrets);

        if(numTurrets[0] == leftTurrets.Count)
        {
            return 0;
        }
        else if(numTurrets[0] == rightTurrets.Count)
        {
            return 1;
        }
        else if (numTurrets[0] == topTurrets.Count)
        {
            return 2;
        }
        else if (numTurrets[0] == bottomTurrets.Count)
        {
            return 3;
        }

        return 0;
    }

    // New Methods for the POE
    private void GetWaveData() // TODO add threshold to determine how much to increase / decrease
    {
        float currentHealthAverage = GetAverageHealth();

        if(currentHealthAverage + 0.1f > previousHealthAverage)
        {
            playerModel.SetDifficulty(playerModel.GetDifficulty() + 1);
        }
        else if(currentHealthAverage > (previousHealthAverage - 0.15f))
        {
            playerModel.SetDifficulty(playerModel.GetDifficulty() - 1);
        }
    }

    private float GetAverageHealth()
    {
        int val = playerBase.Hp;
        int maxVal = playerBase.MaxHp;

        foreach (Turret turret in turrets)
        {
            val += turret.Hp;
            maxVal += turret.MaxHp;
        }

        return val / maxVal;
    } // gets the players total average health

    public  void LoadCredits()
    {
        SceneManager.LoadScene(1);
    }

    // Event listeners
    private void OnEnable()
    {
        playerModel = new PlayerModel();

        BaseUnit.OnAnyUnitHover += DisplayInfo;

        Enemy.OnAnyEnemyKilled += EnemyKilled;
        Enemy.OnAnyEnemyKilled += GoldGained;
        SmartEnemy.OnAnyEnemyKilled += EnemyKilled;
        SmartEnemy.OnAnyEnemyKilled += GoldGained;

        Turret.OnAnyTurretDestroyed += TurretDestroyed;

        audioSource = GetComponent<AudioSource>();
        playerBase = GameObject.FindObjectOfType<PlayerBase>();
        playerBase.OnPlayerBaseDestroyed += GameLost;
    }

    private void OnDisable()
    {
        BaseUnit.OnAnyUnitHover -= DisplayInfo;

        Enemy.OnAnyEnemyKilled -= EnemyKilled;
        Enemy.OnAnyEnemyKilled -= GoldGained;
        SmartEnemy.OnAnyEnemyKilled += EnemyKilled;
        SmartEnemy.OnAnyEnemyKilled += GoldGained;

        Turret.OnAnyTurretDestroyed -= TurretDestroyed;

        playerBase.OnPlayerBaseDestroyed -= GameLost;
    }

}
