using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{


    //List to keep track of the players on the server
    public List<Player> players = new List<Player>();
    //The locations that the different players' units will be spawned at.
    public List<Vector3> spawnLocations = new List<Vector3>();
    //The units that are spawned at the start of the game
    public List<Unit> startingUnits = new List<Unit>();

    public bool[] teams = new bool[4];
    //Runs when a player joins the server by overriding the OnServerAddPlayer function
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

        base.OnServerAddPlayer(conn);


        //Finds the correct player object
        Player player = conn.identity.GetComponent<Player>();
        //Sets the player team
        for (int i = 0; i < 4; i++)
        {
            if (teams[i] == false)
            {
                player.team = i;

                teams[i] = true;
                break;
            }
        }
        //sets the player id
        player.networkConnectionToClient = conn;
        //Runs the On load player function
        player.ClientRpcOnLoad();
        //Spawn player
        print($"Player:{player} Team:{player.team}");
        SpawnStartingUnits(player);
        //Adds the player to the players list
        players.Add(player);



    }

    /// <summary>
    /// Loops through the list of starting units and spawns them with the correct player information
    /// </summary>
    /// <param name="player">The player that has just joined the game</param>
    private void SpawnStartingUnits(Player player)
    {
        //Loops through the units
        foreach(Unit unit in startingUnits)
        {
            
            try
            {
                //creates a new instance of the unit class
                Unit newUnit = unit;
                newUnit.team = player.team;
                newUnit.player = player;
                newUnit.supplyStores = newUnit.maximumCapacity;
                GameObject unitGameObject = Instantiate(newUnit.prefab, spawnLocations[player.team], Quaternion.identity);
                NetworkServer.Spawn(unitGameObject);
                unitGameObject.GetComponent<Unit>().ClientRpcTurnOffNavmeshAgent();
            }
            catch (Exception ex)
            {
                print(ex);
                throw;
            }
            

        }
    }

    //Handles players disconnecting
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        
        //Finds the player that has disconnected
        Player player = conn.identity.GetComponent<Player>();
        //Removes the player from the list
        players.Remove(player);
        //Frees up space for another player to join the team
        teams[player.team] = false;
        //Runs the overrided code
        base.OnServerDisconnect(conn);

    }


    /// <summary>
    /// Checks if all the players are ready and starts the game. If not all the players are ready the game won't start, this is run by the server when any player presses the ready button.
    /// </summary>
    [Server]
    public void PlayerReady()
    {

        foreach (Player player in players)
        {
            if (!player.ready)
            {
                return;
            }
        }

        StartGame();
    }


    /// <summary>
    ///Loops through all of the players and puts them into the game, allowing them to interact with units and place buildings.
    /// </summary>
    private void StartGame()
    {
        foreach(Player player in players)
        {
            player.inLobby = false;

            player.ClientRpcStartGame();
            //Spawn the starting units here later
            
        }
    }

}

    
