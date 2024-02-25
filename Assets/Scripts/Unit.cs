using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.UI;

public class Unit : EntityBase
{
    
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

    [Header("Navigation Info")]
    //navmeshagent
    public NavMeshAgent navMeshAgent;

    [Header("GameObjects")]
    //the visual model of the unit.
    public GameObject prefab;
    //the circle that appears when the unit is selected.
    public GameObject selectionIndicator;

    
    private void Awake()
    {
        //sets the nav mesh speed to the same as the speed variable.
        navMeshAgent.speed = speed;
        //makes sure the type of the unit is correct
        type = 0;
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
        navMeshAgent.SetDestination(target);
    }

    [Server] //If the speed of the unit needs to be updated, this is the function to call. This should only be called by the server.
    public void ChangeSpeed(float newSpeed)
    {
        //Updates navmesh speed and the displayed speed
        navMeshAgent.speed = newSpeed;
        speed = newSpeed;
    }

}
