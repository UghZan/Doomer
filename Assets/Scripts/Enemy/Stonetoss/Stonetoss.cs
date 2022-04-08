using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stonetoss : MonoBehaviour
{
    public static Transform player;
    public EnemyData data;
    [Header("Movement Options")]
    public float moveSpeed;
    public float rotSpeed;
    public float minXRange;
    public float maxXRange;
    public float minZRange;
    public float maxZRange;
    public float flyHeight = 15;

    [Header("Audio Options")]
    public AudioSource commentSource;
    public AudioSource attacksSource;
    public AudioClip intro;

    [Header("Immunity Options")]
    public float armorNormal = 0.5f;
    public float armorAttack = 1f;
    public float armorAfterAttack = 1.25f;

    [Header("Cooldown Options")]
    public float timeForSpawn;
    public float timeForVolley;
    public float timeForMissiles;

    [Header("Spawn Attack Options")]
    public int minGlobs = 2;
    public int maxGlobs = 6;
    public GameObjectPool globsPool;
    public Vector3 spawnPointOffset_glob;
    public float throwPower = 10;
    public AudioClip goo;

    [Header("Volley Attack Options")]
    public float attackHeatupTime = 2f;
    public float attackDuration = 5f;
    public GameObjectPool amogusShotPool;
    public float minShotSpeed = 5f;
    public float maxShotSpeed = 12f;
    public float spawnSpread = 2f;
    public float maxDeviation = 2f;
    public float shootFrequency = 2f;
    public AudioClip warmup;
    public AudioClip attack;

    [Header("Missile Attack Options")]
    public float duration = 10f;
    public float frequency = 0.5f;
    public GameObjectPool missiles;
    bool whichEye;
    public Transform[] eyes;
    public AudioClip shootMissile;

    [Header("Low HP Effect Options")]
    public ParticleSystem lowHPDrip;
    public GameObject visualStonetoss;
    public AudioClip holyFuck;
    public float lowHPShakeFrequency;
    public float lowHPShakePower;
    public GameObject gooExplosion;
    public GameObject gBeams;
    public GameObject finalExplosion;
    public GameObject finalSound;
    public float onDeathExplosionsCount = 5;

    float timer = 0;
    float timer2 = 0;
    float timer3 = 0;

    int lowHPStage = 0; //on 50% some drip will appear and on 25% he will start to shake
    bool missileSpeen;
    float speenPowa;
    bool attackInProgress;
    bool stopMoving;

    bool inited;

    bool dead;
    Vector3 target;
    // Start is called before the first frame update
    void OnEnable()
    {
        if(inited)
        { 
            CameraShakeController.instance.Shake(5,1);
            attacksSource.PlayOneShot(intro);
        }
        data.OnEnemyDeath.AddListener(Death);
        timer = timeForSpawn;
        timer2 = timeForVolley;
        timer3 = timeForMissiles;

        if (player == null) player = GameObject.Find("Player").transform;
        data = GetComponent<EnemyData>();
        globsPool = GameObject.Find("goodropPool").GetComponent<GameObjectPool>();
        amogusShotPool = GameObject.Find("amongusProjPool").GetComponent<GameObjectPool>();
        missiles = GameObject.Find("missilePool").GetComponent<GameObjectPool>();

        GetNextWaypoint();

        lowHPStage = 0;
        if (lowHPDrip.isPlaying) lowHPDrip.Stop();

        transform.position = new Vector3(transform.position.x, flyHeight, transform.position.z);
        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            timer -= Time.deltaTime * (attackInProgress ? 0.15f : 1);
            timer2 -= Time.deltaTime * (attackInProgress ? 0.15f : 1);
            timer3 -= Time.deltaTime * (attackInProgress ? 0.15f : 1);

            if (!attackInProgress)
            {
                data.armorMultiplier = armorNormal;
                if (timer <= 0)
                {
                    StartCoroutine(SpawnAttack());
                    attackInProgress = true;
                }
                if (timer2 <= 0)
                {
                    StartCoroutine(VolleyAttack());
                    attackInProgress = true;
                }
                if (timer3 <= 0)
                {
                    StartCoroutine(MissileAttack());
                    attackInProgress = true;
                }
            }

            if (data.health <= data.maxHealth * 0.5f && lowHPStage == 0)
            {
                lowHPDrip.Play();
                lowHPStage = 1;
            }
            else if (data.health <= data.maxHealth * 0.25f && lowHPStage == 1)
            {
                StartCoroutine(Shake());
                lowHPStage = 2;
            }

            if (!stopMoving)
            {
                if (Vector3.Distance(transform.position, target) > 4f)
                {
                    transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed * (lowHPStage+1) * DifficultyManager.speedMultiplier);
                }
                else
                {
                    GetNextWaypoint();
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target - transform.position, Vector3.up), Time.deltaTime * rotSpeed);
            }

            if (missileSpeen)
            {
                if (speenPowa < 6) speenPowa += Time.deltaTime * 4;
                transform.Rotate(Vector3.up, speenPowa * Time.deltaTime * 8);
            }
            else
            {
                if (speenPowa > 0) { speenPowa -= Time.deltaTime * 4; transform.Rotate(Vector3.up, speenPowa * Time.deltaTime * 8); }
            }
        }
    }

    void GetNextWaypoint()
    {
        target = new Vector3(Random.Range(minXRange, maxXRange), flyHeight, Random.Range(minZRange, maxZRange));
    }

    IEnumerator Shake()
    {
        commentSource.clip = holyFuck;
        commentSource.Play();
        while (true)
        {
            visualStonetoss.transform.rotation = Quaternion.LookRotation((transform.forward + Random.insideUnitSphere) * Random.Range(-lowHPShakePower, lowHPShakePower));
            yield return new WaitForSeconds(lowHPShakeFrequency);
        }
    }

    IEnumerator SpawnAttack()
    {
        int globs = Random.Range(minGlobs, maxGlobs + 1) + lowHPStage * 2;
        for (int i = 0; i < globs; i++)
        {
            attacksSource.PlayOneShot(goo);
            attacksSource.pitch = Random.Range(0.9f,1.1f);
            Vector2 offsetXY = Random.insideUnitCircle;
            Vector3 offset = new Vector3(offsetXY.x, 0, offsetXY.y);
            GameObject glob = globsPool.GetPooledObject();
            glob.transform.position = transform.position + spawnPointOffset_glob;
            glob.SetActive(true);
            glob.GetComponent<Rigidbody>().AddForce((Vector3.up + offset) * throwPower, ForceMode.Impulse);
            yield return new WaitForSeconds(0.6f - lowHPStage * 0.1f);
        }
        timer = timeForSpawn * Random.Range(0.9f, 1.33f) - lowHPStage * 0.5f;
        attackInProgress = false;
        yield return null;
    }

    IEnumerator VolleyAttack()
    {
        stopMoving = true;
        data.armorMultiplier = armorAttack;
        float timeUntilAttack = lowHPStage;

        attacksSource.PlayOneShot(warmup);

        while (timeUntilAttack < attackHeatupTime)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.position - transform.position), Time.deltaTime * rotSpeed * 30f);
            timeUntilAttack += Time.deltaTime;
            yield return null;
        }
        float timeOfAttack = lowHPStage;
        float charge = 0;
        attacksSource.Stop();
        attacksSource.PlayOneShot(attack);

        while (timeOfAttack < attackDuration)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.position - transform.position), Time.deltaTime * rotSpeed * 70f);
            if (charge >= 1f - lowHPStage * 0.1f)
            {
                GameObject shot = amogusShotPool.GetPooledObject();
                Vector3 deviationVector = Random.insideUnitCircle;
                shot.transform.position = transform.position + transform.forward * 5 + Random.insideUnitSphere * spawnSpread;
                shot.GetComponent<SimpleProjectile>().speed = Random.Range(minShotSpeed, maxShotSpeed);
                deviationVector = Random.insideUnitCircle;
                shot.transform.rotation = Quaternion.LookRotation(transform.forward + deviationVector * maxDeviation);
                shot.SetActive(true);
            }
            charge += Time.deltaTime * shootFrequency;
            timeOfAttack += Time.deltaTime;
            yield return null;
        }

        attacksSource.Stop();

        data.armorMultiplier = armorAfterAttack;
        yield return new WaitForSeconds(3f - lowHPStage);
        timer2 = timeForVolley * Random.Range(0.9f, 1.33f) - lowHPStage;
        attackInProgress = false;
        stopMoving = false;
        yield return null;

    }

    IEnumerator MissileAttack()
    {
        missileSpeen = true;
        stopMoving = true;
        float timeLeft = 0;
        data.armorMultiplier = armorAttack;
        while (timeLeft < duration)
        {
            attacksSource.PlayOneShot(shootMissile);
            attacksSource.pitch = Random.Range(0.9f,1.1f);
            Vector3 spawnPos = whichEye ? eyes[0].position : eyes[1].position;
            GameObject missile = missiles.GetPooledObject();
            missile.transform.position = spawnPos;
            missile.transform.rotation = Quaternion.LookRotation(transform.forward);
            missile.SetActive(true);
            timeLeft += frequency;
            whichEye = !whichEye;
            yield return new WaitForSeconds(frequency);
        }
        data.armorMultiplier = armorAfterAttack;
        yield return new WaitForSeconds(3f - lowHPStage);
        timer3 = timeForMissiles * Random.Range(0.9f, 1.33f) - lowHPStage;
        missileSpeen = false;
        stopMoving = false;
        attackInProgress = false;
        yield return null;
    }

    void Death()
    {
        dead = true;
        StopAllCoroutines();
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        Instantiate(gBeams, transform.position, Quaternion.identity);
        int explosions = 0;
        while(explosions < onDeathExplosionsCount)
        {
            explosions++;
            GameObject e = Instantiate(gooExplosion, transform.position + Random.onUnitSphere * 4, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0.7f,1.5f));
        }
        Instantiate(finalSound, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        CameraShakeController.instance.Shake(6,2);
        Instantiate(finalExplosion, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
