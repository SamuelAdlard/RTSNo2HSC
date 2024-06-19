using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourChange : MonoBehaviour
{
    public EntityBase entity;
    //List of gameobjects whose colour will be changeds
    public List<GameObject> gameObjectsToChange = new List<GameObject>();

    
    /// <summary>
    /// Sets the colour when the object is created
    /// </summary>
    public void Awake()
    {
        StartCoroutine(Wait());
        OnTeamChanged();
    }

    /// <summary>
    /// Changes the colour of all gameobjects in the gameobjects to change list to the colour of the team
    /// </summary>
    private void OnTeamChanged()
    {
        foreach (GameObject gameObject in gameObjectsToChange)
        {
            gameObject.GetComponent<MeshRenderer>().material = entity.teamColours[entity.team]; //TODO: Find a better way of doing this
        }
    }

    /// <summary>
    /// Waits 0.5 seconds before setting the colour to allow time for the player object to be selected. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Wait() //TODO: FIX ALL OF THESE STUPID IENUMERATORS IN SITUATIONS LIKE THIS. THEY ARE MESSY
    {
        yield return new WaitForSeconds(0.5f);
        OnTeamChanged();
    }
}
