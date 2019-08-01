using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlyingMovement : MonoBehaviour
{
    public float zSpeed, xSpeed, ySpeed = 3;
    public float mouseXSpeed, mouseYSpeed = 3;
    bool update;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Invoke("SetUpdate", 1);
    }

    void SetUpdate ()
    {
        update = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (update)
        {
            // MOVEMENT
            bool sprint = false || Input.GetKey(KeyCode.LeftShift);

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            Vector3 position = Vector3.zero;
            float y = (Input.GetKey(KeyCode.E) ? 1 : 0) + (Input.GetKey(KeyCode.Q) ? -1 : 0);

            position.x = (sprint ? xSpeed * 8 : xSpeed) * input.x * Time.deltaTime;
            position.y = (sprint ? ySpeed * 8 : ySpeed) * y * Time.deltaTime;
            position.z = (sprint ? zSpeed * 8 : zSpeed) * input.y * Time.deltaTime;

            transform.Translate(position);
            //transform.position = position;

            // CAMERA

            Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            Vector3 cameraRotation = transform.rotation.eulerAngles;

            cameraRotation.x = cameraRotation.x + mouseYSpeed * -mouseInput.y * Time.deltaTime;
            cameraRotation.y = cameraRotation.y + mouseXSpeed * mouseInput.x * Time.deltaTime;

            transform.eulerAngles = cameraRotation;
        }
    }
}
