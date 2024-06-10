using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class UnitTransporter : Unit
{
    //TODO: Add UI to show if units are garisoned
    //Script on the colider that registers the units in range
    public ObjectsInRange range;
    //boolean that states if the transporter contains troops
    [SyncVar]public bool garrisoned = false;
    //List of units within the transporter unit
    public List<Unit> garrisonedUnits = new List<Unit>();
    //The pivot that is rotated to allow the spawnPointFinder to rotate in a circle
    public Transform spawnPointBase;
    //The point at which a ray is casted to find a location on the ground
    public Transform spawnPointFinder;
    //Maximum number of units allowed inside the transporter unit
    public int maxCount = 5;
    //number of units inside the transporter unit
    int count = 0;
    //The maximum size of the units that are allow inside the transporter unit (units of size 2 cannot be garrisoned for example). Done to prevent ships and planes from being garrisoned
    public float maxSize = 1;
    //The tag that the units can spawn on
    public string spawnableTag = "ground";
    
    
    /// <summary>
    /// This function runs every frame and checks if the player is pressing g to decide whether or not to garrison the nearby troops, or ungarrison the garrisoned units
    /// </summary>
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

        
    }

    void Start()
    {
        //Sets the team of the colider of the team to the unit so the colider knows which units to detect
        range.team = team;
    }

    /// <summary>
    /// The function rotates a gameobject around a pivot and casts a ray down to see if the tag of the object below matchs the spawnable tag of the 
    /// script.
    /// </summary>
    /// Returns a vector3 for a valid spawnpoint for the units or returns a zero vector if there is no location
    /// <returns></returns>
    private Vector3 FindSpawnPoint()
    {
        RaycastHit hit;
        //Rotates the gameobject in a circle around the unit
        for (int i = 0; i < 360; i++)
        {
            //Casts a ray to check the tag of the object 
            if (Physics.Raycast(spawnPointFinder.position, Vector3.down, out hit))
            {
                if (hit.transform.CompareTag(spawnableTag)) //Checks if the location is suitable
                {
                    //Returns the location found
                    return hit.point;
                    
                }
            }
            
            spawnPointBase.Rotate(0, i, 0);
        }
        //If there is no sutable location the
        return Vector3.zero;
    }

    /// <summary>
    /// Sends a command from the client to the server to garrison the units. Function loops through every unit in a spherecast of a certain range and 
    /// checks if that unit is the right size to fit inside the unit transporter, and checks to see if the unit transporter has enough free space.
    /// This function then switches of the gameobject to make it invisible on both the server and the client.
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdGarrison()
    {
        //Units to take out of the list of units in range
        List<EntityBase> unitsToRemoveFromRange = new List<EntityBase>();
        //Loops through the list of units in range and checks to see which ones are suitable
        foreach (EntityBase entity in range.objects)
        {
            
            Unit unitBase;
            if (entity.TryGetComponent(out unitBase) && unitBase.size <= maxSize && count < maxCount) //Checks if the entity is a unit and if the unit is able to fit in the unit transporter, based on the number of units in the transporter and the size of the entity
            {
                //Sets the garrisoned state to true
                garrisoned = true;
                unitsToRemoveFromRange.Add(entity);
                unitBase.gameObject.SetActive(false);
                //Switches off the gameobject on the clients
                unitBase.ClientRpcVisible(false);
                garrisonedUnits.Add(unitBase);
                count++;
                
            }
        }

        //Removes all the units that have been garrisoned from the list of units that are in range of the 
        foreach (EntityBase entity in unitsToRemoveFromRange)
        {
            range.objects.Remove(entity);
        }
        unitsToRemoveFromRange.Clear();
    }

    
    /// <summary>
    /// This function removes all units from the garrisoned list, and spawns them at the new location of the transporter unit
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdUngarrison()
    {
        //Sets the garrisoned state to false if there aren't any units in the transporter
        if(garrisonedUnits.Count <= 0) //Checks to see if the transporter actually contains units
        {
            garrisoned = false;
        }
        
        //Finds a valid spawnpoint for the units
        Vector3 spawnPoint = FindSpawnPoint();

        //Doesn't spawn the units if no valid location is found
        if (spawnPoint == Vector3.zero)
        {
            return;
        }

        //Loops through the list of garrisoned units, changes their location to the spawn point and makes them visible again for both the server and the client
        foreach (Unit unit in garrisonedUnits)
        {
            unit.transform.position = spawnPoint;
            unit.gameObject.SetActive(true);  
            unit.ClientRpcVisible(true);
             garrisoned = false;
        }

        if (!garrisoned)
        {
            garrisonedUnits.Clear(); //Clears the list of garrisoned units
            count = 0;
        }
    }

    

}
