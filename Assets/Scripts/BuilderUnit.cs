using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class BuilderUnit : Unit
{
    //The list of supply points
    public List<Transform> supplyPoints = new List<Transform>{null, null};
    //parent object for the builder UI
    public GameObject builderUI;
    //List of indicators for the supply points
    public List<GameObject> pointIndicators = new List<GameObject>();
    //The distance the builder has to be from the supply point to resupply
    public float resupplyDistance = 0.5f;
    //Indicates if the player finding a point
    bool findingPoint = false;
    //The index of the point that the builder is looking for
    int findingPointType = 0;
    //button that allows the player to select a place to pick up supplies
    Button pickPickup;
  
    
    /// <summary>
    /// Handles player inputs and outputs
    /// </summary>
    private void Update()
    {
        if (findingPoint)
        {
            FindingPoint(); //Looks for a point to pick up from
        }

        //Loops through the supply points and sets the location of the visual indicators
        for (int i = 0; i < supplyPoints.Count; i++)
        {
            if (supplyPoints[i] != null && selected)
            {
                pointIndicators[i].SetActive(true); 
                pointIndicators[i].transform.position = supplyPoints[i].transform.position + new Vector3(0, 1, 0);
            }
            else
            {
                pointIndicators[i].SetActive(false);
            }
        }
        
        

       
    }


    /// <summary>
    /// Runs on the server on repeat sending the builder back to the supply point if it runs out of supplies
    /// </summary>
    [ServerCallback]
    private void FixedUpdate()
    {
        if(supplyStores <= 0 && supplyPoints[0] != null && !selected)
        {
            Resupply(); //Sends the builder to the supply point and collects supplies
        }
        
        
    }


    public override void Selected()
    {
        
        base.Selected();
        if (builderUI == null) //Finds the builder UI objects if the builder has not yet found them
        {
            builderUI = FindInActiveObjectByName("BuilderUI");
            pickPickup = FindInActiveObjectByName("BuilderPickup").GetComponent<Button>();
            pickPickup.onClick.AddListener(() => { FindPointPressed(0); });
        }
        player.UIUnits[0]++; //Adds the builder to the count of UI buildings so the UI stays active if there are multiple UI units selected
        builderUI.SetActive(true); //Turns on the UI
        
    }

    public override void Deselected()
    {
        base.Deselected();
        player.UIUnits[0]--; //Subtracts 1 from the count of UI builders
        if (player.UIUnits[0] <= 0) //Turns of the UI when there are no UI builders left
        {
            builderUI.SetActive(false); 
        }

    }

    /// <summary>
    /// Sends the builder to the supply point and collects supplies when the builder is close enough
    /// </summary>
    [Server]
    private void Resupply()
    {
        navMeshAgent.SetDestination(supplyPoints[0].transform.position); //Moves the builder to the supply point
        if (Vector3.Distance(transform.position, supplyPoints[0].transform.position) < resupplyDistance) //If the builder is close enough it gets more supplies
        {
            Building building = supplyPoints[0].GetComponent<Building>(); //Gets the building object

            //Take the appropriate quantity of supplies
            if (building.supplyStores < maximumCapacity) //Makes sure that the builder does not get more supplies than the building has
            {
                supplyStores = building.supplyStores; 
                building.supplyStores = 0;
            }
            else
            {
                building.supplyStores -= maximumCapacity; //Takes supplies if there are an excess of supplies
                supplyStores = maximumCapacity;
            }
        }
    }

    private void FindPointPressed(int type)
    {
        // If 'selected' is false, exit the function early
        if (!selected) return;

        // Disable the 'pickPickup' component or object
        pickPickup.enabled = false;

        // Indicate that we are now in the process of finding a point
        findingPoint = true;

        // Set the type of point we are finding
        findingPointType = type;
    }



    private void FindingPoint()
    {
        Ray ray = player.playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the where the mouse is on the screen
        if (Input.GetMouseButtonDown(0)) //Runs code in if statement if the player left clicks
        {
            CmdCheckPickup(ray, player.team, findingPointType); //Sends command to server to set a valid supply point
            pickPickup.enabled = true; //Allows the pickup button to be pressed again
            findingPoint = false; //Indicate that the builder is no longer looking for a point
        }
    }

    /// <summary>
    /// Casts a ray on the server to find a potential pickup point. Checks if the pickup point is valid for the unit.
    /// </summary>
    /// <param name="ray">The ray the server casts from the player</param>
    /// <param name="playerTeam">The team of the unit sending the command NOT A SECURE METHOD OF CHECKING TEAM</param>
    /// <param name="findingPointType">Index of the point in the supplypoints array that the client is trying to set</param>
    [Command(requiresAuthority = false)]
    private void CmdCheckPickup(Ray ray, int playerTeam, int findingPointType) //fix later not secure and could easily be hacked
    {
        RaycastHit hit;
        //Casts the ray created
        
        if (Physics.Raycast(ray, out hit) && hit.transform.GetComponent<Building>() != null && team == playerTeam) //Checks if the building clicked on is of the correct team
        {
            supplyPoints[findingPointType] = hit.transform; //Sets the location of the supply point
            TargetRpcSupplyPoints(player.connectionToClient, hit.transform.GetComponent<NetworkIdentity>().netId, findingPointType, false); //Sends the supply location to the client
        }
        else //Sets the supply point to null if the player clicks on something invalid
        {
            supplyPoints[findingPointType] = null; 
            TargetRpcSupplyPoints(player.connectionToClient, 0, findingPointType, true);
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
    private void TargetRpcSupplyPoints(NetworkConnection connection, uint netId, int index, bool isNull)
    {

        try
        {
            if (isNull) //Checks is supply point is null
            {
                supplyPoints[index] = null; //Sets point to null
            }
            else
            {

                supplyPoints[index] = NetworkClient.spawned[netId].transform; //Sets the transform of the supplypoint
            }
        }
        catch (System.Exception ex)
        {

            print(ex);
        }
        

    }

    /// <summary>
    /// Finds gamobjects by name that are inactive
    /// </summary>
    /// <param name="name">The name of the gameobject to find</param>
    /// <returns></returns>
    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
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
