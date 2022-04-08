using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Wave")]
public class Wave : ScriptableObject
{
    public WaveEntry[] spawns;
    public WaveEntry[] bonuses;

    public float timeLimit; //set to -1 to stop
}
