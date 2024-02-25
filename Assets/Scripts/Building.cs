using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Building : EntityBase
{
    

    [Header("Production")]
    //price of the unit
    public int price = 100;
    //The amount of work done on the building
    public int buildWorkAchieved = 0;
    //If the building has been built successfully
    public bool functional = false;

    [Header("Placement")]
    //The tags that the building has to be touching
    public string placementTag = "ground";

    [Header("GameObjects")]
    //Prefab of the building
    public GameObject prefab;
    //Visual part
    public GameObject model;
    //the circle that appears when the building is selected. (MAY NOT BE NEEDED)
    public GameObject selectionIndicator;

    public void Awake()
    {
        type = 1; //sets the type of the building
    }

    public void Update()
    {
        model.GetComponent<MeshRenderer>().material = teamColours[team]; //TODO: Find a better way of doing this
        
        
    }
}
