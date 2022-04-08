using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HRRocket : MonoBehaviour
{
    public float speed;
    public float explosiveRadius;
    public float explosionDamage;
    GameObjectPool explosion;
    public PlayerStats owner;

    // Start is called before the first frame update
    void Start()
    {
        explosion = GameObject.Find("miniExplPool").GetComponent<GameObjectPool>();
    }
    void OnEnable()
    {
        StartCoroutine(DisableAfterAWhile());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Boom();
        }
    }
    void Boom()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, explosiveRadius);
        foreach (Collider col in cols)
        {
            if (col.TryGetComponent<IDamagePoint>(out IDamagePoint dmg)) dmg.SendDamage(owner, null, explosionDamage * DifficultyManager.playerDamageMultiplier);
        }
        GameObject e = explosion.GetPooledObject();
        e.transform.position = transform.position;
        e.SetActive(true);
        gameObject.SetActive(false);
    }

    IEnumerator DisableAfterAWhile()
    {
        yield return new WaitForSeconds(7f);
        Boom();
    }
}
