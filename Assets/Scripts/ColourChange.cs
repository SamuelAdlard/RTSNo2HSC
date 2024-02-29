using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourChange : MonoBehaviour
{
    public EntityBase entity;
    public List<GameObject> gameObjectsToChange = new List<GameObject>();

    

    public void Awake()
    {
        StartCoroutine(Wait());
        OnTeamChanged();
    }

    private void OnTeamChanged()
    {
        foreach (GameObject gameObject in gameObjectsToChange)
        {
            gameObject.GetComponent<MeshRenderer>().material = entity.teamColours[entity.team]; //TODO: Find a better way of doing this
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        OnTeamChanged();
    }
}
