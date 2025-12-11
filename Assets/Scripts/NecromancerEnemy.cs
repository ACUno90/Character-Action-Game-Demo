using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("Basics")]
    [SerializeField] NavMeshAgent Agent;
    [SerializeField] int HP;
    [SerializeField] Renderer Model;
    [SerializeField] Transform headPos;
    public LayerMask Ground, WherePlayer;

    [Header("Bullet")]
    [SerializeField] Transform Shotpostion;
    [SerializeField] GameObject Bullet;
    [SerializeField] float shootrate;
    [SerializeField] float shootForce;
    [SerializeField] float shootUpForce;
    Color colorOrig;
    bool Isshooting;

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
    [SerializeField] AudioClip roboDeath;
    [SerializeField] float AudrobotDeathVol;
    [SerializeField] AudioClip RobotHit;
    [SerializeField] float AudrobotHitVol;
    [SerializeField] AudioClip RobotLaser;
    [SerializeField] float AudRobotLaser;
    [SerializeField] AudioClip[] Footsteps;
    [SerializeField] float AudFootSteps;

    bool isPlayingStop;
    void Start()
    {


        colorOrig = Model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
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

    }


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
            Aud.PlayOneShot(RobotLaser, AudRobotLaser);
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
        yield return new WaitForSeconds(.15f);
        Model.material.color = colorOrig;
    }
    public void takeDamage(int amount)
    {

        HP -= amount;
        StartCoroutine(flashColor());
        flashColor();
        Aud.PlayOneShot(RobotHit, AudrobotHitVol);
        if (HP <= 0)
        {
            Aud.PlayOneShot(roboDeath, AudrobotDeathVol);
            int GoldDropped = Random.Range(1, 20);
           // GameManager.Instance.PlayerScript.Gold += GoldDropped;
            Destroy(gameObject);
        }

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

        if (DistanceWalking.magnitude < 1f) // if its lower than one, you reached the walkpoint and stopped walking and will search for a new one
        {
            IsWalking = false;
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

    IEnumerator playSteps()
    {
        isPlayingStop = true;

        //play walk sound
        Aud.PlayOneShot(Footsteps[Random.Range(0, Footsteps.Length)], AudFootSteps);
        yield return new WaitForSeconds(.8f);
        isPlayingStop = false;
    }
}

