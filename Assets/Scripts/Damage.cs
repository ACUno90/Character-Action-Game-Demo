using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{

    [SerializeField] enum damageType { bullet, staionary, melee }
    [SerializeField] damageType DT;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destoryTime;
    void Start()
    {
        if (DT == damageType.bullet)
        {
            rb.velocity = transform.forward * speed;
            Destroy(gameObject, destoryTime);
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
        if (DT == damageType.bullet)
        {
            Destroy(gameObject);
        }

    }

}

