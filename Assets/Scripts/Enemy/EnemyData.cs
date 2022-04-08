using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyData : MonoBehaviour, IDamagePoint
{
    public static HitEffectController hec;
    [Header("Health Settings")]
    public float chanceToGetDepressed; //per tick
    public bool isDepressed;
    public float maxHealth;
    public float armorMultiplier = 1.0f; //less multiplier = less damage

    [Header("Dopamine Settings")]
    public float dopamineDrainOnAttack;
    public float dopamineGainOnKill;
    public float dopamineGainMultiplier = 1.0f;

    [Header("Movement Settings")]
    public float speedMultiplier = 1.0f;
    public float movementSpeed;

    [Header("Audio/Visual Settings")]
    public Color hitColor;
    public string hitEffectPoolName;

    //TODO (if ever) - outsource it to another script, EnemyData is a bit overloaded already
    public AudioClip[] idleSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] attackSounds;
    
    [Header("Misc Settings")]
    public bool isBoss;
    public bool canGoDown; //if true, enemy will first go in "downed" state and then will be able to be killed
    public bool isDowned;
    public bool countsTowardsEnemyCount = true;//does it count towards spawn manager enemy count? used for boss minions

    GameObjectPool hitEffect;
    PlayerStats lastDamagerStat;
    bool dead;
    float cachedArmor;
    public SpawnManager sm;

    public UnityEvent OnEnemyDeath = new UnityEvent();
    public UnityEvent OnEnemyHit = new UnityEvent();
    public UnityEvent OnEnemyDowned = new UnityEvent();

    public float health {get; private set;}

    void Start()
    {
        if(hec == null) hec = GameObject.Find("Manager").GetComponent<HitEffectController>();
        if(hitEffectPoolName != "") hitEffect = GameObject.Find(hitEffectPoolName).GetComponent<GameObjectPool>();
        cachedArmor = armorMultiplier;
    }
    public void OnEnable()
    {
        if(isDowned) armorMultiplier = cachedArmor;
        isDowned = false;
        dead = false;
        health = maxHealth;
    }

    public void SendDamage(PlayerStats source, Vector3? pos, float damage, bool crit = false)
    {
        lastDamagerStat = source;
        health -= damage * armorMultiplier;
        
        if(crit || armorMultiplier > 1.0f) HitEffectController.OnCrit.Invoke();
        else if(!crit || (armorMultiplier > 0.5f && armorMultiplier < 1.0f)) HitEffectController.OnHit.Invoke();
        
        if(pos == null)
            pos = transform.position;

        if(hitEffect != null) SpawnHitEffect((Vector3)pos, hitColor);

        //rally system
        if(source != null)
            if(!source.DopamineRallyRegained()) source.UpdateDopamine(damage * 0.005f);

        if (health <= 0 && !dead)
        {
            if(!canGoDown || (canGoDown && isDowned))
            {
                dead = true;
                OnEnemyDeath.Invoke();
                EnemyCountReduction();
                if(source != null) source.UpdateDopamine(dopamineGainOnKill * dopamineGainMultiplier,true);
            }
            else if(canGoDown && !isDowned)
            {
                OnEnemyDowned.Invoke();
                isDowned = true;
                cachedArmor = armorMultiplier;
                armorMultiplier = 0.25f;
                health = 5f;
            }
        }
    }

    public void HealUp(float newHealth)
    {
        health += Mathf.Clamp(newHealth,0,maxHealth);
        armorMultiplier = cachedArmor;
        isDowned = false;
    }

    public void EnemyCountReduction()
    {
        if(countsTowardsEnemyCount && sm != null) sm.enemiesLeft--;
    }

    void SpawnHitEffect(Vector3 pos, Color color)
    {
        GameObject hE = hitEffect.GetPooledObject();
        ParticleSystem.MainModule mm = hE.GetComponent<ParticleSystem>().main;
        mm.startColor = hitColor;
        hE.transform.position = pos;
        hE.SetActive(true);
    }

    void Update()
    {
        if(isDowned)
        {
            health += Time.deltaTime * 0.1f;
            if(health > 10)
            {
                HealUp(0.1f);
            }
        }
    }

    public AudioClip GetIdleSound()
    {
        return idleSounds[Random.Range(0, idleSounds.Length)];
    }

    public AudioClip GetHitSound()
    {
        return hitSounds[Random.Range(0, idleSounds.Length)];
    }

    public AudioClip GetAttackSound()
    {
        return attackSounds[Random.Range(0, idleSounds.Length)];
    }
}
