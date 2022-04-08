using UnityEngine;


[CreateAssetMenu(menuName = "Items/New Throwable")]
public class ThrowItem : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponIcon;
    public string weaponInfo;

    [Header("Weapon Gameplay Vars")]
    public float throwPower;
    public int rechargeSpeed;
    public int maxOnHand;
    public GameObject thrownObject;
}