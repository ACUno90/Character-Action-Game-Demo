using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] int senetive;
    [SerializeField] int lockverMin, lockVerMax;
    [SerializeField] bool inverY;

    [SerializeField] float downSlashCameraLift = 1.5f; //adjust numbers later to make it look better
    [SerializeField] float cameraMoveSpeed = 6f;

    float roll;
    float defaultLocalY;
    Player player;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player = GetComponentInParent<Player>();
        defaultLocalY = transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        // smoothly lift camera while player is performing down-slash so the player stays in view
        if (player != null)
        {
            float targetY = player.IsDownSlashActive ? defaultLocalY + downSlashCameraLift : defaultLocalY;
            Vector3 lp = transform.localPosition;
            lp.y = Mathf.Lerp(lp.y, targetY, Time.deltaTime * cameraMoveSpeed);
            transform.localPosition = lp;
        }

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
