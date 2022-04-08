using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController instance;
    public Transform camTransform;
    Vector3 originalPos;
    float strength;
    float time;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        originalPos = camTransform.localPosition;
    }

    public void Shake(float _time, float _strength)
    {
        time = _time;
        strength = _strength;
        StartCoroutine(ShakeCam());
    }

    IEnumerator ShakeCam()
    {
        while(time > 0)
        {
			camTransform.localPosition = originalPos + Random.insideUnitSphere * strength;
			time -= Time.deltaTime;
            yield return null;
		}
		time = 0f;
		camTransform.localPosition = originalPos;
    }
}
