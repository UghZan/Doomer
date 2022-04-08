using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public enum PickupType
    {
        WEAPON,
        GRENADE
    }
    public PickupType type;
    public int weapon;
    public float disableTimer = 30f;


void OnEnable()
{
    StartCoroutine(DisableAfterAWhile());
}
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 0.5f);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player"))
        {
            if(type == PickupType.WEAPON)
                other.GetComponent<PlayerWeaponController>().ChangeWeapon(weapon);
            else if(type == PickupType.GRENADE)
                other.GetComponent<PlayerWeaponController>().ChangeGrenade(weapon);
            gameObject.SetActive(false);
        }
    }

    IEnumerator DisableAfterAWhile()
    {
        yield return new WaitForSeconds(disableTimer);
        gameObject.SetActive(false);
    }
}
