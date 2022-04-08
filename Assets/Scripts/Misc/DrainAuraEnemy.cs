using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainAuraEnemy : MonoBehaviour
{
    public PlayerStats ps;
    public Dictionary<EnemyData, float> timers = new Dictionary<EnemyData, float>();
    public float damage;
    public float rate;



    void OnEnable()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        float time = 0;
        while(time < 5)
        {
            transform.localScale = Vector3.one * Random.Range(5.9f,6.1f);
            time += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other) {
        if(!other.TryGetComponent<EnemyData>(out EnemyData ed)) return;
        if(!timers.ContainsKey(ed)) timers.Add(ed, 0);
        timers[ed] += Time.deltaTime;
        if(timers[ed] >= rate)
        {
            ed.SendDamage(null, ed.transform.position, damage);
            ps.UpdateDopamine(damage * 0.001f);
            timers[ed] = 0;
        }
    }
}
