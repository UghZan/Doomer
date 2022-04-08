using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HellSpawn : ThrowableBase
{
    //doom 2016 vampiric grenade

    public float timeUntilSelfDetonate = 3.0f;
    public GameObject drainAura;

    private void Start()
    {
        Invoke("Boom", timeUntilSelfDetonate);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Boom();
        }
    }

    private void Boom()
    {
        GameObject aura = Instantiate(drainAura,transform.position, Quaternion.identity);
        aura.GetComponent<DrainAuraEnemy>().ps = owner;
        aura.SetActive(true);

        Destroy(gameObject);
    }
}
