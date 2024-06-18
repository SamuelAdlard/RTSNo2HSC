using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectsInRange : MonoBehaviour
{
    public List<EntityBase> objects = new List<EntityBase>();
    public string lookingForType = "entity";
    public bool combatMode = false;
    public bool selectionMode = false;
    public bool friendlyMode = false;
    public int team;

    /// <summary>
    /// Adds certain gameobjects to the objects list based on the conditions selected combat mode adds enemies, 
    /// selection mode adds a particular type of unit, friendly mode adds units on the same team.
    /// </summary>
    /// <param name="other">The collider of the gameobject that has just entered the collider</param>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        
        if (combatMode && other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().team != team)
        {
            objects.Add(other.gameObject.GetComponent<EntityBase>());
            
        }


        if (selectionMode && other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().type == lookingForType)
        {
            objects.Add(other.gameObject.GetComponent<EntityBase>());
        }

        if(friendlyMode && other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().team == team)
        {
            objects.Add(other.gameObject.GetComponent<EntityBase>());
        }
    }

    /// <summary>
    /// Removes objects from the list when they exit the collider
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (objects.Contains(other.gameObject.GetComponent<EntityBase>()))
        {
            objects.Remove(other.gameObject.GetComponent<EntityBase>());
        }
    }
}
