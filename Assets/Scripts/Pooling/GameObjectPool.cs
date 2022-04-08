using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField] GameObject poolObject;
    [SerializeField] int preSpawns;//amount at start
    [SerializeField] int overflowSpawnAmount; //in case we need more, how much additional objects to spawn

    int currentAmount;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < preSpawns; i++)
        {
            Instantiate(poolObject, transform.position, Quaternion.identity, transform).SetActive(false);
            currentAmount++;
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < currentAmount; i++)
        {
            if(!transform.GetChild(i).gameObject.activeInHierarchy) return transform.GetChild(i).gameObject;
        }
        //if we are out of available objects, expand the pool
        int previous = currentAmount;
        for (int i = 0; i < overflowSpawnAmount; i++)
        {
            Instantiate(poolObject, transform.position, Quaternion.identity, transform).SetActive(false);
            currentAmount++;
        }
        for (int i = previous; i < currentAmount; i++)
        {
            if(!transform.GetChild(i).gameObject.activeInHierarchy) return transform.GetChild(i).gameObject;
        }
        return null;
    }
}
