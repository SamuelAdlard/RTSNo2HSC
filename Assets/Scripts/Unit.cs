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
    //Circle angle
    public float angle = 0;
    //Radius of circle
    public float radius = 5;
    //Last direction the unit was moving in
    float lastAngle;

    [Header("GameObjects")]
    //the prefab of the unit.
    public GameObject prefab;
    //the circle that appears when the unit is selected.
    public GameObject selectionIndicator;
    //list of visible objects on the gameobject
    public List<GameObject> visibleGameObjects = new List<GameObject>();
    //The visible model of the unit.
    public GameObject model;

    [ServerCallback]
    public void Awake()
    {
        //sets the nav mesh speed to the same as the speed variable.
        navMeshAgent.speed = speed;
        Vector3 randomVector = new Vector3(transform.position.x + Random.Range(-1, 1), transform.position.x + Random.Range(-1, 1), transform.position.x + Random.Range(-1, 1));
        navMeshAgent.SetDestination(randomVector);
        movementTarget = transform.position;
        //print(gameObject.name);
        if (keepMoving) InvokeRepeating("KeepMoving", 0, 0.1f);
        player.units.Add(this);
    }

    
    public void KeepMoving()
    {

        float currentAngle = transform.rotation.eulerAngles.y;

        float currentAngularVelocity = lastAngle - currentAngle; //degrees per second

        if (Vector3.Distance(transform.position, movementTarget) < 7.5f)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 circlePosition = new Vector3(x, 0, y);
            
            
            
            if (navMeshAgent.isOnNavMesh) navMeshAgent.SetDestination(movementTarget + circlePosition);
        }


        lastAngle = currentAngle;
        Vector3 rotation = new Vector3( 0, transform.rotation.eulerAngles.y, currentAngularVelocity);
        model.transform.rotation = Quaternion.Euler(rotation);
        angle += 10;

        if(angle >= 360)
        {
            angle = 0;
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

    [ServerCallback]
    private void OnDestroy()
    {
        player.units.Remove(this);
    }

}
