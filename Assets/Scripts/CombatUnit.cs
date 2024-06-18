using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static UnityEngine.GraphicsBuffer;

public class CombatUnit : Unit
{
    //Amount of damage done in an attack
    public int damage;
    //The supplies that are lost when the unit moves
    public int lossRateWhenMoving;
    //The supplies that are lost when the unit attacks
    public int lossRateWhenFighting;
    //indicates whether the unit has supplies
    public bool hasSupplies;
    //The object that are to look that the enemy when attacking
    public List<Transform> objectsToLook = new List<Transform>();
    //The points from the unit that the rays are casted from
    public List<Transform> turrets = new List<Transform>();
    //The entites that are in range of the unit
    public ObjectsInRange attackArea;
    //The delay between attacks
    public float attackDelay = 1;
    //The range that the ray shoots
    public float range = 1;
    //The linerenderer to indicate the shot
    public LineRenderer lineRenderer;
    //The amount of time the render line is visible
    public float renderTime = 1f;
    //The time of the next attack Time.time + attackdelay
    float nextAttack;
    //The amount of time between losing supplies
    float lossDelay = 1;
    //The next time supplies will be lost Time.time + lossDelay
    float nextLoss;
    
    private void Start()
    {
        attackArea.team = team;
    }

    /// <summary>
    /// Runs every frame on the server and runs the attack and LoseSupplies function after a given delay
    /// </summary>
    [ServerCallback]
    private void Update()
    {
        if (navMeshAgent.velocity.magnitude > 0.5f  && Time.time > nextLoss)
        {
            nextLoss = Time.time + lossDelay;
            LoseSupplies(lossRateWhenMoving);
        }
        
        if (Time.time > nextAttack && hasSupplies)
        {
            nextAttack = attackDelay + Time.time;
            
            Attack();
        }

       
        
    }

    /// <summary>
    /// Runs when the unit is given supplies and handles receiving the supplies
    /// </summary>
    /// <param name="amount">The amount of supplies that are given</param>
    public void GetSupplies(int amount)
    {
        supplyStores += amount;
        hasSupplies = true;
    }

    /// <summary>
    /// Runs when the unit loses supplies handles setting hasSupplies to false if the amount of supplies is zero
    /// </summary>
    /// <param name="amount">The amount of supplies to remove</param>
    public void LoseSupplies(int amount)
    {
        if((supplyStores - amount) <= 0) //Makes sure supplies do not go below zero
        {
            hasSupplies = false;
            supplyStores = 0;
        }
        else
        {
            supplyStores -= amount;
        }
    }

    /// <summary>
    /// Runs after the attack delay and takes some offensive action when there are units in range
    /// </summary>
    [Server]
    private void Attack()
    {
        
        
        
        if (attackArea.objects.Count > 0)
        {
            ShootAtObject();
        }
    }
    /// <summary>
    /// Targets the first unit in the attackArea.objects list and shoots at it by casting out rays from the turrets on the unit.
    /// If the ray collides with an enemy unit the attack damage is subtracted from the health. If the unit in the attack area list is null it is removed from the list.
    /// This function rotates all objects in the objectToLook towards the target object. The same is done for objects in the turret list in order to cast rays in the correct direction.
    /// Ray casts are used in this context so that walls and other objects can be used as cover for units.
    /// This function will also render the shot toward the enemy
    /// </summary>
    private void ShootAtObject()
    {
        RaycastHit hit;
        EntityBase targetEntity = attackArea.objects[0];
        if (targetEntity == null)
        {
            attackArea.objects.RemoveAt(0);
            return;
        }

        foreach (Transform transform in objectsToLook)
        {
            transform.LookAt(targetEntity.transform.position);
        }

        foreach (Transform turret in turrets)
        {
            Debug.DrawRay(turret.transform.position, turret.transform.forward * 5, Color.green, 0.1f);

            if (Physics.Raycast(turret.position, turret.forward, out hit, range))
            {
                ClientRPCStartRenderAttack(turrets.IndexOf(turret), hit.point);
                LoseSupplies(lossRateWhenFighting);
                EntityBase entityBase;
                if (hit.transform.TryGetComponent(out entityBase) && entityBase.team != team)
                {

                    if (entityBase.TakeDamage(damage) && attackArea.objects.Contains(entityBase))
                    {
                        attackArea.objects.Remove(entityBase);

                    }
                }
            }
            else
            {
                ClientRPCStartRenderAttack(turrets.IndexOf(turret), turret.transform.forward * range);
            }
        }
    }

    /// <summary>
    /// Renders a line between the turret position on a unit and the point that a ray was cast to.
    /// This function uses the lineRenderer component by setting the start and the end location of the line
    /// </summary>
    /// <param name="turretIndex">The index of the turret in the turret list</param>
    /// <param name="attackPosition">The location that the ray collided with</param>
    [ClientRpc]
    private void ClientRPCStartRenderAttack(int turretIndex,Vector3 attackPosition)
    {
        lineRenderer.SetPosition(0, turrets[turretIndex].position);
        lineRenderer.SetPosition(1, attackPosition);
        lineRenderer.enabled = true;
        StartCoroutine(StopRenderAttack());
    }

    /// <summary>
    /// Turns off the lineRenderer after the renderTime has passed
    /// </summary>
    /// <returns></returns>
    private IEnumerator StopRenderAttack()
    {
        yield return new WaitForSeconds(renderTime);
        lineRenderer.enabled = false;
    }
}
