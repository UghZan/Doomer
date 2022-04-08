using System;
using UnityEngine;

    public class WeaponRecoilController : MonoBehaviour
    {
        public float recoil;

        private WeaponItem _weaponStats;
        
        private float visualRecoil;

        [Header("References")]
        public WeaponMovementController wmc;
        public RecoilController rc;
        public PlayerMotor pm;

        private float lastShotTime = 0f;


        public void InitStats(WeaponItem stats)
        {
            _weaponStats = stats;
            if (_weaponStats.fixedRecoil) recoil = _weaponStats.maxRecoil;
        }

        private void Update()
        {
            if (lastShotTime > 0f) lastShotTime -= Time.deltaTime;
            else
            {
                if(!_weaponStats.fixedRecoil) recoil = Mathf.Lerp(recoil, _weaponStats.minRecoil, Time.deltaTime * _weaponStats.recoilRecoverSpeed);
                visualRecoil = Mathf.Lerp(visualRecoil, _weaponStats.minVisualRecoil, Time.deltaTime * _weaponStats.visualRecoilRecoverSpeed);
            }
        }
    
        public void Recoil()
        {
            if (!_weaponStats.fixedRecoil)
                recoil = Mathf.Clamp(recoil + _weaponStats.recoilIncreaseStep, 0, _weaponStats.maxRecoil);

            visualRecoil = Mathf.Clamp(visualRecoil + _weaponStats.recoilIncreaseStep, 0, _weaponStats.maxVisualRecoil);
            
            //visual recoil
            wmc.Punch(visualRecoil);
            rc.RecoilPunch(_weaponStats.horizontalCamRecoil, _weaponStats.verticalCamRecoil, _weaponStats.negativeVerticalRecoilReduction, visualRecoil);
            lastShotTime = 0.5f;
        }
    }
