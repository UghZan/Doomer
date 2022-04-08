using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponController
{
    [Header("Shotgun Settings")]
    public int pellets;
    public bool perAmmoReload;
    protected override void PrimaryFire()
    {
        PlayShootSound();
        for(int i = 0; i < pellets; i++) SendDamage();
    }

    protected override void SecondaryFire()
    {

    }

    protected override void Reload() {if(perAmmoReload) StartCoroutine(ShotgunReloading()); else StartCoroutine(Reloading()); }

    IEnumerator ShotgunReloading()
    {
        reloadInProgress = true;
        wmc.reloading = true;
        yield return new WaitForSeconds(weaponStats.reloadTime);
        currentAmmo++;
        reloadInProgress = false;
        wmc.reloading = false;
    }
}
