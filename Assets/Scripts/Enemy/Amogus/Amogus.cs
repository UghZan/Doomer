using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Amogus : MonoBehaviour
{
    public static Transform player;
    public bool armorPiercing = false;    
    public AudioSource audio;
    public float chanceToPlaySound = 0.15f;
    public bool gooTouch;
    public bool canJump;
    public float jumpCooldown = 5.0f;
    public float jumpForce = 10f;
    public AudioClip nyeow;
    GameObjectPool deathEffectPool;
    EnemyData data;
    NavMeshAgent agent;
    Rigidbody rb;
    float soundTimer;

    bool isJumping;
    float jumpTimer;
    // Start is called before the first frame update
    void Awake()
    {
        if (player == null) player = GameObject.Find("Player").transform;
        deathEffectPool = GameObject.Find("deathEffectsPool").GetComponent<GameObjectPool>();
        agent = GetComponent<NavMeshAgent>();
        data = GetComponent<EnemyData>();
        data.OnEnemyDeath.AddListener(Death);
        if (!TryGetComponent<Rigidbody>(out rb) && canJump)
        {
            Debug.Log("No RigidBody found, jump functionality unavailable");
            canJump = false;
        }
    }

    void OnEnable()
    {
        StartCoroutine(AI());
    }

    private void Update()
    {
        agent.speed = data.movementSpeed * data.speedMultiplier * DifficultyManager.speedMultiplier;

        if (canJump)
        {
            if (!isJumping)
            {
                if (jumpTimer < jumpCooldown) jumpTimer += Time.deltaTime;
                else StartCoroutine(Jump());
            }
        }
    }

    IEnumerator Jump()
    {
        audio.PlayOneShot(nyeow);
        isJumping = true;
        agent.enabled = false;
        rb.isKinematic = false;

        rb.AddForce((transform.forward + Vector3.up * 0.25f) * jumpForce, ForceMode.VelocityChange);
        data.dopamineGainMultiplier = 2.0f;
        yield return new WaitForSeconds(3.0f);
        data.dopamineGainMultiplier = 1.0f;

        agent.enabled = true;
        rb.isKinematic = true;
        jumpTimer = 0;
        isJumping = false;

        yield return null;
    }

    IEnumerator AI()
    {
        agent.enabled = true;
        if(rb != null) rb.isKinematic = true;
        jumpTimer = 0;
        isJumping = false;

        while (true)
        {
            if (agent.enabled) agent.SetDestination(player.position);

            if(soundTimer % 10 == 0 && Random.value < chanceToPlaySound && !audio.isPlaying)
                audio.PlayOneShot(data.GetIdleSound());

            soundTimer++;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().UpdateDopamine(-data.dopamineDrainOnAttack * DifficultyManager.damageMultiplier, armorPiercing);
            if (gooTouch) other.GetComponent<PlayerStats>().UpdateGooStatus(4);
            data.EnemyCountReduction();
            Death();
        }
    }

    void Death()
    {
        GameObject dE = deathEffectPool.GetPooledObject();
        ParticleSystem.MainModule mm = dE.GetComponent<ParticleSystem>().main;
        mm.startColor = data.hitColor;
        dE.transform.position = transform.position + Vector3.up;
        dE.SetActive(true);
        gameObject.SetActive(false);
    }
}
