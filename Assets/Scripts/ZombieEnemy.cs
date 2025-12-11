using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] AudioSource Aud;
    [SerializeField] AudioClip roboDeath;
    [SerializeField] float AudrobotDeathVol;
    [SerializeField] AudioClip RobotHit;
    [SerializeField] float AudrobotHitVol;
    [SerializeField] AudioClip[] Footsteps;
    [SerializeField] float AudFootSteps;

    bool isPlayingStop;
    void Start()
    {
        isPlayingStop = false;
        colorOrig = Model.material.color;
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

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashColor());
        Aud.PlayOneShot(RobotHit, AudrobotHitVol);
        if (HP <= 0)
        {
            Aud.PlayOneShot(roboDeath, AudrobotDeathVol);
            int GoldDropped = Random.Range(1, 20);
          //  GameManger.Instance.PlayerScript.Gold += GoldDropped;
            Destroy(gameObject);
        }

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

        if (DistanceWalking.magnitude < 1f)
        {
            IsWalking = false;
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
        Aud.PlayOneShot(Footsteps[Random.Range(0, Footsteps.Length)], AudFootSteps);
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


