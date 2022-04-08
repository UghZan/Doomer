using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    //basically Bloodborne rally system
    //when you get damaged, you can still get dopamine back if you strike right back
    [Header("Dopamine Rally System")]
    public float dopamineLevel = 0.5f; //gets updated over time to lower one
    public float actualDopamineLevel = 0.5f; //is the one that gets damaged, but u can regain it
    public float dopamineReductionDelay = 1.5f; //time until dopamine begins to change
    public float dopamineChangeSpeed = 2f;

    public float critChance = 0.25f;
    public AudioClip[] hitSounds;
    
    public PlayerMotor playerMotor;
    public PlayerEffects playerEffects;
    public ParticleSystem gooed;
    public AudioSource aS;

    public float gooCovered;

    float speedModified;
    float accelModified;
    float dopamineTimer;

    bool isDead;


    // Start is called before the first frame update
    void Start()
    {
        UpdateDopamine(0,true);
    }

    // Update is called once per frame
    void Update()
    {    
        speedModified = 6 - dopamineLevel/3;
        accelModified = 8 - dopamineLevel/3;
        critChance = 0.25f + Mathf.Min(0, dopamineLevel/6);

        //lower the dopamine, higher the speed
        //higher the dopamine, more the damage
        playerMotor.maxSpeed = speedModified * (gooCovered > 0 ? 0.75f : 1f);
        playerMotor.maxAcceleration = accelModified * (gooCovered > 0 ? 0.75f : 1f);
        playerMotor.canSprint = gooCovered <= 0;

        if(dopamineTimer > 0) dopamineTimer -= Time.deltaTime;
        else dopamineLevel = Mathf.MoveTowards(dopamineLevel, actualDopamineLevel, Time.deltaTime * dopamineChangeSpeed);

        if(gooCovered > 0) 
        {
            gooCovered -= Time.deltaTime;
            playerMotor.isSprinting = false;
        }
        else
        {
            if(gooed.isPlaying) gooed.Stop();
        }

        if(dopamineLevel == -2)
        {
            SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
        }
    }

    //ignoreRally is to skip rally system and directly change dopamine
    public void UpdateDopamine(float diff, bool ignoreRally = false)
    {
        if(diff >= 0)
        {
            if(ignoreRally)
            {
                dopamineLevel = Mathf.Clamp(dopamineLevel + diff, -2,2);
                actualDopamineLevel = dopamineLevel;
            }
            else
            {
                if(actualDopamineLevel + diff <= dopamineLevel) actualDopamineLevel = Mathf.Clamp(actualDopamineLevel + diff, -2,2);
                else if(actualDopamineLevel + diff > dopamineLevel)
                {
                    actualDopamineLevel = Mathf.Clamp(actualDopamineLevel + diff, -2,2);
                    dopamineTimer = dopamineReductionDelay;
                }
            }
        }
        else
        {
            if(ignoreRally)
            {
                dopamineLevel = Mathf.Clamp(dopamineLevel + diff, -2,2);
                actualDopamineLevel = dopamineLevel;
            }
            else
            {
                actualDopamineLevel = Mathf.Clamp(actualDopamineLevel + diff, -2,2);
                dopamineTimer = dopamineReductionDelay;
            }
            
            aS.PlayOneShot(hitSounds[Random.Range(0,hitSounds.Length)]);
            playerEffects.Damage();
        }
    }

    public bool DopamineRallyRegained()
    {
        if(Mathf.Abs(dopamineLevel - actualDopamineLevel) < 0.001) return true;
        return false;
    }

    public void UpdateGooStatus(float num)
    {
        gooCovered = num;
        if(!gooed.isPlaying) gooed.Play();
    }
}
