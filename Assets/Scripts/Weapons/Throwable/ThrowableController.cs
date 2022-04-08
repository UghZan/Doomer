using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableController : MonoBehaviour
{
    public PlayerStats playerStats;
    public ThrowItem[] throwables;
    public Transform throwPoint;
    public int currentGrenade = -1;
    public AudioSource aS;
    public AudioClip throwSound;
    public AudioClip boosterSound;

    public float rechargeTimer;
    public int grenadesOnHand;

    public ThrowItem GetCurrentThrowable()
    {
        return throwables[currentGrenade];
    }

    // Update is called once per frame
    void Update()
    {
        if(currentGrenade >= 0)
            if(grenadesOnHand < throwables[currentGrenade].maxOnHand)
            { 
                rechargeTimer+=Time.deltaTime;
                if(rechargeTimer >= throwables[currentGrenade].rechargeSpeed) { grenadesOnHand++; rechargeTimer = 0;}
            }

        if(Input.GetKeyDown(KeyCode.LeftAlt))
            Throw();
    }

    public void UpdateEquippedThrowable(int next)
    {
        currentGrenade = next;
        grenadesOnHand = throwables[currentGrenade].maxOnHand;
    }

    public void Throw()
    {
        if(grenadesOnHand == 0 || currentGrenade == -1) return;
        if(currentGrenade == 2)
        {
            aS.PlayOneShot(boosterSound);
            playerStats.UpdateDopamine(0.5f,false);
        }
        else
        {
            aS.PlayOneShot(throwSound);
            GameObject grenade = Instantiate(throwables[currentGrenade].thrownObject, throwPoint.position, Quaternion.identity);
            grenade.GetComponent<ThrowableBase>().owner = playerStats;
            grenade.GetComponent<ThrowableBase>().enemy = false;
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            rb.AddForce((transform.forward + Vector3.up * 0.5f) * throwables[currentGrenade].throwPower, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * throwables[currentGrenade].throwPower*3);
        }
        grenadesOnHand--;
    }
}
