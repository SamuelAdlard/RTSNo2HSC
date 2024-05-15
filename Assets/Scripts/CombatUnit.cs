using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static UnityEngine.GraphicsBuffer;

public class CombatUnit : Unit
{
    public int damage;
    public int lossRateWhenMoving;
    public int lossRateWhenFighting;
    public bool hasSupplies;
    public List<Transform> objectsToLook = new List<Transform>();
    public List<Transform> turrets = new List<Transform>();
    public ObjectsInRange attackArea;
    public float attackDelay = 1;
    public float range = 1;
    float nextAttack;
    float lossDelay = 1;
    float nextLoss;

    private void Awake()
    {
        attackArea.team = team;
    }

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


    public void GetSupplies(int amount)
    {
        supplyStores += amount;
        hasSupplies = true;
    }

    public void LoseSupplies(int amount)
    {
        if((supplyStores - amount) <= 0)
        {
            hasSupplies = false;
            supplyStores = 0;
        }
        else
        {
            supplyStores -= amount;
        }
    }

    [Server]
    private void Attack()
    {
        //print(transform.name + attackArea.objects.Count);
        RaycastHit hit;
        
        if (attackArea.objects.Count > 0)
        {
            
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
            }
        }
    }
}
