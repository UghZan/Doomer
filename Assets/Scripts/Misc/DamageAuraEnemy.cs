using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAuraEnemy : MonoBehaviour
{
    public Dictionary<EnemyData, float> timers = new Dictionary<EnemyData, float>();
    public float damage;
    public float rate;

    private void OnTriggerStay(Collider other) {
        if(!other.TryGetComponent<EnemyData>(out EnemyData ed)) return;
        if(!timers.ContainsKey(ed)) timers.Add(ed, 0);
        timers[ed] += Time.deltaTime;
        if(timers[ed] >= rate)
        {
            ed.SendDamage(null, ed.transform.position, damage);
            timers[ed] = 0;
        }
    }
}
