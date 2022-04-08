using UnityEngine;


[CreateAssetMenu(menuName = "Items/New Weapon")]
public class WeaponItem : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponIcon;
    public string weaponInfo;

    [Header("Weapon Damage Stats")]
    public float damage;
    public float damageVariance; 
    public float critChanceBonus;

    [Header("Weapon Gameplay Vars")]
    public float range;
    public int ammoMax;
    public bool isAutomatic;
    public float shootSpeed;
    public int ammoPerShot = 1;
    public float weaponSpread; //different from recoil, just a basic additive inaccuracy to projectile/raycast
    public float reloadTime;
    [Header("Weapon Visual Parameters")]
    public float muzzleChance;

    [Header("Weapon Recoil Common Parameters")]
    public float recoilIncreaseStep = 0.1f;
    public bool fixedRecoil;
    public float maxRecoil; //maximal shot recoil
    public float minRecoil;
    public float recoilRecoverSpeed = 1f;

    [Header("Weapon Recoil Cam Parameters")]
    public float horizontalCamRecoil = 2;
    public float verticalCamRecoil = 4;
    public float negativeVerticalRecoilReduction = -0.2f; //how much is camera going DOWN on shot, set to zero/negative to make the camera never go down

    [Header("Weapon Recoil Visual Parameters")]
    public float maxVisualRecoil = 1; //maximal value of cam recoil
    public float minVisualRecoil = 0.2f; //minimal strength of cam recoil
    public float visualRecoilRecoverSpeed = 1f;

    [Header("Weapon Recoil Punch Parameters")]
    public float horizontalRotPunch = 0.2f;
    public float verticalRotPunch = 0.1f;
    public float pushBackForce = 1f;
    //public float maxPushBack = -2f;
    public float negativeVerticalReduction = -0.2f;
}