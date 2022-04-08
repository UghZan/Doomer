using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool movementLocked;
    public float speed;
    public float sensitivity;
    float horInput;
    float verInput;
    float mouseXInput;
    float mouseYInput;

    Rigidbody rb;
    UnityEngine.Camera main;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        main = UnityEngine.Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        horInput = Input.GetAxis("Horizontal");
        verInput = Input.GetAxis("Vertical");
        mouseXInput += Input.GetAxis("Mouse X");
        mouseYInput = Mathf.Clamp(mouseYInput - Input.GetAxis("Mouse Y") * sensitivity, -75,75);
    }

    void FixedUpdate() {
        if(!movementLocked) Move();
        
    }

    private void LateUpdate() {
        if(!movementLocked)
        {
            RotateBody();
            RotateHead();
        }
    }

    void Move()
    {
        rb.MovePosition(transform.position + (transform.forward * verInput + transform.right * horInput) * speed *Time.deltaTime);
    }

    void RotateBody()
    {
        rb.MoveRotation(Quaternion.Euler(0,mouseXInput * sensitivity,0));
    }

    void RotateHead()
    {
        main.transform.localRotation = Quaternion.Euler(mouseYInput,0,0);
    }
}
