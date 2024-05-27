using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class UnitTransporter : Unit
{
    //TODO: Add UI to show if units are garisoned

    public ObjectsInRange range;
    [SyncVar]public bool garrisoned = false;
    public List<Unit> garrisonedUnits = new List<Unit>();
    public Transform spawnPointBase;
    public Transform spawnPointFinder;
    public int maxCount = 5;
    int count = 0;
    public float maxSize = 1;
    public string spawnableTag = "ground";
    
    
    [Client]
    private void Update()
    {
        if (selected && Input.GetKeyDown("g") && !garrisoned)
        {
            
            CmdGarrison();
        }
        else if(selected && Input.GetKeyDown("g"))
        {
            CmdUngarrison();
        }

        KeepMoving();
    }

    private void Awake()
    {
        range.team = team;
    }

    private Vector3 FindSpawnPoint()
    {
        RaycastHit hit;
        for (int i = 0; i < 360; i++)
        {
            if (Physics.Raycast(spawnPointFinder.position, Vector3.down, out hit))
            {
                if (hit.transform.CompareTag(spawnableTag))
                {
                    
                    return hit.point;
                    
                }
            }

            spawnPointBase.Rotate(0, i, 0);
        }
        return Vector3.zero;
    }

    [Command(requiresAuthority = false)]
    private void CmdGarrison()
    {
        List<EntityBase> unitsToRemoveFromRange = new List<EntityBase>();
        foreach (EntityBase entity in range.objects)
        {
            Unit unitBase;
            if (entity.TryGetComponent(out unitBase) && unitBase.size <= maxSize && count < maxCount)
            {
                garrisoned = true;
                unitsToRemoveFromRange.Add(entity);
                unitBase.gameObject.SetActive(false);
                unitBase.ClientRpcVisible(false);
                garrisonedUnits.Add(unitBase);
                count++;
                
            }
        }

        foreach (EntityBase entity in unitsToRemoveFromRange)
        {
            range.objects.Remove(entity);
        }
        unitsToRemoveFromRange.Clear();
    }

    [Command(requiresAuthority = false)]
    private void CmdUngarrison()
    {
        
        if(garrisonedUnits.Count <= 0)
        {
            garrisoned = false;
        }
        

        Vector3 spawnPoint = FindSpawnPoint();

        if (spawnPoint == Vector3.zero)
        {
            return;
        }

        foreach (Unit unit in garrisonedUnits)
        {
            unit.transform.position = spawnPoint;
            unit.gameObject.SetActive(true);  
            unit.ClientRpcVisible(true);
             garrisoned = false;
        }

        if (!garrisoned)
        {
            garrisonedUnits.Clear();
            count = 0;
        }
    }

    

}
