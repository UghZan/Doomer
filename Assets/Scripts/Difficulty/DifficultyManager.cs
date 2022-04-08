using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DifficultyManager
{
    public static float spawnMultiplier;
    public static float damageMultiplier;
    public static float speedMultiplier;
    public static float dashRegenMultiplier;
    public static float playerDamageMultiplier;
    public static bool pickedTrigger;

    public static void Init(int difficulty)
    {
        pickedTrigger = true;
        switch(difficulty)
        {
            case 0:
                spawnMultiplier = 0.75f;
                damageMultiplier = 0.75f;
                speedMultiplier = 0.66f;
                dashRegenMultiplier = 1.25f;
                playerDamageMultiplier = 1.5f;
                break;
            case 1:
                spawnMultiplier = 1f;
                damageMultiplier = 1f;
                speedMultiplier = 1f;
                dashRegenMultiplier = 1f;
                playerDamageMultiplier = 1f;
                break;
            case 2:
                spawnMultiplier = 2f;
                damageMultiplier = 1.5f;
                speedMultiplier = 1.25f;
                dashRegenMultiplier = 2f;
                playerDamageMultiplier = 1.25f;
                break;
        }
        SpawnManager.startSpawning = true;
    }
}
