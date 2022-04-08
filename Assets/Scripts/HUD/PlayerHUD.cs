using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Main Face UI")]
    public Sprite[] faces; //0 - death, 4 - based
    public Image face;
    public Image scale;
    public Image scalePointer;
    public Image scalePointerRally;

    [Header("Primary Weapon UI")]
    public Image primarySlot;
    public TextMeshProUGUI primaryName;
    public TextMeshProUGUI ammoCount;

    [Header("Secondary Weapon UI")]
    public GameObject grenadeSlot;
    public Image secondarySlot;
    public TextMeshProUGUI secondaryName;
    public TextMeshProUGUI secondaryItems;
    public Slider secondaryRecharge;

    [Header("Dash UI")]
    public Slider dashRecharge;

    [Header("Pickup Info UI")]
    public TextMeshProUGUI pickupName;
    public TextMeshProUGUI pickupInfo;

    [Header("Wave Info UI")]
    public TextMeshProUGUI waveNumber;
    public TextMeshProUGUI enemiesCount;
    public TextMeshProUGUI timer;

    [Header("Misc UI")]
    public TextMeshProUGUI pickupNotify;

    [Header("Misc Settings")]
    public float changeSpeed;

    public PlayerStats ps;
    public PlayerWeaponController pwc;
    public SpawnManager sm;
    ThrowableController tc;

    WeaponController weapon; //cached current weapon
    ThrowItem throwable; //cached current held grenade

    void Start() {
        PlayerWeaponController.OnWeaponChange.AddListener(UpdatePrimaryIcon);
        PlayerWeaponController.OnGrenadeChange.AddListener(UpdateSecondaryIcon);
        SpawnManager.OnPickupSpawn.AddListener(Notify);
        weapon = pwc.weapons[0].GetComponent<WeaponController>();
        tc = pwc.tc;

        StartCoroutine(PickupTimer());
    }

    // Update is called once per frame
    void Update()
    {
        scalePointer.rectTransform.anchoredPosition = Vector2.Lerp(scalePointer.rectTransform.anchoredPosition, new Vector2(ps.dopamineLevel * 100, 0), Time.deltaTime * changeSpeed);
        scalePointerRally.rectTransform.anchoredPosition = Vector2.Lerp(scalePointerRally.rectTransform.anchoredPosition, new Vector2(ps.actualDopamineLevel * 100, 0), Time.deltaTime * changeSpeed);

        ammoCount.text = weapon.currentAmmo + "/" + weapon.weaponStats.ammoMax;

        dashRecharge.value = ps.playerMotor.dashTimer/ps.playerMotor.dashCooldown;

        enemiesCount.text = "Enemies left: " + sm.enemiesLeft;
        if(sm.interwaveInProgress)
            timer.text = "Next wave in: " + sm.timer2.ToString("0");
        else
            timer.text = sm.noTimeLimit ? "" : "Time left: " + sm.timer.ToString("0");

        if(grenadeSlot.activeInHierarchy) 
        {
            secondaryRecharge.value = tc.rechargeTimer/throwable.rechargeSpeed;
            secondaryItems.text = tc.grenadesOnHand + "/" + throwable.maxOnHand;
        }

        UpdateFace();
    }
    void UpdateFace()
    {
        if(ps.dopamineLevel < -1.5f)
            face.sprite = faces[0];
        else if (ps.dopamineLevel < -0.5f)
            face.sprite = faces[1];
        else if (ps.dopamineLevel < 0.5f)
            face.sprite = faces[2];
        else if (ps.dopamineLevel < 1.5f)
            face.sprite = faces[3];
        else
            face.sprite = faces[4];
    }

    void UpdatePrimaryIcon()
    {
        weapon = pwc.weapons[pwc.currentWeapon].GetComponent<WeaponController>();
        primarySlot.sprite = weapon.weaponStats.weaponIcon;
        primaryName.text = weapon.weaponStats.weaponName;
        
        if(pwc.currentWeapon != 4)
            pickupName.text = "NEW WEAPON: " + weapon.weaponStats.weaponName;
        else
            pickupName.text = "UPGRADED WEAPON: " + weapon.weaponStats.weaponName;

        pickupInfo.text = weapon.weaponStats.weaponInfo;
        StartCoroutine(PickupTimer());
    }
    
    void UpdateSecondaryIcon()
    {
        if(tc.currentGrenade == -1) grenadeSlot.SetActive(false);
        else grenadeSlot.SetActive(true);

        throwable = tc.GetCurrentThrowable();
        secondarySlot.sprite = throwable.weaponIcon;
        secondaryName.text = throwable.weaponName;

        pickupName.text = "NEW GRENADE: " + throwable.weaponName;
        pickupInfo.text = throwable.weaponInfo;

        StartCoroutine(PickupTimer());
    }

    IEnumerator PickupTimer()
    {
        pickupName.gameObject.SetActive(true);
        pickupInfo.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        pickupName.gameObject.SetActive(false);
        pickupInfo.gameObject.SetActive(false);

        yield return null;
    }

    public void Notify(int type)
    {
        StartCoroutine(PickupSpawnInfo(type));
    }

    IEnumerator PickupSpawnInfo(int type)
    {
        switch(type)
        {
            case 0:
                pickupNotify.text = "New weapon appeared...";
                break;
            case 1:
                pickupNotify.text = "New grenade appeared...";
                break;
        }
        pickupNotify.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        pickupNotify.gameObject.SetActive(false);
        yield return null;
    }
}
