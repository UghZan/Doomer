using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerEffects : MonoBehaviour
{
    public PlayerStats ps;
    public Volume v;
    UnityEngine.Rendering.Universal.ColorAdjustments override_colorAdj;
    UnityEngine.Rendering.Universal.ChromaticAberration override_chromAber;
    // Start is called before the first frame update
    void Start()
    {
        v.profile.TryGet<UnityEngine.Rendering.Universal.ChromaticAberration>(out override_chromAber);
        v.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out override_colorAdj);
    }

    // Update is called once per frame
    void Update()
    {
        override_colorAdj.saturation.Override(Mathf.Lerp(override_colorAdj.saturation.value, ps.dopamineLevel * 35, Time.deltaTime * 10));
        override_colorAdj.postExposure.Override(Mathf.Lerp(override_colorAdj.postExposure.value, 2 + ps.dopamineLevel, Time.deltaTime * 10));
        override_colorAdj.colorFilter.Override(Color.Lerp(override_colorAdj.colorFilter.value, Color.white, Time.deltaTime * 5));
        override_chromAber.intensity.Override(Mathf.Lerp(override_chromAber.intensity.value, Mathf.Clamp(ps.dopamineLevel - 1,0,0.5f), Time.deltaTime * 10));
    }

    public void Damage()
    {
        override_colorAdj.colorFilter.value = Color.red;
    }
}
