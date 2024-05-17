using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class BuilderUnit : Unit
{
    public List<Transform> supplyPoints = new List<Transform>{null, null};
    
    public GameObject builderUI;
    public List<GameObject> pointIndicators = new List<GameObject>();
    public float resupplyDistance = 0.5f;
    bool findingPoint = false;
    int findingPointType = 0;
    Button pickPickup;
    Button pickDropoff;
    
    
    private void Update()
    {
        if (findingPoint)
        {
            FindingPoint();
        }

        //TODO: Fix this later because it is an inefficient way of doing it
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

    [ServerCallback]
    private void FixedUpdate()
    {
        if(supplyStores <= 0 && supplyPoints[0] != null && !selected)
        {
            Resupply();
        }

        
    }


    public override void Selected()
    {
        
        base.Selected();
        if (builderUI == null)
        {
            builderUI = FindInActiveObjectByName("BuilderUI");
            pickPickup = FindInActiveObjectByName("BuilderPickup").GetComponent<Button>();
            //pickDropoff = FindInActiveObjectByName("Dropoff").GetComponent<Button>();
            pickPickup.onClick.AddListener(() => { FindPointPressed(0); });
            //pickDropoff.onClick.AddListener(() => { FindPointPressed(1); });
        }
        player.UIbuildings[0]++;
        builderUI.SetActive(true);
        
    }

    public override void Deselected()
    {
        base.Deselected();
        player.UIbuildings[0]--;
        if (player.UIbuildings[0] <= 0)
        {
            builderUI.SetActive(false);
        }

    }

    [Server]
    private void Resupply()
    {
        navMeshAgent.SetDestination(supplyPoints[0].transform.position);
        if(Vector3.Distance(transform.position, supplyPoints[0].transform.position) < resupplyDistance)
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

    private void FindPointPressed(int type)
    {
        if (!selected) return;
        pickPickup.enabled = false;
        //pickDropoff.enabled = false;
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
            //pickDropoff.enabled = true;
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

}
