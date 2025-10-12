using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] int senetive;
    [SerializeField] int lockverMin, lockVerMax;
    [SerializeField] bool inverY;

    float roll;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y") * senetive * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * senetive * Time.deltaTime;
        if (!inverY)
            roll += mouseY;
        else
            roll -= mouseY;


        roll = Mathf.Clamp(roll, lockverMin, lockVerMax);

        transform.localRotation = Quaternion.Euler(roll, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
