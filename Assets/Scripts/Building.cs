using Mirror;
using UnityEngine;
public class Building : EntityBase
{
    

    [Header("Production")]
    //price of the unit
    public int price = 100;
    //The amount of work done on the building
    [SyncVar]public int buildWorkAchieved = 0;
    //If the building has been built successfully
    [SyncVar]public bool functional = false;
    //Build speed
    public float buildDelay = 1;
    float nextBuild = 0;

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
    //Range that detects if a builder is in range
    public ObjectsInRange builderRange;


    [ServerCallback]
    private void Update()
    {
        if (!functional && Time.time > nextBuild)
        {
            print("Trying to build");
            nextBuild = Time.time + buildDelay;
            foreach (EntityBase builder in builderRange.objects)
            {
                print("Building");
                if(builder.team == team) Build(builder.GetComponent<BuilderUnit>());
            }
        }
        else if (functional && builderRange != null)
        {
            Destroy(builderRange.gameObject);
            ClientRpcSetModelTrue();
        }

        if (buildWorkAchieved >= price) functional = true;
    }


    [Server]
    public void Build(BuilderUnit builder)
    {
        if(builder.supplyStores > 0)
        {
            builder.supplyStores--;
            buildWorkAchieved++;
        }
    }

    [ClientRpc]
    private void ClientRpcSetModelTrue()
    {
        model.SetActive(true);
    }
}
