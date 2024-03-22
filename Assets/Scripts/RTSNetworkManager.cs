using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [Scene][SerializeField] private string menuScene = string.Empty;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    

    //List to keep track of the players on the server
    public List<Player> players = new List<Player>();
    
    public bool[] teams = new bool[4];
    //Runs when a player joins the server by overriding the OnServerAddPlayer function
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == menuScene)
        {
            NetworkRoomPlayerLobby roomPlayerLobby = Instantiate(roomPlayerPrefab);


        }



        //Finds the correct player object
        Player player = conn.identity.GetComponent<Player>();
        //Sets the player team
        for(int i = 0; i < 4; i++)
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
        //Adds the player to the players list
        players.Add(player);

        
        
    }

    //Handles players disconnecting
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        //Runs the overrided code
        base.OnServerDisconnect(conn);
        //Finds the player that has disconnected
        Player player = conn.identity.GetComponent<Player>();
        //Removes the player from the list
        players.Remove(player);
        //Frees up space for another player to join the team
        teams[player.team] = false;
        
    }
}
