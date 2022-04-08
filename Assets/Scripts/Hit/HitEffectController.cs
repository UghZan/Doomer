using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HitEffectController : MonoBehaviour
{
    public Image hitMarker;
    public Image critMarker;
    public float hitMarkerDecaySpeed = 1f;
    public float critMarkerDecaySpeed = 2f;

    Color hitColor = Color.clear;
    Color critColor = Color.clear;

    public static UnityEvent OnHit = new UnityEvent();
    public static UnityEvent OnCrit = new UnityEvent();
    // Start is called before the first frame update
    void Start()
    {
        OnHit.AddListener(ShowHitMarker);
        OnCrit.AddListener(ShowCritMarker);
    }

    void Update()
    {
        hitMarker.color = hitColor;
        critMarker.color = critColor;

        hitColor = Color.Lerp(hitColor, Color.clear, Time.deltaTime * hitMarkerDecaySpeed);
        critColor = Color.Lerp(critColor, Color.clear, Time.deltaTime * critMarkerDecaySpeed);
    }

    void ShowHitMarker()
    {
        hitColor = Color.white;
    }

    void ShowCritMarker()
    {
        critColor = Color.red;
    }
}
