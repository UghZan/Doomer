using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    public float time;
    float timer;

    private void OnEnable() {
        timer = 0;
    }
    void Update()
    {
        timer+=Time.deltaTime;
        if(timer > time) gameObject.SetActive(false);
    }
}
