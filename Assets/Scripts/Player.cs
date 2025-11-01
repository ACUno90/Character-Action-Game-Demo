using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDamage
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask IgnorePlayer;
    [SerializeField] int Speed;
    [SerializeField] int JumpSpeed;
    [SerializeField] int Jump;
    [SerializeField] int JumpMax;
    [SerializeField] int SptrintMax;
    [SerializeField] int Gravity;
    [SerializeField] int ShootDmg;
    [SerializeField] float Shootrate;
    [SerializeField] int shootdistance;
    [SerializeField] int hp;
    [SerializeField] int dashspeed;
    [SerializeField] int dashmax;

    Vector3 moveDirc;
    Vector3 Playerval;
    int jumpcount;
    int dashcount;
    bool isSptrinting;
    bool isShotting;
    private bool DoubleJump;
    public bool dashing;
    public Vector2 turn;
    public float sensitivity = .5f;


    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;   
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootdistance, Color.red);
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        transform.rotation = Quaternion.Euler(-turn.y, turn.x, 0);
        Movement();
        sprint();
       
    }
    void Movement()
    {
        if (controller.isGrounded)
        {
            Playerval = Vector3.zero;
            jumpcount = 0;
            DoubleJump = false; 
        }
        moveDirc = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDirc * Speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && jumpcount < JumpMax)
        {

            jumpcount++;
            Playerval.y = JumpSpeed;
           DoubleJump = !DoubleJump;   
        }

        if(Input.GetButtonDown("Jump") && Playerval.y>0f)
        {
            jumpcount++;
            Playerval.y = JumpSpeed * 0.5f;
        }
        controller.Move(Playerval * Time.deltaTime);
        Playerval.y -= Gravity * Time.deltaTime;





        //if (Input.GetButton("Fire1") && !isShotting)
        //{
        //    StartCoroutine(shoot());
        //}
        if (Input.GetButtonDown("Dash") &&!dashing)
        {
        
            StartCoroutine(Dash());
        }
    }
    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            Speed *= SptrintMax;
            isSptrinting = true;
        }
        else if (Input.GetButtonDown("Sprint"))
        {
            Speed /= SptrintMax;
            isSptrinting = false;
        }

    }
    IEnumerator Dash()
    {
        dashing = true;
        float startTime = Time.time;
        while (Time.time < startTime + 0.5f) 
        {
            controller.Move(moveDirc * dashspeed * Time.deltaTime);
            yield return null;
        }
        dashing = false;
    }

    public void takeDamage(int amount)
    {

    }

  




}
