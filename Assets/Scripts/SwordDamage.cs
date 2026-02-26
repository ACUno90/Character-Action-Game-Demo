using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public Transform attackPoint;
    public Transform StickPoint;
    public LayerMask enemyLayers;
    public float attackRange = 0.5f;
    // enemies currently stuck to the stick point
    private List<GameObject> attachedEnemies = new List<GameObject>();

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
            //use animation event to call this method
            if (df.isAirLauncher)
            {
                NecromancerEnemy nerco = enemy.GetComponent<NecromancerEnemy>();
                ZombieEnemy zom = enemy.GetComponent<ZombieEnemy>();
                if (nerco != null)
                {
                    nerco.StartAirFollow(df);
                }
                else if (zom != null)
                {
                    zom.StartAirFollow(df);
                }
            }

            if (df.isStinger)
            {
                NecromancerEnemy nerco = enemy.GetComponent<NecromancerEnemy>();
                ZombieEnemy zom = enemy.GetComponent<ZombieEnemy>();
                if (nerco != null)
                {
                    nerco.StartStingFollow(df, StickPoint);
                    if (StickPoint != null && !attachedEnemies.Contains(enemy.gameObject))
                        attachedEnemies.Add(enemy.gameObject);
                }
                else if (zom != null)
                {
                    zom.StartStingFollow(df, StickPoint);
                    if (StickPoint != null && !attachedEnemies.Contains(enemy.gameObject))
                        attachedEnemies.Add(enemy.gameObject);
                }


            }
        }
   }
    // Release all enemies currently stuck to the stick point (call when stinger ends)
    public void ReleaseStuckEnemies()
    {
        for (int i = attachedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject go = attachedEnemies[i];
            if (go == null)
            {
                attachedEnemies.RemoveAt(i);
                continue;
            }

            NecromancerEnemy nerco = go.GetComponent<NecromancerEnemy>();
            if (nerco != null)
            {
                nerco.EndStingFollow();
            }
            ZombieEnemy zom = go.GetComponent<ZombieEnemy>();
            if (zom != null)
            {
                zom.EndStingFollow();
            }

            attachedEnemies.RemoveAt(i);
        }
    }
        
    

    private void OnDrawGizmosSelected()
    {
    
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    
    }
}


