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
    [SerializeField] LayerMask IgnoreEnemy;
    [SerializeField] NavMeshAgent agent;
    public LayerMask Ground, WherePlayer;
    // gravity float fields (use the single set below)
    //Patroling
    public Vector3 WalkPoint;
    bool IsWalking;
    [SerializeField] float walkpointRange;
    Color colorOrig;
    //States
    [SerializeField] float Sightrange;
    bool isinSight;
    [Header("Follow Tuning")]
    [SerializeField] float followLerp = 10f;
    [SerializeField] float verticalLerp = 8f;
    [SerializeField] float stingerStopDistance = 1f;
    [SerializeField] float airStopDistance = 0.5f;
    Player player;
    [Header("Audio")]
    [SerializeField] AudioSource Aud;
    [SerializeField] AudioClip ZombiDeath;
    [SerializeField] float AudZombietDeathVol;
    [SerializeField] AudioClip ZombieHit;
    [SerializeField] float AudZombieHitVol;
    [SerializeField] AudioClip[] ZombieFootsteps;
    [SerializeField] float AudZombieFootSteps;
    [SerializeField] AudioClip ZombieMeleeAttack;
    [SerializeField] float AudZombieMeleeAttack;
    public Animator animationZombieController;
    private Rigidbody rb;
    Collider myCollider;
    bool prevIsTrigger;
    [SerializeField] float gravityfloat;
    [SerializeField] float gravityfloatDurantion;
    bool isFloating;
    float floatEndTime;
    public float knockbackDuration = 0.5f;
    bool IsFollowingPlayer;
    bool isFoleingStingPlayerZ;
    bool isFollowingplayerAir;
    bool ZombieHurt;
    bool isPlayingStop;
    bool IsZDead;
    [Header("Combat")]
    [SerializeField] float attackCooldown = 1.2f;
    float attackTimer = 0f;
    bool pendingAttack = false;
    bool hasDealtAttack = false;
    [SerializeField] float attackRadius = 1f;
    [SerializeField] Vector3 attackOffset = new Vector3(0.5f, 0.5f, 0f);
    void Start()
    {
        isPlayingStop = false;
     //   colorOrig = Model.material.color;
        GameManger.Instance.updateGameGoal(1);
       rb = GetComponent<Rigidbody>();
       myCollider = GetComponent<Collider>();
       if (myCollider != null) prevIsTrigger = myCollider.isTrigger;
    }

    public void StartFloat()
    {
        isFloating = true;
        floatEndTime = Time.time + gravityfloatDurantion;
    }

    void UpdateFloatState()
    {
        if (isFloating && Time.time > floatEndTime)
        {
            isFloating = false;
        }
    }




    void Update()
    {
        if(IsZDead) return;
        UpdateFloatState();
        isinSight = Physics.CheckSphere(transform.position, Sightrange, WherePlayer);
        if (!isinSight)
        {
            Patroling();

        }
        else // if in sight, either chase or attack depending on distance
        {
            // update attack timer
            attackTimer -= Time.deltaTime;

            // distance to player using NavMeshAgent remaining distance when path is set
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(GameManger.Instance.Player.transform.position);
                float remaining = agent.remainingDistance;
                if (!agent.pathPending && remaining <= agent.stoppingDistance + 0.5f)
                {
                    // within melee range -> trigger attack if not on cooldown
                    if (attackTimer <= 0f && !pendingAttack)
                    {
                        StartMeleeAttack();
                    }
                }
                else
                {
                    // not in attack range yet
                    Chase();
                }
            }
            else
            {
                Chase();
            }
        }

        // horizontal follow: only run when following horizontally and player available
        if (IsFollowingPlayer && player != null)
        {
            // steer toward player's horizontal position (better control than copying controller velocity)
            Vector3 toPlayer = player.transform.position - transform.position;
            Vector3 horizontalDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            float sqrDist = horizontalDir.sqrMagnitude;
            if (sqrDist <= stingerStopDistance * stingerStopDistance)
            {
                IsFollowingPlayer = false;
            }
            else if (sqrDist > 0.001f)
            {
                horizontalDir.Normalize();
                Vector3 desired = horizontalDir * player.StingerForce;
                Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 neededChange = desired - currentHorizontal;
                // use the same instant velocity-change approach as Necromancer for tighter tracking
                rb.AddForce(neededChange * followLerp, ForceMode.VelocityChange);
            }
        }

        // vertical follow for air launcher: mirror Necromancer logic
        if (isFollowingplayerAir && player != null)
        {
            UpdateFloatState();
            float toPlayerY = player.transform.position.y - transform.position.y;
            float distY = Mathf.Abs(toPlayerY);
            if (distY <= airStopDistance)
            {
                isFollowingplayerAir = false;
            }
            else
            {
                float desiredY = Mathf.Clamp(toPlayerY, -player.AirLauncherForce, player.AirLauncherForce);
                float newY = Mathf.Lerp(rb.linearVelocity.y, desiredY, Time.deltaTime * verticalLerp);
                float neededChangeY = newY - rb.linearVelocity.y;
                rb.AddForce(Vector3.up * neededChangeY, ForceMode.VelocityChange);
            }

            if (player.GetVerticalVelocity() <= 0)
            {
                isFollowingplayerAir = false;
            }
        }

    }

    public void StartStingFollow(Player p, Transform stickPoint)
    {
         player = p;
        //reset rb velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // if a stickPoint is provided, attach to it
        if (stickPoint != null)
        {
            if (agent != null) agent.enabled = false;
            rb.isKinematic = true;
            transform.SetParent(stickPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            IsFollowingPlayer = false;
            isFoleingStingPlayerZ = false;
            // disable physical collisions while attached so weapon/player don't push the enemy away
            if (myCollider != null) myCollider.isTrigger = true;
            // ignore collisions with the player while attached to stick
            Collider playerCol = GameManger.Instance.Player.GetComponent<Collider>();
            if (playerCol == null) playerCol = GameManger.Instance.Player.GetComponentInChildren<Collider>();
            if (playerCol != null && myCollider != null)
                Physics.IgnoreCollision(myCollider, playerCol, true);
        }
        else
        {
            // initial horizontal impulse
            IsFollowingPlayer = true;
            if (agent != null) agent.enabled = false;
            // push toward player's horizontal direction for a tighter stinger launch
            Vector3 toPlayer = (player.transform.position - transform.position);
            Vector3 horizontalDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (horizontalDir.sqrMagnitude > 0.001f)
            {
                horizontalDir.Normalize();
                rb.isKinematic = false;
                rb.AddForce(horizontalDir * player.StingerForce, ForceMode.VelocityChange);
            }
        }
    }

    public void StartAirFollow(Player p)
    {
        StartAirFollow(p, Vector3.zero);
        isFollowingplayerAir = true;
    }

    // overload to allow specifying horizontal launch direction (used for chain collisions)
    public void StartAirFollow(Player p, Vector3 horizontalDir)
    {
        player = p;
        isFollowingplayerAir = true;
        //reset rb velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // ensure agent is disabled and physics active so physics affects the zombie like Necromancer
        if (agent != null) agent.enabled = false;
        rb.isKinematic = false;
        if (myCollider != null) myCollider.isTrigger = false;
        // initial upward impulse (set vertical velocity directly like Necromancer)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, player.AirLauncherForce, rb.linearVelocity.z);
        // start reduced gravity float so the player can follow-up in air
        StartFloat();
    }


    public void EndStingFollow()
    {
      
        //re-enable navmesh agent
        if (agent != null)
            agent.enabled = true;
        IsFollowingPlayer = true;
       isFoleingStingPlayerZ = true;
        //detach from player
        transform.SetParent(null);
        if (myCollider != null) myCollider.isTrigger = prevIsTrigger;
        // re-enable collisions with player
        Collider playerCol = GameManger.Instance.Player.GetComponent<Collider>();
        if (playerCol == null) playerCol = GameManger.Instance.Player.GetComponentInChildren<Collider>();
        if (playerCol != null && myCollider != null)
            Physics.IgnoreCollision(myCollider, playerCol, false);


    }

    private void OnCollisionEnter(Collision collision)
    {
        // chain-launch other enemies if this zombie is airborne and hits them with enough relative velocity
        if (rb == null) return;
        float speed = rb.linearVelocity.magnitude;
        if (speed < 2f) return; // require some momentum to transfer

        var otherZ = collision.gameObject.GetComponent<ZombieEnemy>();
        var otherN = collision.gameObject.GetComponent<NecromancerEnemy>();

        // compute a horizontal launch direction based on collision contact
        Vector3 horizontalDir = Vector3.zero;
        if (collision.contactCount > 0)
        {
            Vector3 contactNormal = collision.GetContact(0).normal;
            // reflect our velocity across the contact normal and take horizontal component toward player
            Vector3 reflect = Vector3.Reflect(rb.linearVelocity.normalized, contactNormal);
            horizontalDir = new Vector3(reflect.x, 0f, reflect.z).normalized;
        }

        if (otherZ != null)
        {
            otherZ.StartAirFollow(GameManger.Instance.PlayerScript, horizontalDir);
            otherZ.StartFloat();
        }
        else if (otherN != null)
        {
            otherN.StartAirFollow(GameManger.Instance.PlayerScript, horizontalDir);
            otherN.StartFloat();
        }

        // If we hit the ground while airborne/being followed, transition back to NavMesh control
        // ground detection via layer mask
        if ((Ground.value & (1 << collision.gameObject.layer)) != 0)
        {
            // consider it a landing if vertical speed is low or we were following in air/float
            if (Mathf.Abs(rb.linearVelocity.y) < 1f || isFollowingplayerAir || isFloating)
            {
                LandFromAir();
            }
        }
    }

    void LandFromAir()
    {
        // stop physics movement and restore NavMesh control
        if (agent != null) agent.enabled = true;
        // switch back to kinematic so agent controls transform
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
      //  rb.velocity = Vector3.zero;
        // reset follow/float flags
        isFollowingplayerAir = false;
        IsFollowingPlayer = false;
        isFoleingStingPlayerZ = false;
        isFloating = false;
        // trigger a get-up animation if present (add "ZombieGetUp" trigger in Animator)
        if (animationZombieController != null)
            animationZombieController.SetTrigger("ZombieGetUp");
        Debug.Log("Zombie landed and returned to NavMesh control");
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Only deal damage from trigger collisions if an attack is currently pending
        if (!pendingAttack) return;
        if (hasDealtAttack) return;

        Player p = collision.GetComponent<Player>();
        if (p == null) p = collision.GetComponentInParent<Player>();
        if (p != null)
        {
            p.takeDamage(damage);
            hasDealtAttack = true;
        }
    }
    public void ApplyKnockback(Vector3 direction, float force)
    {
        // If the zombie is currently attached to or being tracked by the player
        // (stinger or air-launcher) or is floating/kinematic, don't apply knockback.
        if (isFollowingplayerAir || IsFollowingPlayer || isFoleingStingPlayerZ || isFloating || (rb != null && rb.isKinematic))
        {
            Debug.Log("ApplyKnockback skipped due to air/stinger/floating/kinematic state");
            return;
        }
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
           IsZDead = true;
            if (agent != null) agent.enabled = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // schedule destroy
            Destroy(gameObject, 3f);

        }
        //animationZombieController.SetBool("ZombieHit", false);
        //add a if check if the player's simple 3 hit combo is true then apply knockback 
        // Don't apply knockback if the enemy is currently attached to the player (stinger)
        // or is being tracked by the player's air-launcher. Also avoid applying knockback
        // while the rigidbody is kinematic or while floating.
        //if (!isFollowingplayerAir && !IsFollowingPlayer && !isFoleingStingPlayerZ && !isFloating && !rb.isKinematic)
        //{
        //    ApplyKnockback(-transform.forward, 1f);
        //}

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
    // start attack process: trigger animation and mark pending so damage only applies on hit frame
    void StartMeleeAttack()
    {
        pendingAttack = true;
        hasDealtAttack = false;
        // set cooldown so we don't re-trigger immediately
        attackTimer = attackCooldown;
        if (animationZombieController != null)
        {
            animationZombieController.SetTrigger("ZombieAttack");
        }
    }

    // Called from an animation event at the attack hit frame
    public void OnMeleeAttackHit()
    {
        if (!pendingAttack || hasDealtAttack) return;
        Vector3 center = transform.position + transform.forward * attackOffset.x + Vector3.up * attackOffset.y + transform.right * attackOffset.z;
        Collider[] hits = Physics.OverlapSphere(center, attackRadius, WherePlayer);
        foreach (var c in hits)
        {
            Player p = c.GetComponent<Player>();
            if (p != null)
            {
                Aud.PlayOneShot(ZombieMeleeAttack, AudZombieMeleeAttack);
                p.takeDamage(damage);
            }
            else
            {
                var pd = c.GetComponentInParent<Player>();
                if (pd != null) pd.takeDamage(damage);
            }
        }
        hasDealtAttack = true;
    }

    // Called from an animation event when attack animation ends
    public void OnMeleeAttackEnd()
    {
        pendingAttack = false;
        hasDealtAttack = false;
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


