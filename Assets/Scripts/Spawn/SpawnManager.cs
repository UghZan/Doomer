using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnManager : MonoBehaviour
{
    string[] weapons = {"pistol", "smg", "shotgun", "rofle"};
    string[] weaponsNext = {"pistol", "smg", "shotgun", "rofle", "heavyRifle", "autoShotgun", "battleRofle"};
    string[] grenades = {"goojar", "shrapnel","hellspawn","booster"};
    [Header("Spawn Points")]
    public Transform[] normalEnemySpawnPoints;
    public Transform[] bossSpawnPoints;
    public Transform[] pickupSpawnPoints;
    public Transform[] sniperSpawnPoints;

    [Header("Spawn Settings")]
    public Wave[] waves;
    public float spawnDelay; //delay between monster spawns
    public float interwaveTime; //how much time to wait between changing waves

    [Header("Misc Settings & Info")]
    public int difficulty;
    public int currentWave = 0;
    public int enemiesLeft;
    public bool canSpawn = true;
    public static bool startSpawning = false; //used to delay spawning to after tutorial
    public bool interwaveInProgress;
    public float timer {get; private set;}
    public float timer2 {get; private set;}
    public bool noTimeLimit {get; private set;}
    public MusicManager mm;

    //TODO probably needs it own script, but don't have time
    [Header("Misc Settings & Info")]
    public GameObject helicopters;
    public GameObject tutorial;
    Dictionary<string, GameObjectPool> pools;

    public static UnityEvent<int> OnPickupSpawn = new UnityEvent<int>(); //int is for signifying pickup type
    // Start is called before the first frame update
    void Start()
    {
        InitPools();
    }

    void InitPools()
    {
        pools = new Dictionary<string, GameObjectPool>();
        //guaranteed to be used
        for(int i = 0; i < weaponsNext.Length;i++)
        {
            if(i < grenades.Length)
                pools.Add(grenades[i], GameObject.Find(grenades[i] + "Pool").GetComponent<GameObjectPool>());
            pools.Add(weaponsNext[i], GameObject.Find(weaponsNext[i] + "Pool").GetComponent<GameObjectPool>());
        }

        //wave specific, if we won't spawn some enemies - don't add them
        foreach (Wave wave in waves)
        {
            foreach (WaveEntry we in wave.spawns)
            {
                if (!pools.ContainsKey(we.pool)) pools.Add(we.pool, GameObject.Find(we.pool + "Pool").GetComponent<GameObjectPool>());
            }
            foreach (WaveEntry we in wave.bonuses)
            {
                if (!pools.ContainsKey(we.pool)) pools.Add(we.pool, GameObject.Find(we.pool + "Pool").GetComponent<GameObjectPool>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startSpawning)
        {
            MusicManager.start = true;
            tutorial.SetActive(false);
            if (timer > 0) timer -= Time.deltaTime;

            if (canSpawn && !interwaveInProgress)
            {
                if ((enemiesLeft == 0 && noTimeLimit) || ((timer <= 0 || enemiesLeft == 0) && !noTimeLimit))
                {
                    interwaveInProgress = true;
                    if(currentWave == 1) //after first wave
                    {
                        SpawnRandomWeapon(false);
                    }
                    else if (currentWave == 2) //after second wave
                    {
                        SpawnRandomGrenade();
                    }
                    else if(currentWave > 2 && currentWave < 5)
                    {
                        if(Random.value < 0.6) SpawnRandomWeapon(false);
                        else SpawnRandomGrenade();
                    }
                    else if(currentWave >= 5)
                    {
                        for(int i = 0; i < 2; i++)
                        {
                            if(Random.value < 0.75) SpawnRandomWeapon(true);
                            else SpawnRandomGrenade();
                        }
                    }

                    mm.interWave = true;
                    //music stuff
                    if(currentWave == 4)
                    {
                        mm.interWave = false;
                        mm.StopCurrent();
                        mm.nextMusic = mm.GetBossMusic(0);
                        mm.setNext = true;
                    }
                    else if(currentWave == 5)
                    {
                        mm.StopCurrent();
                    }
                    else if(currentWave == 10)
                    {
                        mm.StopCurrent();
                        mm.shouldChange = false;
                    }
                    
                    if(currentWave == 4)
                    {
                        timer2 = 41;
                    }
                    else if(currentWave == 5 || currentWave == 9)
                    {
                        timer2 = 40;
                    }
                    else
                        timer2 = interwaveTime;
                }
            }
            if (interwaveInProgress)
            {
                timer2 -= Time.deltaTime;
                if (timer2 <= 0)
                {
                    mm.interWave = false;
                    interwaveInProgress = false;
                    StartCoroutine(SpawnWave());
                }
            }
        }
    }
    IEnumerator SpawnWave()
    {
        canSpawn = false;
        timer = waves[currentWave].timeLimit;
        noTimeLimit = (timer == -1);
        for (int i = 0; i < waves[currentWave].spawns.Length; i++)
        {
            WaveEntry we = waves[currentWave].spawns[i];
            int spawns = Mathf.CeilToInt(Random.Range(we.minEnemies, we.maxEnemies + 1) * DifficultyManager.spawnMultiplier);
            for (int j = 0; j < spawns; j++)
            {
                Transform spawnPoint;
                if (we.isBoss) spawnPoint = bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)];
                else if(we.isSniper) spawnPoint = sniperSpawnPoints[Random.Range(0, sniperSpawnPoints.Length)];
                else spawnPoint = normalEnemySpawnPoints[Random.Range(0, normalEnemySpawnPoints.Length)];

                GameObject enemy = pools[we.pool].GetPooledObject();
                enemy.GetComponent<EnemyData>().sm = this;
                if(enemy.TryGetComponent<FloppaAI>(out FloppaAI ai))
                {
                    float rand = Random.value;
                    if(rand < 0.5)
                    {
                        ai.rank = FloppaAI.FloppaRank.PRIVATE;
                    }
                    else if(rand < 0.75)
                    {
                        ai.rank = FloppaAI.FloppaRank.CORPORAL;
                    }
                    else if(rand < 0.9)
                    {
                        ai.rank = FloppaAI.FloppaRank.SERGEANT;
                    }
                    else
                    {
                        ai.rank = FloppaAI.FloppaRank.LIETENAUNT;
                    }
                }
                enemy.transform.position = spawnPoint.position;
                enemy.SetActive(true);
                enemiesLeft++;
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        if(currentWave == 5) StartCoroutine(HelicopterCutscene());
        currentWave++;
        canSpawn = true;
    }

    IEnumerator HelicopterCutscene()
    {
        helicopters.SetActive(true);
        yield return new WaitForSeconds(30f);
        helicopters.SetActive(false);
        yield return null;
    }

    void SpawnRandomWeapon(bool expanded)
    {
        if(expanded)
            SpawnPickup(weaponsNext[Random.Range(0,weaponsNext.Length)]);
        else
            SpawnPickup(weapons[Random.Range(0,weapons.Length)]);
        OnPickupSpawn.Invoke(0);
    }

    void SpawnRandomGrenade()
    {
        SpawnPickup(grenades[Random.Range(0,grenades.Length)]);
        OnPickupSpawn.Invoke(1);
    }
    void SpawnPickup(string pickupKey)
    {
        GameObject pickup = pools[pickupKey].GetPooledObject();
        pickup.transform.position = pickupSpawnPoints[Random.Range(0,pickupSpawnPoints.Length)].position + Vector3.up;
        pickup.SetActive(true);
    }
}

