using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class UnitProductionBuilding : Building
{
    // List to store all units
    public List<Unit> units = new List<Unit>();

    // List to store units that are in the production queue
    public List<Unit> queue = new List<Unit>();

    // Reference to the UI element for unit production
    public GameObject unitProductionUI;

    // Dropdown UI element to select unit types
    public TMP_Dropdown unitDropdown;

    // UI text element to show production status
    public TextMeshProUGUI productionIndicator;

    // Button to create a new unit
    public Button createUnit;

    // String to specify the type of unit to spawn (e.g., "ground" units)
    public string spawnable = "ground";

    // Transform for the base spawn point
    public Transform spawnPointBase;

    // Transform for the finder spawn point
    public Transform spawnPointFinder;

    // Boolean to check if a spawn point has been set
    bool hasSpawnPoint = false;

    // Vector3 to store the offset for the spawn point
    public Vector3 spawnOffset;

    // Vector3 to store the actual spawn point location
    Vector3 spawnPoint;

    // Float to track the next spawn time
    float nextSpawn;

    // Boolean to check if units are being made
    bool makingUnits = false;

    // Boolean to check if units are being made at the current location
    bool makingUnitsHere = false;


    /// <summary>
    /// When the unit is first selected the base unit selected class is run to handle turning on the selection circle and and setting the selection bool to true
    /// If the UI objects for this unit are null the unit finds the UI elements and sets up the button listeners.
    /// When the UI has been switched on the function populates a dropdown with all the units that are avaliable in this building
    /// </summary>
    [Client]
    public override void Selected()
    {
        base.Selected();
        if (unitProductionUI == null)
        {
            //Finds the gameobjects for the UI
            unitProductionUI = FindInActiveObjectByName("UnitProductionMenu");
            unitDropdown = FindInActiveObjectByName("UnitProductionDropdown").GetComponent<TMP_Dropdown>();
            createUnit = FindInActiveObjectByName("MakeUnitButton").GetComponent<Button>();
            productionIndicator = FindInActiveObjectByName("IndicatorText").GetComponent<TextMeshProUGUI>();
            createUnit.onClick.AddListener(() => { AddToQueue(unitDropdown.value, team); }); //Runs the function CmdAddUnitToQueue when the button is pressed
        }
        makingUnitsHere = true;
        PopulateDropdown();
        unitProductionUI.SetActive(true);

    }

    /// <summary>
    /// Turns off the UI for this building
    /// </summary>
    public override void Deselected()
    {
        base.Deselected();
        makingUnitsHere = false;
        unitProductionUI.SetActive(false);
        productionIndicator.text = "";
    }


    /// <summary>
    /// Loops through the list of buildings that are avaliable at this building, and adds their name and price to the drop down list
    /// </summary>
    private void PopulateDropdown()
    {
        unitDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (Unit unit in units) 
        {
            options.Add($"{unit.name} - {unit.price}");
        }
        unitDropdown.AddOptions(options);
    }

    /// <summary>
    /// Adds the unit to the build queue by calling a command on the server.
    /// </summary>
    /// <param name="index">The index of the unit type in the avaliable units list</param>
    /// <param name="team">The team of player that is adding a unit to the queue</param>
    private void AddToQueue(int index, int team)
    {
        if (makingUnitsHere)
        {
            CmdAddUnitToQueue(index, team);
        }
    }
    
    /// <summary>
    /// Adds the unit to the queue to be made if the building has enough supplies. Provides feedback to the player about whether the attempt was successful
    /// </summary>
    /// <param name="index">The unit index in the list of avaliable units</param>
    /// <param name="team">The team of the player trying to spawn a unit</param>
    [Command(requiresAuthority = false)]
    private void CmdAddUnitToQueue(int index, int team)
    {
        NetworkConnection playerConnection = player.GetComponent<NetworkIdentity>().connectionToClient;
        if (units[index].price <= supplyStores && functional)
        {
            if (!hasSpawnPoint)
            {
                FindSpawnPoint();
            }
            queue.Add(units[index]);
            supplyStores -= units[index].price;
            if (!makingUnits) StartCoroutine(MakeUnits());
            TargetRPCFeedBack(playerConnection ,true, index);
        }
        else
        {
            TargetRPCFeedBack(playerConnection,false, index);
        }
        
    }

    /// <summary>
    /// Function sends a message to the client that tried to add a unit to the building queue and provides feedback to whether or not it was successful 
    /// </summary>
    /// <param name="connection">The networkconnection to send the message to</param>
    /// <param name="success">Bool, indicates whether adding the unit was successful</param>
    /// <param name="index">The index of the unit in the avaliable units list to provide the name of the unit type being added</param>
    [TargetRpc]
    private void TargetRPCFeedBack(NetworkConnection connection,bool success, int index)
    {
        try
        {
            if(productionIndicator == null)
            {
                print("Indicator text is null");
            }

            if (success)
            {
                productionIndicator.text = $"Added {units[index].name} to the queue.";
            }
            else
            {
                productionIndicator.text = $"Failed to add {units[index].name} to the queue.";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
        
    }


    /// <summary>
    /// Loops through the units in the queue and spawns them after the {timeToMake} number of seconds has past.
    /// MakingUnits boolean indicates whether the IEnumerator is already running in the for loop so that the function is not called multiple times while it is still spawning units.
    /// </summary>
    [Server]
    private IEnumerator MakeUnits()
    {
        makingUnits = true;
        
        for (int i = 0; i < queue.Count && queue[i] != null; i++ )
        {
            yield return new WaitForSeconds(queue[i].timeToMake);
            
            try
            {
                Unit newUnit = queue[i];
                newUnit.player = player;
                newUnit.team = team;
                GameObject unit = Instantiate(newUnit.prefab, spawnPoint, Quaternion.identity);
                NetworkServer.Spawn(unit);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
            
        }
        queue.Clear();
        makingUnits = false;
    }


    /// <summary>
    /// Finds a spawnpoint by rotating a gameobject in a circle path in 360 degrees, at each location a ray is casted down from gameobject to check the location below
    /// </summary>
    [Server]
    private void FindSpawnPoint()
    {
        RaycastHit hit;

        for(int i = 0; i < 360; i++)
        {
            
            if (Physics.Raycast(spawnPointFinder.position, -spawnPointFinder.up, out hit))
            {
                Debug.DrawRay(spawnPointFinder.position, -spawnPointFinder.transform.up * hit.distance, Color.green, 1000);
                if (hit.transform.CompareTag(spawnable))
                {
                    hasSpawnPoint = true;
                    spawnPoint = hit.point;
                    break;
                }
            }

            spawnPointBase.Rotate(0, i, 0);
        }
    }

    

    /// <summary>
    /// Searchs for objects that aren't active with the name {name}
    /// </summary>
    /// <param name="name">The name of the object the function will look for and return</param>
    /// <returns>Returns a gameobject with the name of name</returns>
    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

}
