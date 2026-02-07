using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class ZombieEnemy : MonoBehaviour, IDamage
{
    public int damage;
    [SerializeField] Renderer Model;
    [SerializeField] int HP;
    [SerializeField] NavMeshAgent agent;
    public LayerMask Ground, WherePlayer;
    //Patroling
    public Vector3 WalkPoint;
    bool IsWalking;
    [SerializeField] float walkpointRange;
    Color colorOrig;
    //States
    [SerializeField] float Sightrange;
    bool isinSight;
    Player player;
    [SerializeField] AudioSource Aud;
    [SerializeField] AudioClip ZombiDeath;
    [SerializeField] float AudZombietDeathVol;
    [SerializeField] AudioClip ZombieHit;
    [SerializeField] float AudZombieHitVol;
    [SerializeField] AudioClip[] ZombieFootsteps;
    [SerializeField] float AudZombieFootSteps;
    public Animator animationZombieController;
    private Rigidbody rb;
    public float knockbackDuration = 0.5f;
    bool IsFollowingPlayer;
    bool isFoleingStingPlayerZ;
    bool ZombieHurt;
    bool isPlayingStop;
    void Start()
    {
        isPlayingStop = false;
     //   colorOrig = Model.material.color;
        GameManger.Instance.updateGameGoal(1);
       rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        isinSight = Physics.CheckSphere(transform.position, Sightrange, WherePlayer);
        if (!isinSight)
        {
            Patroling();

        }
        if (isinSight)
        {
            Chase();
        }

        if (!IsFollowingPlayer || player == null) return;

        //match player's horizontal movement
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, player.GetHorizontalVelocity(), rb.linearVelocity.z);

        if (player.GetHorizontalVelocity() <= 0)
        {
            IsFollowingPlayer = false;
        }
    }

    public void StartStingFollow(Player p)
    {
         player = p;
        IsFollowingPlayer = true;
        //reset rb velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //initial drag enemy with stinger horizontaly 
        rb.linearVelocity = new Vector3(rb.linearVelocity.x +player.StingerSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    public void StartAirFollow(Player p)
    {
        player = p;
        isFoleingStingPlayerZ = true;
        //reset rb velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //initial launch
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y + player.AirLauncherForce, rb.linearVelocity.z);
    }


    public void EndStingFollow()
    {
      
        //re-enable navmesh agent
        if (agent != null)
            agent.enabled = true;

    }

    private void OnTriggerEnter(Collider collision)
    {
        IDamage hit = collision.GetComponent<IDamage>();
        if (hit != null) hit.takeDamage(damage);
        else if (collision.GetComponentInParent<IDamage>() != null)
        {
            hit = collision.GetComponentInParent<IDamage>();
            hit.takeDamage(damage);



        }
    }
    public void ApplyKnockback(Vector3 direction, float force)
    {
        // StartCoroutine(KnockbackCoroutine(direction, force));
        //disbale navmesh agent
        agent.enabled = false;
        //calculate knockback vector
        Vector3 knockbackVector = direction.normalized * force;
        //apply an impulse force to the rigidbody
        rb.AddForce(knockbackVector, ForceMode.Impulse);
        Debug.Log("Zombie got knocked back ");

        //use a courtine to re-enable the navmesh agent after a short delay
        StartCoroutine(KnockbackCoroutine());
    }
    IEnumerator KnockbackCoroutine()
    {
      yield return new WaitForSeconds(knockbackDuration);
        //re-enable navmesh agent
        agent.enabled = true;
        //reser veclocity so it stops moving and not move infinite
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Zombie back to normal");

    }


    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashColor());
        Aud.PlayOneShot(ZombieHit, AudZombieHitVol);
        //animationZombieController.SetBool("ZombieHit", true);
        animationZombieController.SetTrigger("ZombieHurt");
        if (HP <= 0)
        {
            Aud.PlayOneShot(ZombiDeath, AudZombietDeathVol);
         

            GameManger.Instance.updateGameGoal(-1);
            animationZombieController.SetTrigger("ZombieDeath");
            //  Destroy(gameObject);
            Debug.Log("ZombieDead as hell" );

        }
        //animationZombieController.SetBool("ZombieHit", false);
        //add a if check if the player's simple 3 hit combo is true then apply knockback 
        ApplyKnockback(-transform.forward, 1f);

    }
    public void Patroling()
    {
        if (!IsWalking)
        {
            SearchWalkpath();
        }
        if (IsWalking)
        {
            if (!isPlayingStop) playSteps();
            agent.SetDestination(WalkPoint);

           

        }

        Vector3 DistanceWalking = transform.position - WalkPoint;
        animationZombieController.SetFloat("ZombieSpeed", 0);

        if (DistanceWalking.magnitude < 1f)
        {
            IsWalking = false;
            animationZombieController.SetFloat("ZombieSpeed", 1);

        }
    }
    private void SearchWalkpath()
    {
        float RandomZ = Random.Range(-walkpointRange, walkpointRange);
        float RandomX = Random.Range(-walkpointRange, walkpointRange);
        WalkPoint = new Vector3(transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ);

        if (Physics.Raycast(WalkPoint, -transform.up, 2f, Ground))
        {
            IsWalking = true;
        }

    }
    public void Chase()
    {
        agent.SetDestination(GameManger.Instance.Player.transform.position);
    }

    IEnumerator playSteps()
    {
        isPlayingStop = true;

        //play walk sound
        Aud.PlayOneShot(ZombieFootsteps[Random.Range(0, ZombieFootsteps.Length)], AudZombieFootSteps);
        yield return new WaitForSeconds(.8f);
        isPlayingStop = false;
    }

    IEnumerator flashColor()
    {
        Model.material.color = Color.red;
        yield return new WaitForSeconds(.2f);
        Model.material.color = colorOrig;
    }

}


