using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    //Network manager
    public RTSNetworkManager RTSNetworkManager;
    //IP address input
    public TMP_InputField IPinput;


    /// <summary>
    /// Starts a server host when a button on the main menu is pressed
    /// </summary>
    public void Host()
    {
        RTSNetworkManager.StartHost();
    }

    /// <summary>
    /// Runs the network manager join function with the ip input text as the ip address
    /// </summary>
    public void Join()
    {
        RTSNetworkManager.networkAddress = IPinput.text;
        
        RTSNetworkManager.StartClient();
    }
}
