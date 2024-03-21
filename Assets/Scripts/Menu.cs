using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    public RTSNetworkManager RTSNetworkManager;
    public TMP_InputField IPinput;

    public void Host()
    {
        RTSNetworkManager.StartHost();
    }

    public void Join()
    {
        RTSNetworkManager.networkAddress = IPinput.text;
        
        RTSNetworkManager.StartClient();
    }
}
