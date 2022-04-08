using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Bob Settings")] 
        [SerializeField] private float bobAmount;
        [SerializeField] private float bobSpeed;
        [SerializeField] private float bobChangeSpeed;
        [SerializeField] private float bobSprintMultiplier;

        [Header("Tilt Settings")] 
        [SerializeField] private float tiltAngle;
        [SerializeField] private float tiltChangeSpeed;
        
        [Header("Dash Tilt Settings")] 
        [SerializeField] private float dashTiltAngle;
        [SerializeField] private float dashTiltChangeSpeed;

        [Header("References")] 
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private RecoilController recoilControl;
        [SerializeField] private Transform cam;
        
        private Vector3 startPosition;
        private Quaternion dashTiltRot;
        private bool isLocked;
        
        private void Start()
        {
            startPosition = cam.transform.localPosition;
            playerMotor.OnDash.AddListener(DashTilt);
        }

        private void LateUpdate()
        {
            if (!isLocked)
            {
                Tilt();
                Bob();
            }

            dashTiltRot = Quaternion.Lerp(dashTiltRot, Quaternion.identity, Time.deltaTime * dashTiltChangeSpeed);
        }

        private void DashTilt()
        {
            Vector3 tiltDirection = playerMotor.lastDashVelocity;
            Vector3 tiltVector = Quaternion.AngleAxis(90, Vector3.up) * (tiltDirection * dashTiltAngle);
            dashTiltRot = Quaternion.Euler(tiltVector);
        }

        private void Tilt()
        {
            Vector3 tiltDirection = PlayerInputController.GetMovementVector().normalized;
            Vector3 tiltVector = Quaternion.AngleAxis(90, Vector3.up) * (tiltDirection * tiltAngle);
            Quaternion tiltRot = Quaternion.Euler(tiltVector);
            
            cam.localRotation = Quaternion.Lerp(cam.localRotation, tiltRot * dashTiltRot * recoilControl.recoilQuaternion, tiltChangeSpeed * Time.deltaTime);
        }

        private void Bob()
        {
            float speed = PlayerInputController.GetMovementVector().normalized.magnitude;
            float speedMultiplier = (playerMotor.isSprinting ? bobSprintMultiplier : 1f);
            
            float y = bobAmount * speed * Mathf.Sin(2 * bobSpeed * speedMultiplier * Time.time);
            cam.localPosition = Vector3.Lerp(cam.localPosition, new Vector3(startPosition.x, startPosition.y + y, startPosition.z), Time.deltaTime * bobChangeSpeed);
        }
    }
}
