using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{

    private enum DamageType { Bullet, Stationary, Melee }

    [SerializeField] DamageType DT;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destoryTime;
    void Start()
    {
        if (DT == DamageType.Bullet)
        {
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destoryTime);
        }
        if(DT == DamageType.Stationary)
        {
            //Destroy(gameObject, destoryTime);
        }
        if(DT == DamageType.Melee)
        {
           // Destroy(gameObject, destoryTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            dmg.takeDamage(damageAmount);
        }
        if (DT == DamageType.Bullet)
        {
            Destroy(gameObject);
        }

    }

}

