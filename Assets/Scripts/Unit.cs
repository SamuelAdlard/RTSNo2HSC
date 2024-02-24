using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
public class Unit : NetworkBehaviour
{
    [Header("Team")]
    //Leads to owner player object
    public Player player;
    //the id of the player who owns the unit
    public int team; //TODO: Make a system that properly handles teams

    [Header("Gameplay")]
    //health
    public int health = 10;
    //supplies, if these run out there is a penalty
    public int supplyLevels = 10;
    //navMeshAgent speed
    [SerializeField] private float speed = 5;
    //True if in the player script's selected list
    public bool selected = false;
    //TODO: Movement type

    [Header("Production")]
    //price of the unit
    public int price = 100;
    //time it takes to produce the unit in a production building
    public float timeToMake = 10f;

    [Header("Unity Object Info")]
    //navmeshagent
    public NavMeshAgent navMeshAgent;
    

    [Header("Visual")]
    //the visual model of the unit.
    public GameObject model;
    //the circle that appears when the unit is selected.
    public GameObject selectionIndicator;

    private void Awake()
    {

        //sets the nav mesh speed to the same as the speed variable.
        navMeshAgent.speed = speed;
    }

    //Runs when the player selects the unit
    public void Selected()
    {
        selected = true;
        //Shows visually the unit is selected
        selectionIndicator.SetActive(true);
    }

    //Runs when the player deselects the unit
    public void Deselected()
    {
        selected = false;
        //Shows visually the units isn't selected 
        selectionIndicator.SetActive(false);
    }

    [Command(requiresAuthority = false)]  //Sends a command to the sever to move the unit.
    public void CmdMove(Vector3 target, int connectionId)
    {
        //TODO: Add something for movement type later
        //TODO: Validate player information here
        print("sent command");
        navMeshAgent.SetDestination(target);
    }

    [Server] //If the speed of the unit needs to be updated, this is the function to call. This should only be called by the server.
    public void ChangeSpeed(float newSpeed)
    {
        navMeshAgent.speed = newSpeed;
        speed = newSpeed;
    }

}
