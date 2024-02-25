using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectsInRange : MonoBehaviour
{
    public List<EntityBase> objects = new List<EntityBase>();


    
    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if(other.gameObject.GetComponent<EntityBase>() != null)
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
