using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Team")]
    //team of the player, synced across all clients
    [SyncVar]public int team;
    
    

    [Header("Player's Objects")]
    //List of all the units a player has
    public List<Unit> units = new List<Unit>();
    //list of all the units the player has selected
    public List<Unit> selectedUnits = new List<Unit>();
    //number of builders selected
    public int builders;

    [Header("Networking")]
    //The network connection to the player
    public NetworkConnectionToClient networkConnectionToClient;

    [Header("Controls")]
    //Player's camera
    public Camera playerCamera;


    [Header("Building")]
    //Whether the player is building or not
    public bool isBuilding = false;
    //phantom guid building that shows up when the player is building something
    [SerializeField] private GameObject ghostBuilding;
    //list of buildings the player can build
    public List<Building> buildings = new List<Building>();
    //Tag that is needed for the building to be placed
    [SerializeField] private string requiredTag;
    //Materials for the ghost object
    public Material invalid, valid;
    //Selected building
    public Building selectedBuilding;


    [Header("UI")]
    //The UI building object
    public GameObject BuildingUI;
    //The dropdown for building 
    public TMP_Dropdown buildingDropdown;
    //The button that starts building
    public Button startBuildingButton;
    //The button that stops building
    public Button stopBuildingButton;
    //UI that displays information about troops
    public GameObject infoPanel;
    //Text that shows health
    public TextMeshProUGUI healthText;
    //Text that shows supply levels
    public TextMeshProUGUI supplyText;

    

    [ClientRpc]
    public void ClientRpcOnLoad() //Runs when the player has joined the server, called by the server
    {
        if (isLocalPlayer)
        {
            //Gets the camera gameobject when the player joins
            playerCamera = GameObject.Find("CameraPivot").transform.GetChild(0).GetComponent<Camera>();
            
        }
    }

    private void Awake()
    {
        //Finds the building menu UI
        BuildingUI = GameObject.Find("BuildMenu");
        //Finds the building dropdown UI
        buildingDropdown = GameObject.Find("BuildDropdown").GetComponent<TMP_Dropdown>();
        //Finds the startBuilding button
        startBuildingButton = GameObject.Find("BuildButtonStart").GetComponent<Button>();
        //Sets up listeners for the buttons, so that the following functions will run when the buttons are pressed
        startBuildingButton.onClick.AddListener(() => { BuildButtonPressed(); });
        //Finds the gameobjects for the UI
        infoPanel = FindInActiveObjectByName("Info");
        healthText = FindInActiveObjectByName("HealthText").GetComponent<TextMeshProUGUI>();
        supplyText = FindInActiveObjectByName("SupplyText").GetComponent<TextMeshProUGUI>();


    }

    

    private void Update()
    {
        if (!isLocalPlayer) return;
        RaycastHit hit = GetRayFromScreen();
       
        ShowEntityInformation(hit);
        if (hit.transform == null) return;
        //Checks if the player has clicked 
        if (Input.GetMouseButtonDown(0))
        {
            //Handles the player input
            ClickHandler(hit);
            
        }

        if (Input.GetMouseButtonDown(1))
        {
            //Handles the player input
            RightClickHandler(hit);

        }

        if(Input.GetKeyDown(KeyCode.C)) 
        {
            DeselectAll();
        }


        if (isBuilding)
        {
            MoveGhostBuilding(hit);
            
        }

    }


    [Client]
    private void DeselectAll()
    {
        foreach (Unit unit in selectedUnits) 
        {
            unit.Deselected();
        }

        selectedUnits.Clear();
    }

    [Client]
    private RaycastHit GetRayFromScreen() //Returns the hit of a ray cast from a point on the screen
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the when the mouse is on the screen
        RaycastHit hit;
        //Casts the ray created
        Physics.Raycast(ray, out hit);
        return hit; //Returns the result of the raycast
    }

    [Client]
    private void ClickHandler(RaycastHit hit)
    {
        if (hit.transform == null || isBuilding) return;
        //Records the unit clicked on
        Unit currentUnit;
        //Checks if the object is a unit and is on the right team
        if (hit.transform.TryGetComponent<Unit>(out currentUnit) && currentUnit.team == team && !currentUnit.selected) //Selects a unit if the player clicks on a non selected unit
        {
            SelectUnit(currentUnit); //If the unit hasn't been selected yet this adds it to the list
        }
        else if(currentUnit != null && currentUnit.team == team && currentUnit.selected) //Deselects the unit if the player clicks on a unit that is selected
        {
            DeselectUnit(currentUnit); //If the unit has already been selected this removes it from the list
        }
        else
        {
            Random.Range(0, 10);



            //Loops through all the units and tells them to move
            List<Unit> removeAt = new List<Unit>();

            foreach(Unit unit in selectedUnits)
            {
                //Sends a command to the server to move the unit
                if(unit != null)
                {
                    unit.CmdMove(SoldierPosition(hit.point, selectedUnits.IndexOf(unit), selectedUnits.Count), connectionToServer.connectionId);
                }
                else
                {
                    removeAt.Add(unit);
                }
                
            }

            
            
            foreach (Unit unit in removeAt)
            {
                    units.Remove(unit);
            }
            removeAt.Clear();
            

        }
    }

    private Vector3 SoldierPosition(Vector3 hitPosition, int index, int selectedCount)
    {
        float spread = 1.5f;
        int offsetMagnitude = Mathf.RoundToInt(Mathf.Sqrt(selectedCount));
        Vector3 offset = new Vector3(-offsetMagnitude / 2, 0, offsetMagnitude / 2);
        Vector3 rawVector = new Vector3(hitPosition.x + index % offsetMagnitude * spread, 0, hitPosition.z - (index/offsetMagnitude) * spread);
        return rawVector + offset;
    }

    [Client]
    private void RightClickHandler(RaycastHit hit)
    {
        Building building;
        if(hit.transform.TryGetComponent<Building>(out building) && selectedBuilding == null)
        {
            if(building.team == team)
            {
                SelectBuilding(building);
            }
        }
        else if(selectedBuilding != null)
        {
            DeselectBuilding(selectedBuilding);
        }
    }

    [Client]
    private void SelectBuilding(Building building)
    {
        selectedBuilding = building;
        
        building.Selected();
    }

    [Client]
    private void DeselectBuilding(Building building)
    {
        selectedBuilding = null;
        building.Deselected();
    }

    [Client] //Selected units are only needed on the client
    private void SelectUnit(Unit unit) 
    {
        unit.Selected(); //Runs the selected function in the unit class
        selectedUnits.Add(unit); //Adds the unit to the list of selected units
    }

    [Client]
    private void DeselectUnit(Unit unit)
    {
        unit.Deselected(); //Runs the deselected function in the unit class
        selectedUnits.Remove(unit);//Removes the unit from the list 
    }

    private void BuildButtonPressed() //Manages the button press depending on whether the player is building or not
    {
        if (!isLocalPlayer) return;
        if (!isBuilding)
        {
            StartBuilding();
        }
        else
        {
            StopBuilding();
        }
    }
    
    private void StartBuilding() //Run when building button is pressed
    {
        //Tells the rest of the script that the player is building
        isBuilding = true;
        //Changes the text of the button
        startBuildingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Building";
        //Stops the player from interacting with the dropdown
        buildingDropdown.interactable = false;
  
        CreateGhostBuilding();
    }

    private void StopBuilding() //Run when building button is pressed while the player is already building. Reverses all effects of the StartBuilding function
    {
        
        isBuilding = false;
        startBuildingButton.GetComponentInChildren<TextMeshProUGUI>().text = "Build";
        buildingDropdown.interactable = true;
        Destroy(ghostBuilding);
    }

    //Creates a "ghost" of the a building to show the player where they will place it, and if it is valid or not
    private void CreateGhostBuilding()
    {
        //gets the index of the dropdown
        int dropdownIndex = buildingDropdown.value;
        //finds the index in the array and uses that to spawn the ghost object
        ghostBuilding = Instantiate(buildings[dropdownIndex].prefab);
        //removes the collider and the building script from the ghost object
        Destroy(ghostBuilding.GetComponent<Collider>());
        Destroy(ghostBuilding.GetComponent<Building>());
        //Sets the requiredTag variable so the program knows where is valid and where isn't
        requiredTag = buildings[dropdownIndex].placementTag;
    }

    //Moves the ghost object and changes the colour
    private void MoveGhostBuilding(RaycastHit hit)
    {
        
        ghostBuilding.transform.position = hit.point; //Sets the location of the ghost object
        if (hit.transform.CompareTag(requiredTag)) //If the ghost object is on a valid area the colour changes to the valid material otherwise it is the invalid material
        {
            ghostBuilding.GetComponent<MeshRenderer>().material = valid;//valid material
        }
        else
        {
            ghostBuilding.GetComponent<MeshRenderer>().material = invalid;//invalid material
        }
        
        if (Input.GetMouseButtonDown(0)) CmdPlaceBuilding(buildingDropdown.value, playerCamera.ScreenPointToRay(Input.mousePosition));
    }

    private void ShowEntityInformation(RaycastHit hit)
    {
        EntityBase entity;
        if(hit.transform != null && hit.transform.TryGetComponent(out entity))
        {
            infoPanel.gameObject.SetActive(true);
            healthText.text = $"Health: {entity.health}";
            supplyText.text = $"Supplies: {entity.supplyStores}/{entity.maximumCapacity}";

        }
        else
        {
            infoPanel.gameObject.SetActive(false);
        }

        
    }

    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

    [Command]
    private void CmdPlaceBuilding(int index, Ray ray)
    {
        RaycastHit hit;
        //Casts the ray created
        Physics.Raycast(ray, out hit);
        if (hit.transform.CompareTag(buildings[index].placementTag))
        {
            Building newBuilding = buildings[index];
            newBuilding.team = team;
            GameObject build = Instantiate(newBuilding.prefab, hit.point, Quaternion.identity);
            
            NetworkServer.Spawn(build);

        }
        

    }

}
