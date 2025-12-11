using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDamage
{

    [Header("Player Stats")]
    [SerializeField] public CharacterController controller;
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
    //[SerializeField] int hp;
    //[SerializeField] int maxHp;
    [SerializeField] int dashspeed;
    [SerializeField] int dashmax;

    public float health;
    public float maxHealth;

    //Header for action moves

    [Header("Action Move")]
    [SerializeField] int AirLauncherForce;
    [SerializeField] int AitLauncherdDamage;
    [SerializeField] int AirLauncherSpeed;//to experiment
    [SerializeField] int SimpleCombospeed;
    [SerializeField] int SimpleComboDamage;
    [SerializeField] public int StingerSpeed;//to experiment
    [SerializeField] int StingerDamage;


    [Header("Sounds")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audWalk;
    [SerializeField] float audWalkVol;

    public bool isStinger;
    public bool isAirLauncher;
    private bool IsPlayingStop;
    public Vector3 moveDirc;
    Vector3 Playerval;
    int jumpcount;
    //int dashcount;
    //bool isSptrinting;
    //bool isShotting;
    //bool isNotActionMove = false;
    public bool RequestingActionMove = false;
    private bool DoubleJump;
    private bool jump;
    public bool dashing;
    public Vector2 turn;
    public float sensitivity = .5f;
   public Animator animationController;
    // to make it use states
    private PlayerStateMachine state;

    public MovementState MoveState;
    public enum MovementState
    {
        idle,
        stinger,
        laucher,
        ThreeHitCombo
    }

    // Start is called before the first frame update
    void Start()
    {
        IsPlayingStop = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        // make a new statemachine
        state = new PlayerStateMachine();

        //Initialize movement state with new state
        MovementSuperState movementState = new MovementSuperState(this, "isMoving", state);
        state.InitializeStateMachine(movementState);

       
    }

    // Update is called once per frame
    void Update()
    {
        // so we can move camera with mouse
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootdistance, Color.red);
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        transform.rotation = Quaternion.Euler(-turn.y, turn.x, 0);
        //Movement();
        //sprint();
        // so we can use the logic from movement state machine to dash and walk
        state.GetCurrentState().LogicUpdate();
  
    }
    public void Movement()
    {
       
        if (controller.isGrounded)
        {
            Playerval = Vector3.zero;
            jumpcount = 0;
            DoubleJump = false;
            if(!IsPlayingStop && moveDirc != Vector3.zero)
            {
                StartCoroutine(PlaySteps());
            }
        }
        moveDirc = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
       



        controller.Move(moveDirc * Speed * Time.deltaTime);
        if (moveDirc == Vector3.zero)
        {
            //idle animation
            if(isStinger == true)
            {
                animationController.SetBool("Stinger", false);
                isStinger = false;
            }


            animationController.SetFloat("speed", 0);
        }
      
        else
        {
        //run animation
        animationController.SetFloat("speed", 1);
         }


        if (Input.GetButtonDown("Jump") && jumpcount < JumpMax)
        {
            jumpcount++;
            Playerval.y = JumpSpeed;
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
            //if (Playerval.y == Vector3.zero.y)
            //{
            //    animationController.SetFloat("JumpSpeed", 0);
            //}
            //else
            //{

            //    //reset y velocity
            //  //  rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            //    animationController.SetFloat("JumpSpeed", 1);

            //}
            if (controller.isGrounded)
            {
                animationController.SetFloat("JumpSpeed", 0);

            }
            else
            {
              animationController.SetFloat("JumpSpeed", 1);
            }

                DoubleJump = !DoubleJump;
        }

        //if (Input.GetButtonDown("Jump") && Playerval.y>0f)
        //{
        //    jumpcount++;
        //    Playerval.y = JumpSpeed * 0.5f;
        //}
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
    //void sprint()
    //{
    //    if (Input.GetButtonDown("Sprint"))
    //    {
    //        Speed *= SptrintMax;
    //        isSptrinting = true;
    //    }
    //    else if (Input.GetButtonDown("Sprint"))
    //    {
    //        Speed /= SptrintMax;
    //        isSptrinting = false;
    //    }

    //}
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

    //Character Action Moves
    public void StingerMove()
    {
        isStinger = true;
        animationController.SetBool("Stinger", true);
        controller.Move(transform.forward * StingerSpeed * Time.deltaTime);
    }

    public void AirLauncher()
    {
        isAirLauncher = true;
        controller.Move(transform.up * AirLauncherSpeed * Time.deltaTime);
       
    }

    public void SimpleCombo()
    {

    }
    public void takeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            // GameManger.Instance.StartLose();
        }
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
    }


    IEnumerator PlaySteps()
    {
        IsPlayingStop = true;
        //Play footstep sound   
        aud.PlayOneShot(audWalk[Random.Range(0, audWalk.Length)], audWalkVol);
        if(Speed < 6)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }
        IsPlayingStop = false;
    }



}
