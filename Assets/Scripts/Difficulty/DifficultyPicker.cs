using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyPicker : MonoBehaviour
{
    public GameObject[] pickers;
    public int difficulty;

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * difficulty * 50f);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            DifficultyManager.Init(difficulty);
        }
        foreach(GameObject p in pickers) Destroy(p);
    }   
}
