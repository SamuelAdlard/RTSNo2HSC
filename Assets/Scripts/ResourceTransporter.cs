using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceTransporter : Unit
{
    public Building refillPoint;
    public GameObject builderUI;
    public float resupplyDistance = 0.5f;
    bool findingPickUp = false;
    Button pickPickup;
    Vector3 returnPosition;

    private void Update()
    {
        if (findingPickUp)
        {
            FindingPickUp();
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (supplyStores <= 0 && refillPoint != null && !selected)
        {
            Resupply();
        }

        if (selected)
        {
            returnPosition = transform.position;
        }

    }


    public override void Selected()
    {

        base.Selected();
        if (builderUI == null)
        {
            builderUI = FindInActiveObjectByName("BuilderUI");
            pickPickup = builderUI.GetComponentInChildren<Button>();
            pickPickup.onClick.AddListener(() => { FindPickUpBuildingPressed(pickPickup); });
        }
        player.builders++;
        builderUI.SetActive(true);

    }

    public override void Deselected()
    {
        base.Deselected();
        player.builders--;
        if (player.builders <= 0)
        {
            builderUI.SetActive(false);
        }

    }

    [Server]
    private void Resupply()
    {
        navMeshAgent.SetDestination(refillPoint.transform.position);
        if (Vector3.Distance(transform.position, refillPoint.transform.position) < resupplyDistance)
        {
            refillPoint.supplyStores -= maximumCapacity;
            supplyStores = maximumCapacity;
            //navMeshAgent.SetDestination(returnPosition);
        }
    }

    private void FindPickUpBuildingPressed(Button pickPickup)
    {
        pickPickup.enabled = false;
        findingPickUp = true;
        returnPosition = transform.position;
    }

    private void FindingPickUp()
    {
        Ray ray = player.playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the when the mouse is on the screen
        if (Input.GetMouseButtonDown(0))
        {
            CmdCheckPickup(ray, player.team);
            findingPickUp = false;
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdCheckPickup(Ray ray, int playerTeam) //fix later not secure and could easily be hacked
    {
        RaycastHit hit;
        //Casts the ray created
        Physics.Raycast(ray, out hit);
        if (hit.transform.GetComponent<Building>() != null && team == playerTeam)
        {
            refillPoint = hit.transform.GetComponent<Building>();
        }
        else
        {
            refillPoint = null;
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
