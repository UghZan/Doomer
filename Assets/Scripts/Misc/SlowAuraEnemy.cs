using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowAuraEnemy : MonoBehaviour
{
    
    public float slowFactor;
    void OnTriggerEnter(Collider other)
    { 
        if(other.TryGetComponent<EnemyData>(out EnemyData ed)) ed.speedMultiplier = slowFactor;
    }

    void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent<PlayerStats>(out PlayerStats ps)) ps.UpdateGooStatus(1f);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent<EnemyData>(out EnemyData ed)) ed.speedMultiplier = 1.0f;
    }
}
