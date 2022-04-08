using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    public int times;
    public GameObject whatToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < times; i++)
        {
            Instantiate(whatToSpawn, transform.position, Quaternion.identity);
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
