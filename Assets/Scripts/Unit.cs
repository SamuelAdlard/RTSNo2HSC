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
    
    //What the unit can fit inside
    public float size = 1;
   

    [Header("Production")]
    //price of the unit
    public int price = 100;
    //time it takes to produce the unit in a production building
    public float timeToMake = 10f;

    [Header("Navigation Info")]
    //navmeshagent
    public NavMeshAgent navMeshAgent;
    public Vector3 offset;
    //The place the unit is moving to
    Vector3 movementTarget;
    //TODO: Movement type
    public bool keepMoving = false;

    [Header("GameObjects")]
    //the visual model of the unit.
    public GameObject prefab;
    //the circle that appears when the unit is selected.
    public GameObject selectionIndicator;
    //list of visible objects on the gameobject
    public List<GameObject> visibleGameObjects = new List<GameObject>();

    
    private void Awake()
    {
        //sets the nav mesh speed to the same as the speed variable.
        navMeshAgent.speed = speed;
        
    }

    public void KeepMoving()
    {
        if (keepMoving && navMeshAgent.velocity.magnitude < 1)
        {
            
            Vector3 randomVector = new Vector3(Random.Range(-4, 4), 0, Random.Range(4, 4));
            navMeshAgent.SetDestination(movementTarget + randomVector);
            
        }
    }


    //Runs when the player selects the unit
    public virtual void Selected()
    {
        selected = true;
        //Shows visually the unit is selected
        selectionIndicator.SetActive(true);
    }

    //Runs when the player deselects the unit
    public virtual void Deselected()
    {
        selected = false;
        //Shows visually the units isn't selected 
        if(selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
        
    }

    //Moves the Unit
    [Command(requiresAuthority = false)]
    public void CmdMove(Vector3 target, uint playerID)
    {
        
        if (NetworkServer.spawned[playerID].GetComponent<Player>().team != team) return;   
        //TODO: Add something for movement type later
        //TODO: Validate player information here
        if(navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(target + offset);

        }
        movementTarget = target + offset;
        
    }

    [ClientRpc]
    public void ClientRpcVisible(bool visible)
    {
        foreach(GameObject gameObject in visibleGameObjects)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = visible;
        }
    }
    

    [Server] //If the speed of the unit needs to be updated, this is the function to call. This should only be called by the server.
    public void ChangeSpeed(float newSpeed)
    {
        //Updates navmesh speed and the displayed speed
        navMeshAgent.speed = newSpeed;
        speed = newSpeed;
    }

}
