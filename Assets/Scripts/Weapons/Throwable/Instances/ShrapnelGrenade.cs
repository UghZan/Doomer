using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelGrenade : ThrowableBase
{
    //basically it works like a real life grenade
    //the explosion itself isn't that powerful, however the shrapnel (raycasts) will fuck them up good
    public float timeUntilSelfDetonate = 3.0f;
    public int shrapnels = 50;
    public int shrapnelDamage = 20;
    public int shrapnelDistance = 15;
    public float explosiveRadius = 2;
    public float explosionDamage = 10;
    public GameObjectPool explosivePool;
    public GameObjectPool tracers;

    private void Start()
    {
        explosivePool = GameObject.Find("explPool").GetComponent<GameObjectPool>();
        tracers = GameObject.Find("tracerPool").GetComponent<GameObjectPool>();
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
        float playerMultiplier = enemy ? 1f : 0.5f,
        enemyMultiplayer = enemy ? 0.5f : 1f;

        GameObject expl = explosivePool.GetPooledObject();
        expl.transform.position = transform.position;
        expl.SetActive(true);
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

        for (int i = 0; i < shrapnels; i++)
        {
            Vector3 rand = Random.insideUnitSphere;
            rand.y *= 0.1f;
            if (Physics.Raycast(transform.position, rand, out RaycastHit hit, shrapnelDistance))
            {
                if (hit.collider.TryGetComponent<IDamagePoint>(out IDamagePoint dmg))
                {
                    if (dmg is EnemyData) explosionDamage *= (dmg as EnemyData).isBoss ? 0.33f : 1.0f; // so that you can't melt bosses with shrapnel
                    dmg.SendDamage(owner, null, shrapnelDamage * enemyMultiplayer * DifficultyManager.playerDamageMultiplier);
                }
                if (hit.collider.TryGetComponent<PlayerStats>(out PlayerStats ps))
                {
                    ps.UpdateDopamine(-shrapnelDamage * 0.001f * playerMultiplier * DifficultyManager.damageMultiplier);
                }
            }
            GameObject tracerO = tracers.GetPooledObject();
            LineRenderer tracer = tracerO.GetComponent<LineRenderer>();
            tracer.SetPosition(0, transform.position);
            if (hit.collider != null) tracer.SetPosition(1, hit.point);
            else tracer.SetPosition(1, transform.position + rand * shrapnelDistance);
            tracerO.SetActive(true);
        }
        Destroy(gameObject);
    }
}
