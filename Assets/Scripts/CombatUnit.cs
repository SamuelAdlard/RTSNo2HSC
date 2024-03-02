using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CombatUnit : Unit
{
    public int damage;
    public int lossRateWhenMoving;
    public int lossRateWhenFighting;
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
            supplyStores -= lossRateWhenMoving;
        }

        if (Time.time > nextAttack)
        {
            nextAttack = attackDelay + Time.time;
            Attack();
        }

    }

    [Server]
    private void Attack()
    {
        RaycastHit hit;
        if(attackArea.objects.Count > 1)
        {
            EntityBase targetEntity = attackArea.objects[1];
            foreach (Transform transform in objectsToLook)
            {
                transform.LookAt(targetEntity.transform.position);
            }

            foreach (Transform turret in turrets)
            {
                
                if(Physics.Raycast(transform.position, transform.forward, out hit, range))
                {
                    print(hit.transform.name);
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
