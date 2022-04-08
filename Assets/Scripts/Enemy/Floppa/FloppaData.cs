using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/New Floppa Data")]
public class FloppaData : ScriptableObject
{
    public FloppaRole role;

    [Header("Health Settings")]
    public float health;
    public float hpGainPerRank; //health + hpGainPerRank * rank
    [Header("Movement Settings")]
    public float[] movementSpeed;
    public float minDistanceFromPlayer;
    public float maxDistanceFromPlayer;

    [Header("Weaponry Settings")]  
    public AudioClip shootSound;
    public int usedWeapon;
    public float weaponDamage;
    public int maxAmmoInMag; //when floppa is out, he gotta reload
    public int[] maxGrenadesInBag; //how much grenades floppa can carry

    [Header("Attack Settings")]
    public float shotSpeed;
    //public float shootDistance; //how close floppa gotta be
    public float[] shootDelay; //how long floppa takes to shoot ONE shot
    public int[] shotsInVolley; //how much shots floppa will fire, depends on rank
    public float[] timeBetweenVolleys; //delay between volleys (aim time), depends on rank
    public float[] floppaInaccuracy; //shot spread, depends on rank
    public float[] targetLeadFactor; // 1.0 is full lead, 0.0 - no lead

    [Header("Tactics Settings")]
    public bool[] mayUseGrenades;
    public float[] chanceToChangePositionAfterShooting;
    public float[] chanceToShootOnTheMove;
    public float[] chanceToRetreatOnTheHit;

    [Header("Grenade Lob Settings")]
    public float[] chanceToThrowGrenade;
    public float[] grenadeThrowAccuracy; //how accurate the throw will be, 0 is deadset on player, 1 - it will land in random point in radius of 1 etc.
    public float minGrenadeDistance; //if floppa is closer than that, he will not throw
    public float maxGrenadeDistance; //you get the drill

    [Header("Reload Settings")]
    public float reloadDelay;

    [Header("Medic Settings")]
    public float[] healPercentage; //how much hp% he heals
    public float[] applicationTime; //how much he takes to heal a flopper
}


    public enum FloppaRole
    {
        SOLDIER, //m16, few grenades
        SHOTGUNNER, //shotgun, rushes in
        SNIPER, //sniper rifle, stays at long distance
        MEDIC //m16, heals allies
    }
