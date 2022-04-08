using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    public float speed;
    public float dopamineDrain;
    public float disableTime = 7f;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(DisableAfterAWhile());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime * DifficultyManager.speedMultiplier);
    }

    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().UpdateDopamine(-dopamineDrain * DifficultyManager.damageMultiplier,true);
        }
        if(!other.CompareTag("Enemy"))
            gameObject.SetActive(false);
    }

    IEnumerator DisableAfterAWhile()
    {
        yield return new WaitForSeconds(disableTime);
        gameObject.SetActive(false);
    }
}
