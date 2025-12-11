using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionMoves : MonoBehaviour
{
    [SerializeField] CharacterController Actioncontroller;
   // [SerializeField] LayerMask IgnorePlayer;
    [SerializeField] int AttackSpeed;
 //   [SerializeField] int SwordDamage;
    [SerializeField] int AirLauncherForce;
    [SerializeField] int AitLauncherdDamage;
    [SerializeField] int AirLauncherSpeed;//to experiment
    [SerializeField] int SimpleCombospeed;
    [SerializeField] int SimpleComboDamage;
   [SerializeField] int Gravity;
    [SerializeField] int StingerSpeed;//to experiment
    [SerializeField] int StingerDamage;
    bool isStinger;
    Vector3 moveDirc;
    Vector3 Playervalues;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StingerMove()
    {
        if(Input.GetButtonDown("Stinger")&& !isStinger)
        {
            isStinger = true;
            Actioncontroller.Move(transform.forward * StingerSpeed * Time.deltaTime);
            isStinger = false;
        }
    }

    public void AirLauncher()
    {

    }

    public void SimpleCombo()
    {

    }
}
