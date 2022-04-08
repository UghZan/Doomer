using UnityEngine;


    public static class PlayerInputController
    {
        public static bool mouseLock { get; private set; }

        public static void ChangeLockMode(bool state)
        {
            mouseLock = state;
            if (mouseLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public static Vector3 GetMovementVector()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(x, 0, z);

            return move.normalized;
        }
    }
