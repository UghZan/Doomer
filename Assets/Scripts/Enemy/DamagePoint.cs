using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePoint : MonoBehaviour, IDamagePoint
{
    public EnemyData mainEnemy;
    public float damageMultiplier;
    public float dopamineGainBonus;
    public void SendDamage(PlayerStats owner, Vector3? pos, float damage, bool crit = false)
    {
        mainEnemy.SendDamage(owner, pos, damage * damageMultiplier, true);
        if(owner != null) owner.UpdateDopamine(dopamineGainBonus, true);
    }
    
}
