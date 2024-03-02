using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectsInRange : MonoBehaviour
{
    public List<EntityBase> objects = new List<EntityBase>();
    public string lookingForType = "entity";
    public bool combatMode = false;
    public int team;
    
    private void OnTriggerEnter(Collider other)
    {
        if (combatMode && other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().team != team)
        {
            objects.Add(other.gameObject.GetComponent<EntityBase>());
            return;
        }


        if (other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().type == lookingForType)
        {
            objects.Add(other.gameObject.GetComponent<EntityBase>());
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (objects.Contains(other.gameObject.GetComponent<EntityBase>()))
        {
            objects.Remove(other.gameObject.GetComponent<EntityBase>());
        }
    }
}
