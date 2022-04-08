using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRofle : WeaponController
{
    [Header("HE Settings")]
    public float explosiveRadius = 1f;
    public float explosiveDamage = 10f;
    public GameObjectPool expl;
    protected override void PrimaryFire()
    {
        PlayShootSound();
        SendDamage();
    }

    protected override void SecondaryFire()
    {

    }

    protected override void SendDamage()
    {
        float spr = weaponStats.weaponSpread;
        Ray ray = cam.ViewportPointToRay(Vector2.one/2 + new Vector2(Random.Range(-spr,spr)/Screen.width, Random.Range(-spr,spr)/Screen.height));
        bool isHit = false;
        LayerMask lm = LayerMask.GetMask("PlayerBarrier");
        if(Physics.Raycast(ray, out RaycastHit hit, weaponStats.range, ~lm))
        {
            if(hit.collider.TryGetComponent<IDamagePoint>(out IDamagePoint ed)) ed.SendDamage(owner, hit.point, GetDamage() * (nextCrit ? 2f : 1f), nextCrit);
            isHit = true;

            Collider[] cols = Physics.OverlapSphere(hit.point, explosiveRadius);
            foreach (Collider col in cols)
            {
                if (col.TryGetComponent<IDamagePoint>(out IDamagePoint dmg))
                {
                     dmg.SendDamage(owner, null, explosiveDamage * DifficultyManager.playerDamageMultiplier, false);
                }
            }
            GameObject e = expl.GetPooledObject();
            e.transform.position = hit.point;
            e.SetActive(true);
        }
        GameObject tracerO = tracers.GetPooledObject();
        LineRenderer tracer = tracerO.GetComponent<LineRenderer>();
        tracer.SetPosition(0, aimMuzzlePoint.position);
        if(isHit) tracer.SetPosition(1, hit.point);
        else tracer.SetPosition(1, aimMuzzlePoint.position + ray.direction * weaponStats.range);
        tracerO.SetActive(true);
    }
}
