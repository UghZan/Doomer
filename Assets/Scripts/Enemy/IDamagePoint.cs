using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagePoint
{
    public void SendDamage(PlayerStats source, Vector3? pos, float damage, bool crit = false);
}
