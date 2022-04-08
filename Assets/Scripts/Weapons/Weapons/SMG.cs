using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMG : WeaponController
{
    protected override void PrimaryFire()
    {
        PlayShootSound();
        SendDamage();
    }

    protected override void SecondaryFire()
    {

    }
}
