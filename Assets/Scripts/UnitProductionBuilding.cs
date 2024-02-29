using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using static UnityEditor.ObjectChangeEventStream;

public class UnitProductionBuilding : Building
{
    public List<Unit> units = new List<Unit>();

    List<Unit> queue = new List<Unit>();

    public GameObject unitProductionUI;
    public TMP_Dropdown unitDropdown;
    public Button createUnit;

    public override void Selected()
    {
        base.Selected();
        if (unitProductionUI == null)
        {
            unitProductionUI = FindInActiveObjectByName("UnitProductionMenu");
            createUnit = createUnit.GetComponentInChildren<Button>();
            //Finds the gameobjects for the UI
            unitDropdown = FindInActiveObjectByName("UnitProductionDropdown").GetComponent<TMP_Dropdown>();
            createUnit = FindInActiveObjectByName("MakeUnitButton").GetComponent<Button>();
            createUnit.onClick.AddListener(() => { CmdAddUnitToQueue(); });
        }
        player.builders++;
        unitProductionUI.SetActive(true);

    }



    public override void Deselected()
    {
        base.Deselected();

        unitProductionUI.SetActive(false);
    }

    private void PopulateDropdown()
    {
        unitDropdown.ClearOptions();
        foreach (Unit unit in units) 
        {
            //unitDropdown.AddOptions()
        }
    }

    [Command]
    private void CmdAddUnitToQueue()
    {

    }

    //code taken from stackoverflow
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

}
