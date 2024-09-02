using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaveManager : MonoBehaviour
{
    //Script that controls the Waves from Harvest to Attack Waves
    //There are 5 waves
    private const float SECONDS = 60,
                        SHRINK_FACTOR = 0.4f,
                        BOUND_OFFSET = 0.5f;
    public enum WaveType
    {
        Normal, Attack, Win, Lose
    };

    private WaveType waveType;
    private int waveIndex = 0;

    [SerializeField]
    private WaveData[] waveData = new WaveData[5];

    [Space]
    [Header("Timer")]
    [SerializeField]
    private float waveTimer, waveTimerThresold;


    [Space]
    [Header("Chests")]
    private ChestSpawner[] chestSpawners;

    [SerializeField]
    private GameObject demogorgon, demodogs, demobats, vecna;
    private List<EnemyController> monstersAlive = new List<EnemyController>();

    [Space]
    [SerializeField]
    private Transform waveBounds;
    private Collider waveBoundsCollider;

    private GameManager gameManager;

    private void Awake()
    {
        waveBoundsCollider = waveBounds.GetComponent<Collider>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (!(gameManager.GetGameMode() == GameManager.GameMode.GameStarted))
            return;
        if(waveType == WaveType.Normal)
        {
            waveTimer += Time.deltaTime;
            if (waveIndex > waveData.Length - 1)
                return;
            if(waveTimer > waveData[waveIndex].timeBeforeWaveStarts * SECONDS)
            {
                waveTimer = 0;
                ChangeWave(WaveType.Attack);
            }
            NormalWave();

        }
        else if(waveType == WaveType.Attack)
        {
            if(monstersAlive.Count < 1)//if all the monsters have been killed, switch to normal mode
            {
                ChangeWave(WaveType.Normal);
            }
            AttackWave();
        }
    }

    private void LateUpdate()
    {
        UpdateMonstersAlive();
    }

    public void ChangeWave(WaveType waveType)
    {
        this.waveType = waveType;
        switch (waveType)
        {
            case WaveType.Normal:
                TriggerNormalWave();
                //NormalWave();
                break;
            case WaveType.Attack:
                TriggerAttackWave();
                //AttackWave();
                break;
            default:
                break;
        }
    }

    public WaveType GetCurrentWave()
    {
        return waveType;
    }

    private void TriggerNormalWave()
    {
        waveType = WaveType.Normal;
        //stuff that happens as soon as normal wave starts
        //UI to show wave has started

        //spawn chests around map
        SpawnChests();

        //Destroy any unecessary monsters
        GameObject[] lostEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in lostEnemies)
        {
            SpawnManager.DestroyNetworkObject(gameObject);
        }


    }

    private void TriggerAttackWave()
    {
        waveType = WaveType.Attack;
        waveIndex++;
        //stuff that happens as soon as attack wave starts
        //VFX to show wave has started
        //UI to show wave has started
        //Monsters spawning
        monstersAlive = SpawnMonsters();

        //Destroy All Chests
        //Destroy any unecessary monsters
        GameObject[] chests = GameObject.FindGameObjectsWithTag("Chest");
        foreach (GameObject chest in chests)
        {
            SpawnManager.DestroyNetworkObject(chest);
        }
    }

    private void NormalWave()
    {
        float boundShrink = Time.deltaTime * SHRINK_FACTOR;
        waveBounds.localScale = new Vector3(waveBounds.localScale.x - boundShrink, waveBounds.localScale.y, waveBounds.localScale.z - boundShrink);
    }

    private void AttackWave()
    {

    }

    private void SpawnChests()
    {
        GetChestSpawners();
        foreach (var chest in chestSpawners)
        {
            chest.SpawnChest();
        }
    }

    private List<EnemyController> SpawnMonsters()
    {
        List<EnemyController> monsters = new List<EnemyController>();
        foreach(EnemyAmountPerWave enemy in waveData[waveIndex - 1].enemiesInWave)
        {
            for(int i = 0; i < enemy.numberOfEnemies; i++)
            {
                //Spawn Algorithm goes here
                RaycastHit hit;
                float x, y, z, rayDistance;
                x = Random.Range(waveBoundsCollider.bounds.min.x + BOUND_OFFSET, waveBoundsCollider.bounds.max.x - BOUND_OFFSET);
                y = waveBoundsCollider.bounds.max.y;
                z = Random.Range(waveBoundsCollider.bounds.min.z + BOUND_OFFSET, waveBoundsCollider.bounds.max.z - BOUND_OFFSET);
                rayDistance = waveBounds.transform.localScale.y;
                Vector3 rayPoint = new Vector3(x, y, z);
                int layer = 9;
                LayerMask layerMask = 1 << layer;
                layerMask = ~layerMask;

                if(Physics.Raycast(rayPoint, transform.TransformDirection(-Vector3.up), out hit, rayDistance))
                {
                    string monsterToSpawn = " ";
                    switch (enemy.enemyType)
                    {
                        case EnemyType.Demogorgon:
                            monsterToSpawn = "Demogorgon";
                            break;
                        case EnemyType.Demobats:
                            monsterToSpawn = "Demobat";
                            break;
                        case EnemyType.Demodogs:
                            monsterToSpawn = "Demodog";
                            break;
                        case EnemyType.Vecna:
                            monsterToSpawn = "Vecna";
                            break;
                    }
                    GameObject E = SpawnManager.SpawnANetworkObject(monsterToSpawn, new Vector3(rayPoint.x, hit.point.y + 3, rayPoint.z), Quaternion.identity);
                    EnemyController eC = E.GetComponent<EnemyController>();
                    monsters.Add(eC);

                }
                
            }

        }

        return monsters;
    }

    private void UpdateMonstersAlive()
    {
        for (int i = 0; i < monstersAlive.Count; i++)
        {
            if(monstersAlive[i] == null)
                monstersAlive.RemoveAt(i);
        }
        /*foreach (EnemyController monster in monstersAlive)
        {
            if(monster == null)
            {
                monstersAlive.Remove(monster);
            }
        }*/
    }

    private void GetChestSpawners()
    {
        chestSpawners = FindObjectsOfType<ChestSpawner>();
    }

    public void VecnaKilled()
    {
        Debug.Log("Vecna Killed");
        KillAllMonsters();
        WinGame();
    }

    private void KillAllMonsters()
    {
        for(int i = 0; i < monstersAlive.Count; i++)
        {
            SpawnManager.DestroyNetworkObject(monstersAlive[i].gameObject);
        }
    }

    private void WinGame()
    { 
        gameManager.WinGame();
        waveType = WaveType.Win;
    }

    private void LoseGame()
    {
        gameManager.LoseGame();
        waveType = WaveType.Lose;
    }


}
