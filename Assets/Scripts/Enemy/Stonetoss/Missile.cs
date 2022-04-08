using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public GameObject visualAmogus;
    public static Transform player;
    public float passivePhaseLength;
    public float activeSeekPhaseLength;
    public float moveSpeed;
    public float rotSpeed;
    public GameObjectPool explosion;
    EnemyData ed;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        explosion = GameObject.Find("explPool").GetComponent<GameObjectPool>();
        if(player == null) player = GameObject.Find("Player").transform;
        ed = GetComponent<EnemyData>();
        ed.OnEnemyDeath.AddListener(Death);
    }

    void OnEnable()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        visualAmogus.transform.Rotate(Vector3.forward*2);
        timer += Time.deltaTime;
        if(timer < passivePhaseLength)
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime * 0.75f * DifficultyManager.speedMultiplier);
        else if(timer < passivePhaseLength + activeSeekPhaseLength)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.position - transform.position), Time.deltaTime * rotSpeed);
            transform.Translate(Vector3.forward * moveSpeed*Time.deltaTime * DifficultyManager.speedMultiplier);
        }
        else
        {
            transform.Translate(Vector3.forward * moveSpeed*Time.deltaTime * 1.5f * DifficultyManager.speedMultiplier);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(!other.CompareTag("Enemy"))
        {
            if(other.CompareTag("Player"))
                other.GetComponent<PlayerStats>().UpdateDopamine(-ed.dopamineDrainOnAttack * DifficultyManager.damageMultiplier, true);
            GameObject e = explosion.GetPooledObject();
            e.transform.position = transform.position;
            e.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    void Death()
    {
        GameObject e = explosion.GetPooledObject();
        e.transform.position = transform.position;
        e.SetActive(true);
        gameObject.SetActive(false);
    }
}
