using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rofle : WeaponController
{
    [Header("Pierce Settings")]
    public int maxPierces = 3;
    public float damageLossPerPierce = 0.15f; //n% less damage per pierce

    public AudioSource pingSource;
    protected override void PrimaryFire()
    {
        PlayShootSound();
        SendDamage();
        if(currentAmmo == 1) pingSource.Play();
    }

    protected override void SecondaryFire()
    {

    }

    protected override void SendDamage()
    {
        //piercimg 
        float spr = weaponStats.weaponSpread;
        Ray ray = cam.ViewportPointToRay(Vector2.one/2);
        LayerMask lm = LayerMask.GetMask("PlayerBarrier");
        RaycastHit[] enemies = Physics.RaycastAll(ray, weaponStats.range, ~lm);
        System.Array.Sort(enemies, (x,y) => x.distance.CompareTo(y.distance));

        GameObject tracerO = tracers.GetPooledObject();
        LineRenderer tracer = tracerO.GetComponent<LineRenderer>();
        tracer.SetPosition(0, aimMuzzlePoint.position);
        if(enemies.Length != 0) tracer.SetPosition(1, enemies[enemies.Length-1].point);
        else tracer.SetPosition(1, aimMuzzlePoint.position + transform.forward * weaponStats.range);
        tracerO.SetActive(true);
        
        int pierces = 0;
        float damageMul = 1;
        for(int i = 0; i < enemies.Length;i++)
        {
            if(pierces >= maxPierces) break;
            if(enemies[i].collider.TryGetComponent<IDamagePoint>(out IDamagePoint id)) {
                if(id is DamagePoint && pierces > 0) continue; //or it would be too easy to kill armored opponents
                id.SendDamage(owner, enemies[i].point, GetDamage() * damageMul);
                pierces++;
                damageMul -= damageLossPerPierce;
            }
            else break;
        }

        if(pierces > 1) owner.UpdateDopamine((pierces-1) * 0.01f, true);
    }
}
