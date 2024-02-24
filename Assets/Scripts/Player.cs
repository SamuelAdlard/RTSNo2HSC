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

    private void SelectUnit()
    {

    }

    private void DeselectUnit()
    {

    }

}
