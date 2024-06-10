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
    /// Tells the client where the supply points are. Targets the client that set the supply point
    /// </summary>
    [TargetRpc]
    private void TargetRpcSupplyPoints(NetworkConnection connection,uint netId, int index, bool isNull)
    {
        if (isNull)
        {
            supplyPoints[index] = null; //The supply point doesn't exist so this sets the supply point to null on the client
        }
        else
        {
            supplyPoints[index] = NetworkClient.spawned[netId].transform; //
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, dropOffRadius);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (supplyStores <= 0 && supplyPoints[0] != null && !selected)
        {
            Resupply();
        }
        else if (supplyPoints[1] != null && !selected && supplyStores > 0)
        {
            Dropoff();
        }
    }


    public override void Selected()
    {

        base.Selected();
        if (supplyPointUI == null)
        {
            supplyPointUI = FindInActiveObjectByName("ResourceTransportUI");
            pickPickup = FindInActiveObjectByName("Pickup").GetComponent<Button>();
            pickDropoff = FindInActiveObjectByName("Dropoff").GetComponent<Button>();
            dropoffForUnits = FindInActiveObjectByName("UnitDropOff").GetComponent<Button>();
            pickPickup.onClick.AddListener(() => { FindPointPressed(0); });
            pickDropoff.onClick.AddListener(() => { FindPointPressed(1); });
            dropoffForUnits.onClick.AddListener(() => { DropOffForUnits(); });
        }
        player.UIbuildings[1]++;
        supplyPointUI.SetActive(true);
    }

    public override void Deselected()
    {
        base.Deselected();
        player.UIbuildings[1]--;
        if (player.UIbuildings[1] <= 0)
        {
            supplyPointUI.SetActive(false);
        }

    }

    [Server]
    private void Resupply()
    {
        
        navMeshAgent.SetDestination(supplyPoints[0].transform.position);
        if (Vector3.Distance(transform.position, supplyPoints[0].transform.position) < resupplyDistance)
        {
            Building building = supplyPoints[0].GetComponent<Building>();
            if (building.supplyStores < maximumCapacity)
            {
                supplyStores = building.supplyStores;
                building.supplyStores = 0;
            }
            else
            {
                building.supplyStores -= maximumCapacity;
                supplyStores = maximumCapacity;
            }
           
        }
    }

    private void Dropoff()
    {
        
        navMeshAgent.SetDestination(supplyPoints[1].transform.position);
        if (Vector3.Distance(transform.position, supplyPoints[1].transform.position) < resupplyDistance)
        {
            Building building = supplyPoints[1].GetComponent<Building>();
            if (supplyStores + building.supplyStores > building.maximumCapacity)
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

    private void FindPointPressed(int type)
    {
        if (!selected) return;
        pickPickup.enabled = false;
        pickDropoff.enabled = false;
        findingPoint = true;
        findingPointType = type;
    }



    private void FindingPoint()
    {
        Ray ray = player.playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the when the mouse is on the screen
        if (Input.GetMouseButtonDown(0))
        {
            CmdCheckPickup(ray, player.team, findingPointType);
            pickPickup.enabled = true;
            pickDropoff.enabled = true;
            findingPoint = false;
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdCheckPickup(Ray ray, int playerTeam, int findingPointType) //fix later not secure and could easily be hacked
    {
        RaycastHit hit;
        //Casts the ray created
        
        if (Physics.Raycast(ray, out hit) && hit.transform.GetComponent<Building>() != null && team == playerTeam)
        {
            supplyPoints[findingPointType] = hit.transform;
            TargetRpcSupplyPoints( player.netIdentity.connectionToClient,hit.transform.GetComponent<NetworkIdentity>().netId, findingPointType, false);
        }
        else
        {
            supplyPoints[findingPointType] = null;
            TargetRpcSupplyPoints( player.netIdentity.connectionToClient, 0, findingPointType, true);
        }
    }

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

    public void DropOffForUnits()
    {
        if(selected)
        {
            FindNearbyUnits();
            
        }
    }

    [Command(requiresAuthority = false)]
    private void FindNearbyUnits()
    {
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, dropOffRadius);
        if (hitColliders.Length <= 0) return;
        
        List<CombatUnit> units = new List<CombatUnit>();
        foreach (Collider collider in hitColliders)
        {
            //print(collider.name);
            if (collider.TryGetComponent(out CombatUnit unit))
            {
                if (unit.team == team)
                {
                   
                    units.Add(unit);
                }
            }
        }

        SupplyUnits(units);

    }

    private void SupplyUnits(List<CombatUnit> units)
    {
        print(units.Count);
        foreach (CombatUnit unit in units)
        {
            int suppliesToGive = supplyStores / units.Count;
            if(suppliesToGive + unit.supplyStores > unit.maximumCapacity)
            {
                
                
                supplyStores -= (unit.maximumCapacity - unit.supplyStores);
                unit.GetSupplies(unit.maximumCapacity - unit.supplyStores);
            }
            else
            {
                unit.GetSupplies(suppliesToGive);
                supplyStores -= suppliesToGive;
            }
            

        }
        
    }
}
