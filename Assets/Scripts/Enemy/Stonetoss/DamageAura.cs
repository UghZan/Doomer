using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAura : MonoBehaviour
{
    public float damage;
    public float rate;

    float timer;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer >= rate)
            {
                other.GetComponent<PlayerStats>().UpdateDopamine(-damage, true);
                timer = 0;
            }
        }
    }
}
