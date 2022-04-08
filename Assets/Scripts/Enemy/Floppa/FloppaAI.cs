using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FloppaAI : MonoBehaviour
{
    public enum FloppaRank
    {
        PRIVATE,
        CORPORAL,
        SERGEANT,
        LIETENAUNT
    }
    public enum State
    {
        IN_POSITION, //in position, ready to shoot
        MOVING, //moving to position
        SHOOTING, //in progress of volley
        RELOADING, //GOTTA RELOAD
        GOING_TO_WOUNDED, //for medics
        HEALING, //for medics
        DOWNED
    }
    public static Transform player;
    [Header("Common Settings")]
    public GameObject[] weapons;
    public GameObject[] fakeWeapons;//spawned after death
    public AudioSource weaponAudio;
    public AudioSource flopperAudio;
    public float medicRangeCheck;
    public GameObject pryaniki;

    [Header("General Settings")]
    public FloppaData preset;
    public FloppaRank rank; //higher rank - more danger

    [Header("Attack Settings")]
    public GameObject usedWeapon;
    public GameObjectPool shots;
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;

    [Header("Grenade Settings")]

    public GameObjectPool grenadePool;

    private bool maySwitchToNextState = true;
    private bool hasSight;
    private float distanceToPlayer;
    private State currentState;
    private State prevState;

    private int ammoInMag;
    private int grenadesOnSelf;
    private float inaccuracyAdditional;
    private float timeInState;

    private bool canShoot;
    private bool canThrowGrenades;
    private Transform medicTarget;
    private bool comingToAWounded;
    private Vector3 nextPosition;
    private LayerMask itself;

    private EnemyData data;
    private NavMeshAgent agent;

    void Start()
    {
        data.OnEnemyHit.AddListener(TryRetreat);
        data.OnEnemyDeath.AddListener(Death);
    }

    void OnEnable()
    {
        Init();
        StartCoroutine(AI());
        if(preset.role == FloppaRole.MEDIC)
            StartCoroutine(MedicCheck());
    }

    void Init()
    {
        transform.rotation = Quaternion.identity;
        if (player == null) player = GameObject.Find("Player").transform;
        if (data == null) data = GetComponent<EnemyData>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        grenadePool = GameObject.Find("shrapnelGrenadePool").GetComponent<GameObjectPool>();
        shots = GameObject.Find("floppaShotPool").GetComponent<GameObjectPool>();

        usedWeapon = weapons[preset.usedWeapon];
        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(i == preset.usedWeapon);
        muzzlePoint = usedWeapon.transform.GetChild(0);
        muzzleFlash = usedWeapon.transform.GetChild(1).GetComponent<ParticleSystem>();
        pryaniki.SetActive(preset.role == FloppaRole.MEDIC);
        StartCoroutine(Recharge());
        StartCoroutine(GrenadeRecharge());
        ammoInMag = preset.maxAmmoInMag;
        grenadesOnSelf = preset.maxGrenadesInBag[(int)rank];

        agent.speed = preset.movementSpeed[(int)rank];

        data.maxHealth = preset.health + preset.hpGainPerRank * (int)rank;
        data.dopamineDrainOnAttack = preset.weaponDamage;

        itself = LayerMask.GetMask("Floppa");
    }
    IEnumerator AI()
    {
        currentState = State.IN_POSITION;
        while (true)
        {
            distanceToPlayer = Vector3.Distance(player.position, transform.position);
            if(Physics.Raycast(transform.position + Vector3.up, (player.position - transform.position).normalized, out RaycastHit hit, preset.maxDistanceFromPlayer, ~itself))
            {
                hasSight = hit.transform.CompareTag("Player");
            }
            switch (currentState)
            {
                case State.IN_POSITION:
                    
                    if (ammoInMag == 0) //if out of ammo, reload
                    {
                        SwitchToState(State.RELOADING);
                        break;
                    }

                    if (hasSight) //in position for attack, so shoot
                    {
                        if(preset.mayUseGrenades[(int)rank])
                        if(distanceToPlayer > preset.minGrenadeDistance && distanceToPlayer < preset.maxGrenadeDistance && Random.value < preset.chanceToThrowGrenade[(int)rank] * (grenadesOnSelf/(preset.maxGrenadesInBag[(int)rank] + 1)))
                        {
                            if(grenadesOnSelf > 0 && canThrowGrenades)
                            {
                                StartCoroutine(ThrowGrenade());
                            }
                            break;
                        }
                        if (canShoot)
                        {
                            SwitchToState(State.SHOOTING);
                            break;
                        }
                    }
                    else if (!hasSight || distanceToPlayer > preset.maxDistanceFromPlayer || distanceToPlayer < preset.minDistanceFromPlayer || agent.remainingDistance != 0) //out of position or lost contact
                    {
                        nextPosition = GetClosestPositionToPlayer();
                        SwitchToState(State.MOVING);
                        break;
                    }
                    else if(comingToAWounded)
                    {
                        SwitchToState(State.GOING_TO_WOUNDED);
                    }

                    break;

                case State.MOVING:
                    if (timeInState % 10 == 0 && Random.value < preset.chanceToShootOnTheMove[(int)rank])
                    {
                        SwitchToState(State.SHOOTING);
                        break;
                    }

                    if(timeInState > 50)
                    {
                        nextPosition = GetClosestPositionToPlayer();
                        SwitchToState(State.MOVING);
                    }

                    if (agent.remainingDistance == 0)
                        SwitchToState(State.IN_POSITION);

                    break;

                case State.SHOOTING:
                    if(!maySwitchToNextState) break;

                    if (ammoInMag == 0)
                    {
                        SwitchToState(State.RELOADING);
                        break;
                    }

                    if (Random.value < preset.chanceToChangePositionAfterShooting[(int)rank])
                    {
                        nextPosition = GetClosestPositionToPlayer();
                        SwitchToState(State.MOVING);
                    }
                    else
                    {
                        SwitchToState(State.IN_POSITION);
                    }
                    break;

                case State.RELOADING:
                    if(!maySwitchToNextState) break;

                    if(ammoInMag == preset.maxAmmoInMag) SwitchToState(State.IN_POSITION);

                    break;

                case State.GOING_TO_WOUNDED:
                    if (agent.remainingDistance < 0.1)
                        SwitchToState(State.HEALING);

                    agent.speed = preset.movementSpeed[(int)rank] * 1.5f * DifficultyManager.speedMultiplier;
                    
                    if (timeInState % 10 == 0 && Random.value < preset.chanceToShootOnTheMove[(int)rank])
                    {
                        SwitchToState(State.SHOOTING);
                        break;
                    }

                    if(!medicTarget.gameObject.activeInHierarchy || medicTarget.GetComponent<EnemyData>().health < 0) SwitchToState(State.IN_POSITION);
                    break;
                case State.HEALING:
                    if(!maySwitchToNextState) break;
                    if(!comingToAWounded) SwitchToState(State.IN_POSITION);
                    break;
                case State.DOWNED:
                    if(!data.isDowned) SwitchToState(State.IN_POSITION);
                    break;
            }

            agent.speed = preset.movementSpeed[(int)rank] * DifficultyManager.speedMultiplier;

            timeInState+=0.1f;

            if(data.isDowned)
                SwitchToState(State.DOWNED);

            while(!maySwitchToNextState)
            { 
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void TryRetreat()
    {
        if(Random.value < preset.chanceToRetreatOnTheHit[(int)rank] && maySwitchToNextState)
        {
            nextPosition = GetAwayFromPlayer();
            SwitchToState(State.MOVING);
        }
    }

    IEnumerator HealUp()
    {
        EnemyData ed = medicTarget.GetComponent<EnemyData>();

        if(data.isDowned) yield break;

        yield return new WaitForSeconds(preset.applicationTime[(int)rank]);

        if(data.isDowned) yield break;

        if(medicTarget.gameObject.activeInHierarchy && ed.health > 0) ed.HealUp(ed.maxHealth * preset.healPercentage[(int)rank]);
        SwitchToState(State.IN_POSITION);
        comingToAWounded = false;
        maySwitchToNextState = true;
    }

    IEnumerator MedicCheck()
    {
        while(true)
        {
            if(data.isDowned) yield return null;
            Collider[] floppers = Physics.OverlapSphere(transform.position, medicRangeCheck, itself, QueryTriggerInteraction.Collide);
            Transform closestWounded = null;
            float closestDistance = 10000;
            foreach(Collider flop in floppers)
            {
                if(flop.TryGetComponent<EnemyData>(out EnemyData ed))
                if((ed.isDowned || ed.health <= ed.maxHealth * 0.1f) && Vector3.Distance(flop.transform.position, transform.position) < closestDistance) 
                { 
                    closestWounded = flop.transform; 
                    closestDistance = Vector3.Distance(flop.transform.position, transform.position);
                }
            }
            
            if(closestWounded != null)
            {
                medicTarget = closestWounded;
                if(currentState != State.HEALING && !data.isDowned) SwitchToState(State.GOING_TO_WOUNDED);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ThrowGrenade()
    {
        canThrowGrenades = false;
        GameObject projectile = grenadePool.GetPooledObject();
        projectile.GetComponent<ThrowableBase>().enemy = true;
        projectile.GetComponent<Rigidbody>().isKinematic = true;
        Vector3 randomPoint = Random.insideUnitSphere;
        randomPoint.y = 0;

        Vector3 point1 = transform.position + transform.forward * 2 + Vector3.up*2;
        Vector3 point3 = player.transform.position - Vector3.up + randomPoint * preset.grenadeThrowAccuracy[(int)rank];
        point3.y = 0;
        Vector3 point2 = Vector3.Lerp(point1,point3, 0.5f) + Vector3.up * Vector3.Distance(point1,point3);

        projectile.transform.position = point1;
        projectile.SetActive(true);


        float t = 0;
        while(t < 1)
        {
		    float oneMinusT = 1f - t;
		    projectile.transform.position = oneMinusT * oneMinusT * point1 + 2f * oneMinusT * t * point2 + t * t * point3;
            t += Time.deltaTime * 0.5f;
            yield return null;
        }
        projectile.GetComponent<Rigidbody>().isKinematic = false;
        grenadesOnSelf--;
    }
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(preset.reloadDelay);
        ammoInMag = preset.maxAmmoInMag;
        SwitchToState(prevState);
        maySwitchToNextState = true;
    }

    IEnumerator ShootVolley()
    {
        canShoot = false;

        Vector2 inaccuracy;
        Vector3 inaccuracy3;
        GameObject shot;
        for (int i = 0; i < preset.shotsInVolley[(int)rank]; i++)
        {
            if (ammoInMag == 0 || data.isDowned) { break; }

            inaccuracy = Random.insideUnitCircle;
            inaccuracy3 = new Vector3(inaccuracy.x, inaccuracy.y, 0);
            shot = shots.GetPooledObject();

            shot.transform.position = muzzlePoint.position;
            shot.transform.rotation = Quaternion.LookRotation(GetShotVector() + inaccuracy3 * (preset.floppaInaccuracy[(int)rank] + inaccuracyAdditional));
            shot.GetComponent<SimpleProjectile>().speed = preset.shotSpeed;
            shot.GetComponent<SimpleProjectile>().dopamineDrain = data.dopamineDrainOnAttack;
            transform.rotation = Quaternion.LookRotation(player.position - transform.position);
            muzzleFlash.Play();
            shot.SetActive(true);
            weaponAudio.PlayOneShot(preset.shootSound);
            ammoInMag--;

            yield return new WaitForSeconds(preset.shootDelay[(int)rank]);
        }
        StartCoroutine(Recharge());
        maySwitchToNextState = true;
    }

    IEnumerator Recharge()
    {
        yield return new WaitForSeconds(preset.timeBetweenVolleys[(int)rank]);
        canShoot = true;
    }

    IEnumerator GrenadeRecharge()
    {
        yield return new WaitForSeconds(10f);
        canThrowGrenades = true;
    }

    //Gives out a vector to player with leading
    Vector3 GetShotVector()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        //https://answers.unity.com/questions/506772/how-do-i-make-an-enemy-lead-his-shots.html
        float t = (player.position - muzzlePoint.transform.position).magnitude / preset.shotSpeed;
        Vector3 futurePos = player.position + cc.velocity * t * preset.targetLeadFactor[(int)rank];

        Vector3 aimVector = (futurePos - muzzlePoint.transform.position).normalized;

        return aimVector;
    }

    Vector3 GetClosestPositionToPlayer()
    {
        float midDistance = Mathf.Lerp(preset.minDistanceFromPlayer, preset.maxDistanceFromPlayer, 0.5f);
        Vector3 midPoint = Vector3.Lerp(player.position, player.position + (transform.position - player.position).normalized * preset.maxDistanceFromPlayer, midDistance / preset.maxDistanceFromPlayer);
        float randomDistance = midDistance-preset.minDistanceFromPlayer;
        Vector3 randomPosition = Random.insideUnitSphere;
        randomPosition.y = 0;

        if (NavMesh.SamplePosition(midPoint + randomPosition * randomDistance, out NavMeshHit hit, 5, NavMesh.AllAreas))
            return hit.position;

        return player.position;
    }

    Vector3 GetAwayFromPlayer()
    {
        Vector3 point = player.position + (transform.position - player.position).normalized * preset.maxDistanceFromPlayer;
        float randomDistance = 5f;
        Vector3 randomPosition = Random.insideUnitSphere;
        randomPosition.y = 0;

        if (NavMesh.SamplePosition(point + randomPosition * randomDistance, out NavMeshHit hit, 5, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void SwitchToState(State nextState)
    {
        maySwitchToNextState = false;

        if(currentState == State.DOWNED && nextState !=State.DOWNED)
        { 
            agent.enabled = true;
            transform.rotation = Quaternion.identity;
            maySwitchToNextState = true;
        }

        //Debug.Log(gameObject.name + " switching to " + nextState);

        prevState = currentState;
        switch (nextState)
        {
            case State.IN_POSITION:
                maySwitchToNextState = true;
                break;
            case State.MOVING:
                inaccuracyAdditional = 0.2f;
                agent.SetDestination(nextPosition);
                maySwitchToNextState = true;
                break;
            case State.SHOOTING:
                inaccuracyAdditional = 0f;
                StartCoroutine(ShootVolley());
                break;
            case State.GOING_TO_WOUNDED:
                agent.SetDestination(medicTarget.transform.position);
                comingToAWounded = true;
                maySwitchToNextState = true;
                break;
            case State.RELOADING:
                StartCoroutine(Reload());
                break;
            case State.HEALING:
                StartCoroutine(HealUp());
                break;
            case State.DOWNED:
                comingToAWounded = false;
                agent.SetDestination(transform.position);
                agent.enabled = false;
                transform.rotation = Quaternion.LookRotation(Vector3.up);
                maySwitchToNextState = true;
                break;
        }
        currentState = nextState;
    }

    void Death()
    {
        currentState = State.DOWNED;
        for(int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }
        pryaniki.SetActive(false);
        GameObject wepFake = Instantiate(fakeWeapons[preset.usedWeapon], usedWeapon.transform.position, usedWeapon.transform.rotation);
        if(pryaniki.activeInHierarchy) Instantiate(fakeWeapons[weapons.Length], pryaniki.transform.position, pryaniki.transform.rotation);
        StartCoroutine(DisableAfterWhile());
    }

    IEnumerator DisableAfterWhile()
    {
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }
}
