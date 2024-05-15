using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerNameInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField inputField = null;
    //[SerializeField] private Button continueButton = null;

    public string displayName {  get; private set; }

    //private const string PlayerPrefsNameKey = "PlayerName";
    public void SetPlayerName()
    {
        displayName = inputField.text;
    }
}
