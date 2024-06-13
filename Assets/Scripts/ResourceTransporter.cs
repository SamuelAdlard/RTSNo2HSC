using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceTransporter : Unit
{
    //The points the unit will take to and from
    public List<Transform> supplyPoints = new List<Transform> { null, null };
    //Coloured spheres that show where the different points are
    public List<GameObject> pointIndicators = new List<GameObject>();
    //The UI for selecting supply points
    public GameObject supplyPointUI;
    //The distance the unit has to be to pick up or drop off resources
    public float resupplyDistance = 0.5f;
    //The radius in which units will be given supplies
    public float dropOffRadius = 4.0f;
    //whether the player is looking for a pickup location or not
    bool findingPoint = false;
    //The type of supply point the player is finding
    int findingPointType = 0;
    //Pickup button
    Button pickPickup;
    //Dropoff button
    Button pickDropoff;
    //The button to give supplies to units
    Button dropoffForUnits;

    /// <summary>
    /// Handles player inputs and outputs
    /// </summary>
    private void Update()
    {
        
        if (findingPoint)
        {
            FindingPoint(); //runs the finding point function if the player has started looking for a pickup point
        }

        for (int i = 0; i < supplyPoints.Count; i++) //Loops through the supplyPoints list an sets up the supply point indicators
        {
            if (supplyPoints[i] != null && selected) //Checks if the supply point exists
            {
                pointIndicators[i].SetActive(true); //Turns on the indicator for that type of point
                pointIndicators[i].transform.position = supplyPoints[i].transform.position + new Vector3(0, 1, 0); //Sets the location of the indicator to the supply point
            }
            else
            {
                pointIndicators[i].SetActive(false); //Turns the point indicator off if the 
            }
        }
    }

    /// <summary>
    /// Tells the client the location of supply points so that the client can set the visual indicators.
    /// Function sets the transform of supplyPoints[index] if the supply point is null on the server, the function will set it to null on the client.
    /// </summary>
    /// <param name="connection">The client to send to</param>
    /// <param name="netId">The netId of the supply point</param>
    /// <param name="index">The index of the supply point in the supply point list</param>
    /// <param name="isNull">Whether the supply point on the server is null or not</param>
    [TargetRpc]
    private void TargetRpcSupplyPoints(NetworkConnection connection,uint netId, int index, bool isNull)
    {
        if (isNull)
        {
            supplyPoints[index] = null; //The supply point doesn't exist so this sets the supply point to null on the client
        }
        else
        {
            supplyPoints[index] = NetworkClient.spawned[netId].transform; //Sets the transform of the supplypoint
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, dropOffRadius); //Draws a gizmo sphere for debugging
    }

    /// <summary>
    /// Runs on the server on repeat sending the builder back to the supply point if it runs out of supplies
    /// </summary>
    [ServerCallback]
    private void FixedUpdate()
    {
        if (supplyStores <= 0 && supplyPoints[0] != null && !selected) //Moves the units to the pickup point if the unit is out of supplies
        {
            Resupply();
        }
        else if (supplyPoints[1] != null && !selected && supplyStores > 0) //Moves the units to the dropoff point of the unit has supplies
        {
            Dropoff();
        }
    }

    /// <summary>
    /// Handles selecting the player
    /// Handles showing UI to the player
    /// </summary>
    public override void Selected()
    {

        base.Selected();
        if (supplyPointUI == null) //Finds UI gameobjects if they have not already been found
        {
            supplyPointUI = FindInActiveObjectByName("ResourceTransportUI");
            pickPickup = FindInActiveObjectByName("Pickup").GetComponent<Button>();
            pickDropoff = FindInActiveObjectByName("Dropoff").GetComponent<Button>();
            dropoffForUnits = FindInActiveObjectByName("UnitDropOff").GetComponent<Button>();
            //Sets up listeners for buttons 
            pickPickup.onClick.AddListener(() => { FindPointPressed(0); });
            pickDropoff.onClick.AddListener(() => { FindPointPressed(1); });
            dropoffForUnits.onClick.AddListener(() => { DropOffForUnits(); });
        }
        player.UIUnits[1]++; //Adds 1 to the UI units counter
        supplyPointUI.SetActive(true);
    }

    /// <summary>
    /// Handles deselecting the unit and turning off UI
    /// </summary>
    public override void Deselected()
    {
        base.Deselected(); //Runs the base inherited deselected function 
        player.UIUnits[1]--; //Removes 1 from the UI units counter
        if (player.UIUnits[1] <= 0) //Turns off the UI if there are no UI units left
        {
            supplyPointUI.SetActive(false);
        }

    }


    /// <summary>
    /// Moves the unit to the supply point and picks up supplies if the unit is close enough
    /// </summary>
    [Server]
    private void Resupply()
    {
        
        navMeshAgent.SetDestination(supplyPoints[0].transform.position); //Moves the unit to the
        if (Vector3.Distance(transform.position, supplyPoints[0].transform.position) < resupplyDistance) //Gives the unit supplies if the unit is close enough
        {
            Building building = supplyPoints[0].GetComponent<Building>(); //Gets the building component of the supply point
            if (building.supplyStores < maximumCapacity) //Makes sure the unit does not take more supplies than the building has
            {
                supplyStores = building.supplyStores;
                building.supplyStores = 0;
            }
            else
            {   //Gives the unit supplies if the building has exccess supplies
                building.supplyStores -= maximumCapacity;
                supplyStores = maximumCapacity;
            }
           
        }
    }

    /// <summary>
    /// Handles dropping off supplies at a building.
    /// Moves the unit to the dropoff point. When the unit is close enough it deposites all supplies at the building
    /// </summary>
    private void Dropoff()
    {
        
        navMeshAgent.SetDestination(supplyPoints[1].transform.position); //moves the unit to the supply point
        if (Vector3.Distance(transform.position, supplyPoints[1].transform.position) < resupplyDistance) //Checks the distance of the unit to the building
        {
            Building building = supplyPoints[1].GetComponent<Building>(); //Gets the building component of the supply point
            if (supplyStores + building.supplyStores > building.maximumCapacity) //Gives the building supplies from the unit without going over the building maximum limit
            {
                building.supplyStores = building.maximumCapacity;
                supplyStores = 0;
            }
            else
            {
                building.supplyStores += supplyStores;
                supplyStores = 0;
            }
        }
    }

    /// <summary>
    /// Runs when the player presses a button that selects a supply point
    /// </summary>
    /// <param name="type">Determines whether the player wants to select the pick up or drop off point</param>
    private void FindPointPressed(int type)
    {
        if (!selected) return; //Doesn't run if the unit isn't selected
        pickPickup.enabled = false; //Stops the player from pressing the buttons again
        pickDropoff.enabled = false;
        findingPoint = true; //Determines whether the player is looking for a pickup point
        findingPointType = type; //Sets the type of point the player is looking for
    }


    /// <summary>
    /// Runs on update when the player is looking for a point.
    /// When the player clicks on a building sends a command to the server and sets the supply point of the unit on the server
    /// </summary>
    private void FindingPoint()
    {
        Ray ray = player.playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the when the mouse is on the screen
        if (Input.GetMouseButtonDown(0)) //Detects left-click
        {
            CmdCheckPickup(ray, player.team, findingPointType); //Sends a command to the server to set the new supply point
            pickPickup.enabled = true; //Allows the buttons to be pressed
            pickDropoff.enabled = true; 
            findingPoint = false; //Indicates that the player is no longer looking for a building 
        }
    }


    /// <summary>
    /// Checks that the building the player clicked on is a valid supply point, and then sets that point to the supply point 
    /// </summary>
    /// <param name="ray">Casts a ray from the player perspective to check the building it is clicking on is valid USED NETID NEXT TIME</param>
    /// <param name="playerTeam">The team of the player that wants to find a new supply point.  NOT SECURE FIND ANOTHER METHOD LATER</param>
    /// <param name="findingPointType">The index in the supply points array that will be updates</param>
    [Command(requiresAuthority = false)]
    private void CmdCheckPickup(Ray ray, int playerTeam, int findingPointType) //fix later not secure and could easily be hacked
    {
        RaycastHit hit;
        //Casts the ray from the player created
        if (Physics.Raycast(ray, out hit) && hit.transform.GetComponent<Building>() != null && team == playerTeam) //Runs if the ray hits a valid building
        {
            supplyPoints[findingPointType] = hit.transform; //Sets the transform of the supply point
            TargetRpcSupplyPoints( player.netIdentity.connectionToClient,hit.transform.GetComponent<NetworkIdentity>().netId, findingPointType, false); //Sets the transform of the supply point on the client that sent the command
        }
        else
        {
            supplyPoints[findingPointType] = null; //sets the transform of the supplypoint to null if the player clicks on an unvalid object
            TargetRpcSupplyPoints( player.netIdentity.connectionToClient, 0, findingPointType, true); //Sends null value to client
        }
    }



    // Function to find an inactive GameObject by its name
    GameObject FindInActiveObjectByName(string name)
    {
        // Gets all GameObjects in the scene, including inactive ones
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();

        // Loops through all the GameObjects
        for (int i = 0; i < objs.Length; i++)
        {
            // Checks if the GameObject is not hidden
            if (objs[i].hideFlags == HideFlags.None)
            {
                // Checks if the GameObject's name matches the given name
                if (objs[i].name == name)
                {
                    // Returns the matching GameObject
                    return objs[i].gameObject;
                }
            }
        }
        // Returns null if no matching GameObject is found
        return null;
    }

    // Function to initiate the drop-off process for units
    public void DropOffForUnits()
    {
        // Checks if the object is selected
        if (selected)
        {
            // Calls the function to find nearby units
            FindNearbyUnits();
        }
    }

    // Command attribute allows this function to be called on the server
    [Command(requiresAuthority = false)]
    private void FindNearbyUnits()
    {
        // Finds all colliders within a certain radius of the current object
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, dropOffRadius);

        // If no colliders are found, exit the function
        if (hitColliders.Length <= 0) return;

        // List to store nearby combat units
        List<CombatUnit> units = new List<CombatUnit>();

        // Loops through all the found colliders
        foreach (Collider collider in hitColliders)
        {
            // Checks if the collider has a CombatUnit component
            if (collider.TryGetComponent(out CombatUnit unit))
            {
                // Checks if the unit is on the same team
                if (unit.team == team)
                {
                    // Adds the unit to the list
                    units.Add(unit);
                }
            }
        }

        // Calls the function to supply units with resources
        SupplyUnits(units);
    }

    // Function to supply units with resources
    private void SupplyUnits(List<CombatUnit> units)
    {
        // Prints the number of units to the console
        print(units.Count);

        // Loops through each unit in the list
        foreach (CombatUnit unit in units)
        {
            // Calculates the amount of supplies to give to each unit
            int suppliesToGive = supplyStores / units.Count;

            // Checks if the supplies to give exceed the unit's maximum capacity
            if (suppliesToGive + unit.supplyStores > unit.maximumCapacity)
            {
                // Adjusts the supply stores and gives the unit the maximum amount it can take
                supplyStores -= (unit.maximumCapacity - unit.supplyStores);
                unit.GetSupplies(unit.maximumCapacity - unit.supplyStores);
            }
            else
            {
                // Gives the unit the calculated amount of supplies
                unit.GetSupplies(suppliesToGive);
                // Deducts the given supplies from the supply stores
                supplyStores -= suppliesToGive;
            }
        }
    }
}
