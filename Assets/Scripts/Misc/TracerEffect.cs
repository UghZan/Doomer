using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerEffect : MonoBehaviour
{
    public float time;
    [SerializeField] LineRenderer lr;
    Color c = Color.white;

    private void OnEnable() {
        c = Color.white;
        StartCoroutine(Effect());
    }
    IEnumerator Effect()
    {
        float t = time;
        while(t > 0)
        {
            t -= Time.deltaTime;
            c.a = t/time;
            lr.startColor = c;
            lr.endColor = c;
            yield return null;
        }
        gameObject.SetActive(false);
        yield return null;
    }
}
