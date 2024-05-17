using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceTransporter : Unit
{
    public List<Transform> supplyPoints = new List<Transform> { null, null };
    public List<GameObject> pointIndicators = new List<GameObject>();
    public GameObject supplyPointUI;
    public float resupplyDistance = 0.5f;
    public float dropOffRadius = 4.0f;
    bool findingPoint = false;
    int findingPointType = 0;
    Button pickPickup;
    Button pickDropoff;
    Button dropoffForUnits;

    private void Update()
    {
        
        if (findingPoint)
        {
            FindingPoint();
        }

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
        }
        else
        {
            supplyPoints[findingPointType] = null;
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
                unit.supplyStores = unit.maximumCapacity;
            }
            else
            {
                unit.supplyStores += suppliesToGive;
                supplyStores -= suppliesToGive;
            }
            

        }
        
    }
}
