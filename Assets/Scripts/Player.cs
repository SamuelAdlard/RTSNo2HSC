using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    //team of the player, synced across all clients
    [SyncVar]public int team;
    //List of all the units a player has
    public List<Unit> units = new List<Unit>();
    //TODO: Make building class
    //list of all the units the player has selected
    public List<Unit> selectedUnits = new List<Unit>();
    //The network connection to the player
    public NetworkConnectionToClient networkConnectionToClient;
    //Player's camera
    public Camera playerCamera;

    [ClientRpc]
    public void ClientRpcOnLoad()
    {
        if (isLocalPlayer)
        {
            playerCamera = GameObject.Find("CameraPivot").transform.GetChild(0).GetComponent<Camera>();
        }
    }

    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0) && isLocalPlayer)
        {
            ClickHandler(ClickOnObject());
            
        }
    }

    [Client]
    private RaycastHit ClickOnObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        return hit;
    }

    [Client]
    private void ClickHandler(RaycastHit hit)
    {
        //Records the unit clicked on
        Unit currentUnit;
        if (hit.transform.TryGetComponent<Unit>(out currentUnit) && currentUnit.team == team && !currentUnit.selected) //Selects a unit if the player clicks on a non selected unit
        {
            SelectUnit(currentUnit);
        }
        else if(currentUnit != null && currentUnit.team == team && currentUnit.selected) //Deselects the unit if the player clicks on a unit that is selected
        {
            DeselectUnit(currentUnit);
        }
        else
        {
            //TODO: Add logic buldings later
            foreach(Unit unit in selectedUnits)
            {
                print(unit.name);
                print(hit.point);
                
                unit.CmdMove(hit.point, connectionToServer.connectionId);
            }
        }
    }

    [Client]
    private void SelectUnit(Unit unit)
    {
        unit.Selected();
        selectedUnits.Add(unit);
    }

    [Client]
    private void DeselectUnit(Unit unit)
    {
        unit.Deselected();
        selectedUnits.Remove(unit);
    }

}
