using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectsInRange : MonoBehaviour
{
    public List<EntityBase> objects = new List<EntityBase>();
    public string lookingForType = "entity";

    
    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.GetComponent<EntityBase>() != null && other.gameObject.GetComponent<EntityBase>().type == "entity")
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
