using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class ResourceProductionBuilding : Building
{
    public int productionRate = 1;
    public float WaitTime = 1;
    float nextProduction = 0;

    /// <summary>
    /// Makes supplies after the wait time has passed, makes sure the amount of supplies made cannot excede the the maximum capacity of the building
    /// </summary>
    [ServerCallback]
    public override void Update()
    {
        base.Update();
        if (Time.time > nextProduction && supplyStores < maximumCapacity && functional)
        {
            nextProduction = Time.time + WaitTime;
            supplyStores += productionRate;
        }
    }
}
