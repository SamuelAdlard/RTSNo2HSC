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
    public List<int> UIUnits = new List<int>();
    

    [Header("Networking")]
    //The network connection to the player
    public NetworkConnectionToClient networkConnectionToClient;
    //NetworkManager
    public RTSNetworkManager networkManager;

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
    //If the player is in the lobby
    [SyncVar]public bool inLobby = true  ;
    //If the player is ready
    [SyncVar]public bool ready = false;
    //The UI that is shown if the player is in the lobby
    public GameObject lobbyUI;
    //ready button for lobby menu
    public Button readyButton;
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
    

    

    
    /// <summary>
    /// Sets up listeners for the UI and finds the UI objects
    /// </summary>
    private void Awake()
    {
        //Finds the network manager object
        networkManager = GameObject.Find("NetworkManager").GetComponent<RTSNetworkManager>();
        //Finds the lobby menu
        lobbyUI = FindInActiveObjectByName("LobbyUI");
        readyButton = lobbyUI.GetComponentInChildren<Button>();
        //Finds the building menu UI
        BuildingUI = FindInActiveObjectByName("BuildMenu");
        //Finds the building dropdown UI
        buildingDropdown = FindInActiveObjectByName("BuildDropdown").GetComponent<TMP_Dropdown>();
        //Finds the startBuilding button
        startBuildingButton = FindInActiveObjectByName("BuildButtonStart").GetComponent<Button>();
        //Sets up listeners for the buttons, so that the following functions will run when the buttons are pressed
        startBuildingButton.onClick.AddListener(() => { BuildButtonPressed(); });
        readyButton.onClick.AddListener(() => { GetReady(); });
        //Finds the gameobjects for the UI
        infoPanel = FindInActiveObjectByName("Info");
        healthText = FindInActiveObjectByName("HealthText").GetComponent<TextMeshProUGUI>();
        supplyText = FindInActiveObjectByName("SupplyText").GetComponent<TextMeshProUGUI>();


    }

    /// <summary>
    /// Called by the server when the game starts. Allows the player to move the camera and interact with units and game UI
    /// </summary>
    [ClientRpc]
    public void ClientRpcStartGame()
    {
        //Removes the ready button UI
        readyButton.gameObject.SetActive(false);
        //Turns on the building UI
        BuildingUI.SetActive(true);        
        playerCamera = GameObject.Find("CameraPivot").transform.GetChild(0).GetComponent<Camera>();
        playerCamera.GetComponentInParent<CameraMovement>().enabled = true; //Allows the player camera to move
    }

    /// <summary>
    /// Function called by button to allow the player to get ready to play the game, lets the server know that the player is ready to player the game
    /// </summary>
    public void GetReady()
    {
        if (!isLocalPlayer) return; //Only runs function on the local player to prevent the function from running multiple times
        //readyButton.GetComponentInChildren<Text>().text = "Ready"; (feature that may be added later)
        readyButton.enabled = false; //Prevents the player from pressing the ready button againt
        //Tells the server that the player is ready
        CmdGetReady();
    }


    /// <summary>
    /// Command called by client that lets the server know that the player is ready to start the game
    /// </summary>
    [Command]
    private void CmdGetReady()
    {
        ready = true; //Sets the ready variable to true for this player
        networkManager.PlayerReady(); //Tells the network manager that a player has pressed ready
        
    }

    [ClientRpc]
    public void ClientRpcOnLoad() //Runs when the player has joined the server, called by the server
    {
        //Gets the camera gameobject when the player joins
        playerCamera = GameObject.Find("CameraPivot").transform.GetChild(0).GetComponent<Camera>();
        
    }

    /// <summary>
    /// Only runs for the local player. Handles inputs from the player, allows player to select/deselect units and place buildings
    /// </summary>
    private void Update()
    {
        if (!isLocalPlayer || inLobby) return; //Doesn't run the script if the player is in the lobby, or if this object isn't the localplayer 
        RaycastHit hit = GetRayFromScreen(); //Casts a ray from the player's mouse position and returns the hit
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); //Checks if the player's mouse is over any UI elements
        ShowEntityInformation(hit); //Updates UI information based on which unit the mouse is over to show information from the unit

        if (Input.GetKeyDown(KeyCode.C)) //Deselects all the units currently selected
        {
            DeselectAll();
        }

        if (hit.transform == null) return; //Stops the rest of the function from running if there is no object under the player's mouse
        //Handles left-click inputs
        if (Input.GetMouseButtonDown(0) && !isOverUI)
        {
            
            //Handles the player input
            ClickHandler(hit);
            
        }

        //Handles right-click inputs
        if (Input.GetMouseButtonDown(1) && !isOverUI)
        {
            //Handles the player input
            RightClickHandler(hit);

        }

        

        //If the player is building the ghost object to represent the building is moved to the location of the mouse 
        if (isBuilding)
        {
            MoveGhostBuilding(hit);
            
        }

    }

    /// <summary>
    /// Loops through all the selected units and removes them from the list.
    /// </summary>
    [Client]
    private void DeselectAll()
    {
        foreach (Unit unit in selectedUnits) 
        {
            unit.Deselected(); //Calls the deselected function on the unit script to handle the unit being deselected
        }

        selectedUnits.Clear();//Clears the selected unit list
    }

    /// <summary>
    /// Casts a ray out from the camera based on where the player mouse is on the screen
    /// </summary>
    /// <returns>
    /// Returns a raycast hit of where the ray hit
    /// </returns>
    [Client]
    private RaycastHit GetRayFromScreen() //Returns the hit of a ray cast from a point on the screen
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); //Creates a ray from the when the mouse is on the screen
        RaycastHit hit;
        //Casts the ray created
        Physics.Raycast(ray, out hit);
        return hit; //Returns the result of the raycast
    }


    /// <summary>
    /// Interperates a click from the player based on what the mouse is hovering over on the screen.
    /// </summary>
    /// <param name="hit">
    /// Takes hit as an parameter so an action can be decided upon, base on what object the player is clicking on.
    /// </param>
    [Client]
    private void ClickHandler(RaycastHit hit)
    {
        if (hit.transform == null || isBuilding) return;
        //Records the unit clicked on
        Unit currentUnit;
        //Checks if the object is a unit and is on the right team
        if (hit.transform.TryGetComponent<Unit>(out currentUnit) && currentUnit.team == team && !currentUnit.selected) //Selects a unit if the player clicks on a non selected unit
        {
            ClientSelectUnit(currentUnit); //If the unit hasn't been selected yet this adds it to the list
        }
        else if(currentUnit != null && currentUnit.team == team && currentUnit.selected) //Deselects the unit if the player clicks on a unit that is selected
        {
            ClientDeselectUnit(currentUnit); //If the unit has already been selected this removes it from the list
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
                    Vector3 target = SoldierPosition(hit.point, selectedUnits.IndexOf(unit), selectedUnits.Count);
                    //print(connectionToServer.connectionId);
                    unit.CmdMove(target, netId);
                    
                }
                else
                {
                    removeAt.Add(unit);
                }
                
            }
            
            foreach (Unit unit in removeAt) //Loops through the selected list and removes any units that have been destroyed.
            {
                    units.Remove(unit);
            }
            removeAt.Clear();
            
        }
    }

    

    /// <summary>
    /// Creates a square grid of vector3 positions when the selected units array is used. Divides the index of the unit by the grid side length to find the location
    /// inside the grid.
    /// </summary>
    /// <param name="hitPosition">The location the player clicked on, which is the location the soldiers have to move to</param>
    /// <param name="index">Decides where in the grid the unit will be</param>
    /// <param name="selectedCount">The number of selected units that need to be moved</param>
    /// <returns></returns>
    private Vector3 SoldierPosition(Vector3 hitPosition, int index, int selectedCount)
    {
        float spread = 1.5f; //Distance between the units
        int offsetMagnitude = Mathf.RoundToInt(Mathf.Sqrt(selectedCount));//Calculates the magnitude of the offset required so that the units will be in the centre of the grid.
        Vector3 offset = new Vector3(-offsetMagnitude / 2, 0, offsetMagnitude / 2);//The vector of the offset
        Vector3 rawVector = new Vector3(hitPosition.x + index % offsetMagnitude * spread, 0, hitPosition.z - (index/offsetMagnitude) * spread); //Calculates the position of the unit inside the grid
        return rawVector + offset;//returns the target location for the unit
    }

    /// <summary>
    /// Checks the object that has been right-clicked on to check that it is a building. 
    /// If it is a building that can be selected like a production building the SelectBuilding function will be run on the building script to open up all relevent UI for that building.
    /// </summary>
    /// <param name="hit">Takes the hit from the player mouse position</param>
    [Client]
    private void RightClickHandler(RaycastHit hit)
    {
        Building building;
        if(hit.transform.TryGetComponent(out building) && selectedBuilding == null) //Checks to see if the object has a building
        {
            if(building.team == team) //Checks the building team matches the player team
            {
                SelectBuilding(building); //Handles selecting the building
            }
        }
        else if(selectedBuilding != null) //Deselects the building if a building is already selected
        {
            DeselectBuilding(selectedBuilding); 
        }
    }


   
    [Client] //Handles selecting a building 
    private void SelectBuilding(Building building)
    {
        selectedBuilding = building;//Sets the selected building variable to the newly selected building 
        
        building.Selected();//Opens up UI if applicable
    }

    [Client] //Handles deselecting a building
    private void DeselectBuilding(Building building)
    {
        selectedBuilding = null; 
        building.Deselected(); //Turns off UI
    }

    [Client] //Selected units are only needed on the client
    private void ClientSelectUnit(Unit unit) 
    {
        unit.Selected(); //Runs the selected function in the unit class
        selectedUnits.Add(unit); //Adds the unit to the list of selected units
    }

    [Client]
    private void ClientDeselectUnit(Unit unit)
    {
        unit.Deselected(); //Runs the deselected function in the unit class
        selectedUnits.Remove(unit);//Removes the unit from the list 
    }

    

    private void BuildButtonPressed() //Manages the button press depending on whether the player is building or not
    {
        if (!isLocalPlayer) return; //Only runs on the local player object
        if (!isBuilding)
        {
            StartBuilding(); //Creates a ghost building and allows the player to place a building
        }
        else
        {
            StopBuilding(); //Destroys the ghost building and stops the player from placing a building
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
        //Creates a "ghost" of the building to be created so the player can see where the building will be
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

    /// <summary>
    /// Shows the health and supply information of the entity underneath the mouse. 
    /// A UI element is turned on when the player moves their mouse over an entity.
    /// Health and supply information from the entity is taken and put into text boxes.
    /// When the player moves their mouse away the UI element is turned off.
    /// </summary>
    /// <param name="hit">Takes the raycast hit to get the entity data</param>
    private void ShowEntityInformation(RaycastHit hit)
    {
        EntityBase entity;
        if(hit.transform != null && hit.transform.TryGetComponent(out entity)) //Checks to see if the object is an entity
        {
            infoPanel.gameObject.SetActive(true); //turns the info panel on
            healthText.text = $"Health: {entity.health}"; //Shows entity health
            supplyText.text = $"Supplies: {entity.supplyStores}/{entity.maximumCapacity}"; //Shows entity supply levels

        }
        else
        {
            infoPanel.gameObject.SetActive(false); //Turns off the info panel 
        }

        
    }

    /// <summary>
    /// Finds a gamobjects by name that are inactive
    /// </summary>
    /// <param name="name">The name of the gameobject to find</param>
    /// <returns></returns>
    GameObject FindInActiveObjectByName(string name)
    {
        //Finds all transforms in the scene
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        //Loops through all objects
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject; //returns an object with the same name as the name given
                }
            }
        }
        return null;
    }


    /// <summary>
    /// Sends a command to the server to place a building.
    /// The server casts the same ray as the player and checks to see if the area the player wants to place the building in is free
    /// If the area is free a building script is created and certain values on the building script are changed, then the building is spawned across the network.
    /// </summary>
    [Command]
    private void CmdPlaceBuilding(int index, Ray ray)
    {
        if (inLobby) return; //Doesn't run if in the lobby
        RaycastHit hit;
        //Casts the ray created
        Physics.Raycast(ray, out hit); //Casts the same ray that the player cast
        if (hit.transform.CompareTag(buildings[index].placementTag)) //Checks if the tag the ray hits matches the tag the building requires
        {
            Building newBuilding = buildings[index]; //creates a new building script
            newBuilding.team = team; //Sets the building team to the correct team
            newBuilding.player = this; //Sets the building's player
            GameObject build = Instantiate(newBuilding.prefab, hit.point, Quaternion.identity); //Creates an instance of the building 
            
            NetworkServer.Spawn(build); //Spawns the building across the network

        }
        

    }

}
