using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooProjectile : MonoBehaviour
{
    public GameObjectPool amogus;
    public ParticleSystem splooge;
    EnemyData ed;

    public float timeUntilBoom;

    float timer;
    bool primed;
    // Start is called before the first frame update
    void Start()
    {
        amogus = GameObject.Find("googusPool").GetComponent<GameObjectPool>();
        ed = GetComponent<EnemyData>();
        ed.OnEnemyDeath.AddListener(Death);
    }


    private void OnEnable() {
        timer = timeUntilBoom;
    }
    // Update is called once per frame
    void Update()
    {
        ed.dopamineGainMultiplier = primed ? 1f : 2f;
        if(primed) timer -= Time.deltaTime;
        if(timer <= 0 && primed) Boom();
    }

    void Boom()
    {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            splooge.Play();
            GameObject amog = amogus.GetPooledObject();
            amog.transform.position = new Vector3(transform.position.x,0,transform.position.z);
            StartCoroutine(StopAfterAWhile());
            amog.SetActive(true);
            primed = false;
    }

    private void OnCollisionEnter(Collision other) {
        if(other.transform.CompareTag("Ground") && timer > 0)
        {
            primed = true;
        }
        else if(other.transform.CompareTag("Player"))
        {
            other.transform.GetComponent<PlayerStats>().UpdateGooStatus(12);
            gameObject.SetActive(false);
        }
            
    }

    IEnumerator StopAfterAWhile()
    {
        yield return new WaitForSeconds(3.0f);
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<MeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }

    void Death()
    {
        primed = false;
        timer = 0;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(StopAfterAWhile());
        splooge.Play();
    }
}
