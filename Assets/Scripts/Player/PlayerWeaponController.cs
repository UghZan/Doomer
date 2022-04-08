using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponController : MonoBehaviour
{
    public GameObject[] weapons;
    public int currentWeapon = -1;
    public ThrowableController tc;

    public AudioSource pickupSound;
    public static UnityEvent OnWeaponChange = new UnityEvent(); //used by PlayerHUD to display info about weapon
    public static UnityEvent OnGrenadeChange = new UnityEvent(); //ditto
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ChangeWeapon(0);
    }

    public void ChangeWeapon(int next)
    {
        pickupSound.Play();
        if(currentWeapon == 0 && next == 0)
            next = 4;
        for(int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == next);
        }
        currentWeapon = next;
        OnWeaponChange.Invoke();
    }

    public void ChangeGrenade(int next)
    {
        pickupSound.Play();
        tc.UpdateEquippedThrowable(next);
        OnGrenadeChange.Invoke();
    }
}
