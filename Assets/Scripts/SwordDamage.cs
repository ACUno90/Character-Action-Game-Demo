using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    /*
    PSEUDOCODE / PLAN (detailed):

    - Problem: Calls to `StartAirFollow` currently pass only the `Player` parameter,
      but the signature requires a `Vector3 horizontalDir` second parameter.
      This causes CS7036.

    - Fix approach:
      1. For each `enemy` hit in `SwordAttack`, when `df.isAirLauncher` is true,
         compute a horizontal direction vector from the `Player` to the `enemy`.
      2. Build `horizontalDir` by subtracting positions, zeroing the Y component,
         and normalizing. If the computed vector is near zero, fall back to the
         player's forward direction to avoid passing a zero vector.
      3. Pass the computed `horizontalDir` to both `NecromancerEnemy.StartAirFollow`
         and `ZombieEnemy.StartAirFollow` calls.
      4. Preserve all existing behavior (damage, StartFloat, TriggerFloat).
      5. Keep changes localized to `SwordDamage.cs` and do not remove any lines.

    - Implementation details:
      - Use `enemy.transform.position` and `df.transform.position` to compute direction.
      - Use small epsilon via `sqrMagnitude` to check for zero-length.
      - Reuse the same `horizontalDir` for `nerco` and `zom` calls.
    */

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
                // Compute a horizontal direction from enemy toward the player and normalize it.
                // This ensures the enemy is launched toward the player (so the player can follow),
                // instead of away from the player which would push it back.
                Vector3 horizontalDir = df.transform.position - enemy.transform.position;
                horizontalDir.y = 0f;
                if (horizontalDir.sqrMagnitude > 0.0001f)
                {
                    horizontalDir.Normalize();
                }
                else
                {
                    // Fallback to player's forward if positions are nearly identical
                    horizontalDir = df.transform.forward;
                    horizontalDir.y = 0f;
                    if (horizontalDir.sqrMagnitude > 0.0001f)
                        horizontalDir.Normalize();
                }

                NecromancerEnemy nerco = enemy.GetComponent<NecromancerEnemy>();
                ZombieEnemy zom = enemy.GetComponent<ZombieEnemy>();
                if (nerco != null)
                {
                    nerco.StartAirFollow(df, horizontalDir);
                    nerco.StartFloat();
                    df.TriggerFloat();
                }
                else if (zom != null)
                {
                    zom.StartAirFollow(df, horizontalDir);
                    zom.StartFloat();
                    df.TriggerFloat();
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


