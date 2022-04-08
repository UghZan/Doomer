using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyRifle : WeaponController
{
    [Header("Rifle Settings")]
    public Transform rocketPoint;
    public GameObjectPool rocketPool;
    public AudioClip rocketSound;
    public int ammoPerRocket;
    public float rocketDelay;
    float rocketRecharge;
    protected override void PrimaryFire()
    {
        PlayShootSound();
        SendDamage();
    }

    protected override void SecondaryFire()
    {
        if(currentAmmo >= ammoPerRocket && rocketRecharge >= rocketDelay)
        {
            audioSource.PlayOneShot(rocketSound);
            audioSource.pitch = Random.Range(0.9f,1.1f);
            
            GameObject rocket = rocketPool.GetPooledObject();
            rocket.GetComponent<HRRocket>().owner = owner;
            rocket.transform.position = rocketPoint.position;
            rocket.transform.rotation = Quaternion.LookRotation(transform.forward);
            rocket.SetActive(true);
            currentAmmo-=ammoPerRocket;
            wrc.Recoil();
            rocketRecharge = 0;
        }
    }

    protected override void Update()
    {
        base.Update();
        if(rocketRecharge < rocketDelay) rocketRecharge+=Time.deltaTime;
    }
}
