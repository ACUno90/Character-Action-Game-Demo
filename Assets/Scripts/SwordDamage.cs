using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackRange = 0.5f;
   public void SwordAttack()
    {
        Player df = Object.FindFirstObjectByType<Player>();
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            IDamage dmg = enemy.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(10);
            }
            Debug.Log("We hit " + enemy.name);
            // add if check here if we do it and hit here launch enemy up
           
            if (df.isAirLauncher)
            {
               NecromancerEnemy nerco = enemy.GetComponent<NecromancerEnemy>(); 
                if(nerco != null)
                {
                    nerco.StartAirFollow(df);
                }
            }
          
          
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

