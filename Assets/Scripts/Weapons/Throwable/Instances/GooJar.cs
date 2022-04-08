using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooJar : ThrowableBase
{
    //slowing molotov-ish grenade
    //deals damage on explosion and leaves a putrid cloud that DoT's and slows enemies
    public float explosiveRadius = 2;
    public float explosionDamage = 6;
    public GameObjectPool gooCloudPool;
    private void Start()
    {
        gooCloudPool = GameObject.Find("gooCloudPool").GetComponent<GameObjectPool>();
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
        float playerMultiplier = enemy ? 1f : 0.5f,
        enemyMultiplayer = enemy ? 0.5f : 1f;
        
        GameObject cc = gooCloudPool.GetPooledObject();
        cc.transform.position = transform.position;
        cc.SetActive(true);

        Collider[] cols = Physics.OverlapSphere(transform.position, explosiveRadius);
        foreach (Collider col in cols)
        {
            if (col.TryGetComponent<IDamagePoint>(out IDamagePoint dmg))
            {
                dmg.SendDamage(owner, null, explosionDamage * enemyMultiplayer * DifficultyManager.playerDamageMultiplier);
            }
            if (col.TryGetComponent<PlayerStats>(out PlayerStats ps))
            {
                ps.UpdateDopamine(-explosionDamage * 0.01f * playerMultiplier * DifficultyManager.damageMultiplier);
            }
        }

        Destroy(gameObject);
    }
}
