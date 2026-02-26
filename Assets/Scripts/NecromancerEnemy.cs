using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class NecromancerEnemy : MonoBehaviour,IDamage
{
    [Header("Basics")]
    [SerializeField] NavMeshAgent Agent;
    [SerializeField] int HP;
    [SerializeField] LayerMask IgnoreEnemy;
    [SerializeField] Renderer Model;
    [SerializeField] Transform headPos;
    public LayerMask Ground, WherePlayer;
    private Rigidbody rb;
    public float knockbackDuration = 0.5f;
    [Header("Bullet")]
    [SerializeField] Transform Shotpostion;
    [SerializeField] GameObject Bullet;
    [SerializeField] float shootrate;
    [SerializeField] float shootForce;
    [SerializeField] float shootUpForce;
    public bool airBorne;
    public float NGravity =20f;
    Color colorOrig;
    bool Isshooting;
    [Header("Follow Tuning")]
    [SerializeField] float followLerp = 10f;
    [SerializeField] float verticalLerp = 8f;
    [SerializeField] float stingerStopDistance = 1f;
    [SerializeField] float airStopDistance = 0.5f;

    //Patroling
    [Header("Patroll")]
    public Vector3 WalkPoint;
    bool IsWalking;
    [SerializeField] float walkpointRange;

    //States
    [Header("States")]
    [SerializeField] float Sightrange;
    [SerializeField] float Shootrange;
    bool isinSight;
    bool isinRange;

    // Audio 
    [SerializeField] AudioSource Aud;
    [SerializeField] AudioClip NecromancerDeath;
    [SerializeField] float AudNecromancerDeathVol;
    [SerializeField] AudioClip NecromancerHit;
    [SerializeField] float AudNecromancerHitVol;
    [SerializeField] AudioClip NecromancerFireBall;
    [SerializeField] float AudNecromancerFireBall;
    [SerializeField] AudioClip[] NecromancerFootsteps;
    [SerializeField] float AudNecromancerFootSteps;
     int damage;

    bool isPlayingStop;
    public Animator animationNecroController;
    bool NercroEnemyHurt;
    bool isFollowingplayer;
    bool isFollwingStingPlayer;
    Player player;
    bool isStingerFollowed;
    bool isNDead;
    private bool isStingerAttached;
    private Transform stingerAttachPoint;

    void Start()
    {

        GameManger.Instance.updateGameGoal(1);
        rb = GetComponent<Rigidbody>();
      //  colorOrig = Model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (isNDead) return;
        isinSight = Physics.CheckSphere(transform.position, Sightrange, WherePlayer); // checks how far it can see the player and what you want to put im for it
        isinRange = Physics.CheckSphere(transform.position, Shootrange, WherePlayer); //Checks how far it can shoot the player and what you want to put im for it

        if (!isinSight) // if not in sight, it patrols around its area
        {
            PatrolingArea();

        }
        if (isinSight && !isinRange) // If it sees you,it will chase you but not attack you until your in range
        {
            Chasing();
        }
        if (isinSight && isinRange)   // if in sight and range to attack, it would start shooting you
        {

            Shooting();

        }
    


        // Stinger follow: actively steer toward player's horizontal position for tighter tracking
        if (isFollwingStingPlayer && player != null)
        {
            Vector3 toPlayer = player.transform.position - transform.position;
            Vector3 horizontalDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            float sqrDist = horizontalDir.sqrMagnitude;
            // stop following when close enough
            if (sqrDist <= stingerStopDistance * stingerStopDistance)
            {
                isFollwingStingPlayer = false;
            }
            else if (sqrDist > 0.001f)
            {
                horizontalDir.Normalize();
                // use AddForce for more physics-driven response
                Vector3 desired = horizontalDir * player.StingerForce;
                // compute required change in velocity and apply as velocity change force
                Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                Vector3 neededChange = desired - currentHorizontal;
                // apply force as VelocityChange for immediate effect, scaled by followLerp for tuning
                rb.AddForce(neededChange * followLerp, ForceMode.VelocityChange);
            }
        }

        // Air launcher follow: track vertical movement toward player's height
        if (isFollowingplayer && player != null)
        {
            float toPlayerY = player.transform.position.y - transform.position.y;
            float distY = Mathf.Abs(toPlayerY);
            if (distY <= airStopDistance)
            {
                isFollowingplayer = false;
            }
            else
            {
                // desired vertical velocity to close the height gap
                float desiredY = Mathf.Clamp(toPlayerY, -player.AirLauncherForce, player.AirLauncherForce);
                float newY = Mathf.Lerp(rb.linearVelocity.y, desiredY, Time.deltaTime * verticalLerp);
                float neededChangeY = newY - rb.linearVelocity.y;
                rb.AddForce(Vector3.up * neededChangeY, ForceMode.VelocityChange);
            }
        }



    }
    public void StartAirFollow(Player p)
    {
        player = p;
        isFollowingplayer = true;
        //reset rb velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // initial upward impulse
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, player.AirLauncherForce, rb.linearVelocity.z);
    }

    public void StartStingFollow(Player p, Transform stickPoint)
    {
        player = p;
        // reset velocities
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (stickPoint != null)
        {
            //// immediately attach to the provided stick point
            //isStingerAttached = true;
            //if (Agent != null) Agent.enabled = false;
            //rb.isKinematic = true;
            //// snap to stick point and remember it for per-frame alignment
            //stingerAttachPoint = stickPoint;
            //transform.SetParent(null);
            //transform.position = stingerAttachPoint.position;
            //transform.rotation = stingerAttachPoint.rotation;

            if (Agent != null) Agent.enabled = false;
            rb.isKinematic = true;
            transform.SetParent(stickPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            isFollwingStingPlayer = false;
            isFollowingplayer = false;
            isStingerAttached = true;
        }
        else
        {
            // follow horizontally toward player
            isFollwingStingPlayer = true;
            Vector3 toPlayer = (player.transform.position - transform.position);
            Vector3 horizontalDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (horizontalDir.sqrMagnitude > 0.001f)
            {
                horizontalDir.Normalize();
                rb.AddForce(new Vector3(horizontalDir.x * player.StingerForce, 0f, horizontalDir.z * player.StingerForce), ForceMode.VelocityChange);
            }
        }
    }
    //add this as a end 
    public void EndStingFollow()
    {
        // detach if attached to stick point
        if (isStingerAttached)
        {
            isStingerAttached = false;
            stingerAttachPoint = null;
            transform.SetParent(null);
            rb.isKinematic = false;
            if (Agent != null) Agent.enabled = true;
        }
        // stop following behaviour
        isFollwingStingPlayer = true;
        isFollowingplayer= true;

        //re-enable navmesh agent
        if (Agent != null)
            Agent.enabled = true;

        //detach from player
       // transform.SetParent(null);
    }

    // public void Launch(float launchForce)
    // {
    //    // rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
    //    airBorne = true;
    //   //add launchForce to y velocity
    //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, launchForce, rb.linearVelocity.z);
    ////turn off navmesh agent
    //        if (Agent !=null)
    //         Agent.enabled = false;

    // }
    public void EndLaunch()
    {
               //airBorne = false;
        //re-enable navmesh agent
        if (Agent != null)
            Agent.enabled = true;

    }

    //Stinger player follow



    private void Shooting()
    {
        // Check for a clear line of sight before shooting
        RaycastHit hit;
        Vector3 directionToPlayer = GameManger.Instance.Player.transform.position - transform.position;
       // Perform the raycast to check for obstacles between enemy and player
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, Shootrange))
            {
                Transform Parent = hit.transform.parent;

                // Check if the raycast hit the player
                if (hit.transform.CompareTag("Player") || Parent != null)
                {
                    if (!hit.transform.CompareTag("Player") && Parent != null)
                    {
                        if (Parent.CompareTag("Player"))
                        {
                            InstantiateBullet(directionToPlayer);
                        }
                    }
                    else
                    {
                        InstantiateBullet(directionToPlayer);
                    }
                }
            }
    }

    private void InstantiateBullet(Vector3 directionToPlayer)
    {

        // Make the enemy face the player
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation

        Agent.SetDestination(GameManger.Instance.Player.transform.position);
        if (!Isshooting)
        {
            Aud.PlayOneShot(NecromancerFireBall, AudNecromancerFireBall);
            // Calculate the direction towards the player
            Vector3 shootDirection = (GameManger.Instance.Player.transform.position - Shotpostion.position).normalized;

            // Instantiate the bullet
            GameObject bulletInstance = Instantiate(Bullet, Shotpostion.position, Quaternion.identity);
            Rigidbody body = bulletInstance.GetComponent<Rigidbody>();

            Collider enemyCollider = GetComponent<Collider>();
            Collider bulletCollider = bulletInstance.GetComponent<Collider>();

            // Ignore collision between enemy and bullet
            Physics.IgnoreCollision(enemyCollider, bulletCollider);

            //enemy no shoot himself
            Physics.IgnoreCollision(enemyCollider, bulletCollider);

            body.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            body.AddForce(transform.up * shootUpForce, ForceMode.Impulse);


            //
            Isshooting = true;
            Invoke(nameof(ResetShooting), shootrate);
        }
    }

    IEnumerator flashColor()
    {
        Model.material.color = Color.red;
        yield return new WaitForSeconds(.2f);
        Model.material.color = colorOrig;
    }
    public void ApplyKnockbackNercro(Vector3 direction, float force)
    {
        // StartCoroutine(KnockbackCoroutine(direction, force));
        //disbale navmesh agent
        Agent.enabled = false;
        //calculate knockback vector
        Vector3 knockbackVector = direction.normalized * force;
        //apply an impulse force to the rigidbody
        rb.AddForce(knockbackVector, ForceMode.Impulse);
        Debug.Log("Zombie got knocked back ");

        //use a courtine to re-enable the navmesh agent after a short delay
        StartCoroutine(KnockbackNercroCoroutine());
    }
    IEnumerator KnockbackNercroCoroutine()
    {
        yield return new WaitForSeconds(knockbackDuration);
        //re-enable navmesh agent
        Agent.enabled = true;
        //reser veclocity so it stops moving and not move infinite
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Necromancer back to normal");

    }
    public void takeDamage(int amount)
    {

        HP -= amount;
        StartCoroutine(flashColor());
        flashColor();
        Aud.PlayOneShot(NecromancerHit, AudNecromancerHitVol);
        animationNecroController.SetTrigger("NercoHurt");
        if (HP <= 0)
        {
            Aud.PlayOneShot(NecromancerDeath, AudNecromancerDeathVol);

            // GameManager.Instance.PlayerScript.Gold += GoldDropped;

            GameManger.Instance.updateGameGoal(-1);
            animationNecroController.SetTrigger("NercoDie");
            Debug.Log("Necromancer dead" );
            // stop movement and disable agent/physics
            isNDead = true;
            if (Agent != null) Agent.enabled = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // schedule destroy
            Destroy(gameObject, 3f);
        }
        //  animationNecroController.SetBool("GotHitN", false);
        ApplyKnockbackNercro(-transform.forward, 2f);

    }

    public void PatrolingArea()
    {
        if (!IsWalking)  // if its not walking, it will begin to walk in its range
        {
            SearchWalkroad();
        }
        if (IsWalking) // if it is walking, it would search what its walk range is and move around in that range
        {
            if (!isPlayingStop) playSteps();
            Agent.SetDestination(WalkPoint);
        }

        Vector3 DistanceWalking = transform.position - WalkPoint;  // calucating its walking distance 
        animationNecroController.SetFloat("speed", 0);

        if (DistanceWalking.magnitude < 1f) // if its lower than one, you reached the walkpoint and stopped walking and will search for a new one
        {
            IsWalking = false;
            animationNecroController.SetFloat("speed", 1);

        }
    }
    private void SearchWalkroad()
    {
        float RandomZ = Random.Range(-walkpointRange, walkpointRange); // randomizing the range where it would walk on Z plane
        float RandomX = Random.Range(-walkpointRange, walkpointRange); // randomizing the range where it would walk on x plane
        WalkPoint = new Vector3(transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ); // adds the random range to the enemy amd keep Y the same

        if (Physics.Raycast(WalkPoint, -transform.up, 2f, Ground)) // to check if its on the groud of the map and will walk if it is
        {
            IsWalking = true;
        }

    }
    public void Chasing()
    {
        Agent.SetDestination(GameManger.Instance.Player.transform.position); // Chases the player
    }

    private void ResetShooting()
    {
        Isshooting = false;
    }


    private void OnTriggerEnter(Collider collision)
    {
        IDamage hit = collision.GetComponent<IDamage>();
        if (hit != null) hit.takeDamage(damage);
       if (collision.GetComponentInParent<IDamage>() != null)
        {
            hit = collision.GetComponentInParent<IDamage>();
            hit.takeDamage(damage);
        }
    }
    IEnumerator playSteps()
    {
        isPlayingStop = true;

        //play walk sound
        Aud.PlayOneShot(NecromancerFootsteps[Random.Range(0, NecromancerFootsteps.Length)], AudNecromancerFootSteps);
        yield return new WaitForSeconds(.8f);
        isPlayingStop = false;
    }
 
}

