using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    ZombieEnemy df;
    public float health;
    public float maxHealth;

    //Header for action moves

    [Header("Action Move")]
    public float AirLauncherForce;
    [SerializeField] int AitLauncherdDamage;
    [SerializeField] int AirLauncherSpeed;//to experiment
    [SerializeField] int SimpleCombospeed;
    [SerializeField] int SimpleComboDamage;
    [SerializeField] public int StingerSpeed;//to experiment
  public float StingerForce;


    [Header("Sounds")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audWalk;
    [SerializeField] float audWalkVol;
    [SerializeField] AudioClip audSword;
    [SerializeField] float audSwordVol;
    [SerializeField] AudioClip audStinger;
    [SerializeField] float audStingVol;
     [SerializeField] AudioClip audLauncher;
    [SerializeField] float audLauncherVol;

    public LayerMask enemyLayers2;
    public bool isStinger;
    public bool isAirLauncher;
    bool airLauncherActive;
    bool canMoveUp;
    bool canStingForward;
    private bool IsPlayingStop;
    public Vector3 moveDirc;
    SwordDamage sd;
    Vector3 Playerval;
    int jumpcount;
  //  public float cooldownAttackTime = 2f;
    private float nextAttackTime = 0f;
    public static int noOfClicks = 0;
    float lastClickedTime = 0;
    float maxComboDelay = 1f;
    private bool canAcceptNextInput = true;
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
  
        // so we can use the logic from movement state machine to dash and walk
        state.GetCurrentState().LogicUpdate();

        //check for mouse input
        if (Input.GetMouseButtonDown(0))
        {

            SimpleCombo();
        }


    }
    public void Movement()
    {

        if (controller.isGrounded)
        {
            Playerval = Vector3.zero;
            jumpcount = 0;
            DoubleJump = false;
            if (!IsPlayingStop && moveDirc != Vector3.zero)
            {
                StartCoroutine(PlaySteps());
            }
        }
        moveDirc = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;




        controller.Move(moveDirc * Speed * Time.deltaTime);
        if (moveDirc == Vector3.zero)
        {
            //idle animation
            //if (isStinger == true)
            //{
            //    animationController.SetBool("Stinger", false);
            //    isStinger = false;
            //}


            animationController.SetFloat("speed", 0);
        }

        else
        {
            //run animation
            animationController.SetFloat("speed", 1);
        }

        //if (isAirLauncher == true)
        //{
        //    animationController.SetBool("airLauncher", false);
        //    isAirLauncher = false;
        //}


        if (Input.GetButtonDown("Jump") && jumpcount < JumpMax)
        {
            jumpcount++;
            Playerval.y = JumpSpeed;
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
        
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


        controller.Move(Playerval * Time.deltaTime);
        Playerval.y -= Gravity * Time.deltaTime;





        //if (Input.GetButton("Fire1") && !isShotting)
        //{
        //    StartCoroutine(shoot());
        //}
        if (Input.GetButtonDown("Dash") && !dashing)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        animationController.SetTrigger("Dash");
        dashing = true;
        float startTime = Time.time;
        while (Time.time < startTime + 0.5f)
        {
            //animationController.SetTrigger("Dash");
            controller.Move(moveDirc * dashspeed * Time.deltaTime);
         
            yield return null;
        }
       
        dashing = false;
    }

    //Character Action Moves
   
    public void StartStinger()
    {
        isStinger = true;
        canStingForward = false;
        animationController.SetTrigger("Stinger");
    }

    public void ActivateStingerForward()
    {
        canStingForward = true;
    }

    public void UpdateStingerMove()
    {
        //isStinger = true;
        //animationController.SetBool("Stinger", true);
        if (!canStingForward)
        {
            return;
        }

        controller.Move(transform.forward * StingerSpeed * Time.deltaTime);
        aud.PlayOneShot(audStinger, audStingVol);
    }

    public void EndStinger()
    {
      
        isStinger = false;
    }



    public void StartAirLauncher()
    {
        isAirLauncher = true;
        airLauncherActive = true;
        canMoveUp = false;
        //use a trigger instead of bool 
        animationController.SetTrigger("airLauncher");
    
    }
     
    public void ActivateAirLauncherForce()
    {
        canMoveUp = true;
    }

    public void UpdateAirLauncher()
    {
      if(!canMoveUp)
        {
            return;
        }
  
        controller.Move(transform.up * AirLauncherSpeed * Time.deltaTime);
        aud.PlayOneShot(audLauncher, audLauncherVol);
    }



  public void EndAirLauncher()
    {
        isAirLauncher = false;
    }


    public void SimpleCombo()
    {
        if (canAcceptNextInput == false)
        {
            return;
        }
        noOfClicks++;

        AnimatorStateInfo AStates = animationController.GetCurrentAnimatorStateInfo(0);
        if (AStates.IsName("HumanM@Idle01"))
        {
            aud.PlayOneShot(audSword, audSwordVol);

            noOfClicks = 1;
            animationController.SetTrigger("SwordAttack");
            // df.ApplyKnockback(df.transform.forward, 70f);
            return;
        }
        else if (AStates.IsName("slash1"))
        {
            aud.PlayOneShot(audSword, audSwordVol);
            noOfClicks = 2;
            animationController.SetTrigger("SwordAttack2");
            return;
        }
        else if (AStates.IsName("slash2"))
        {
            aud.PlayOneShot(audSword, audSwordVol);
            noOfClicks = 3;
            animationController.SetTrigger("SwordAttack3");
            //df.ApplyKnockback(transform.forward, 70f);

            //add a bool check here for truth so we can call it in collider func in zombie enemy
            return;

        }
        // canAcceptNextInput = false;

    }
    public void takeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            GameManger.Instance.StartLose();
        }
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
    }


    IEnumerator PlaySteps()
    {
        IsPlayingStop = true;
        //Play footstep sound   
        aud.PlayOneShot(audWalk[Random.Range(0, audWalk.Length)], audWalkVol);
        if (Speed < 6)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }
        IsPlayingStop = false;
    }

    public void EnableNextInput()
    {
        canAcceptNextInput = true;
    }

    public void ResetCombo()
    {
        noOfClicks = 0;
        canAcceptNextInput = true;
    }

    public float GetVerticalVelocity()
    {
      return controller.velocity.y; 
    }

    public float GetHorizontalVelocity()
    {
        return controller.velocity.x;
    }
}
