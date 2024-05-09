using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class UnitProductionBuilding : Building
{
    public List<Unit> units = new List<Unit>();

    public List<Unit> queue = new List<Unit>();

    public GameObject unitProductionUI;
    public TMP_Dropdown unitDropdown;
    public Button createUnit;
    public string spawnable = "ground";
    public Transform spawnPointBase;
    public Transform spawnPointFinder;
    bool hasSpawnPoint = false;
    Vector3 spawnPoint;
    float nextSpawn;
    bool makingUnits = false;
   


    public override void Selected()
    {
        base.Selected();
        if (unitProductionUI == null)
        {
            //Finds the gameobjects for the UI
            unitProductionUI = FindInActiveObjectByName("UnitProductionMenu");
            unitDropdown = FindInActiveObjectByName("UnitProductionDropdown").GetComponent<TMP_Dropdown>();
            createUnit = FindInActiveObjectByName("MakeUnitButton").GetComponent<Button>();
            try
            {
                createUnit.onClick.AddListener(() => { CmdAddUnitToQueue(unitDropdown.value, team); }); //Runs the function CmdAddUnitToQueue when the button is pressed
                print("Called command successfully");
            }
            catch (System.Exception ex)
            {
                print("Client side: " + ex);
                
            }
            
            
        }

        PopulateDropdown();
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
        List<string> options = new List<string>();
        foreach (Unit unit in units) 
        {
            options.Add(unit.name);
        }
        unitDropdown.AddOptions(options);
    }

    
    
    [Command(requiresAuthority = false)]
    private void CmdAddUnitToQueue(int index, int team)
    {
        try
        {
            if (units[index].price <= supplyStores && functional)
            {
                if (!hasSpawnPoint)
                {
                    FindSpawnPoint();
                }
                queue.Add(units[index]);
                supplyStores -= units[index].price;
                if (!makingUnits) StartCoroutine(MakeUnits());
            }
            print("Added to queue successfully");
        }
        catch (System.Exception ex)
        {
            print("Failed");
            print("Server side:" + ex);
            
        }
       
    }

    [Server]
    private IEnumerator MakeUnits()
    {
        makingUnits = true;
        
        for (int i = 0; i < queue.Count && queue[i] != null; i++ )
        {
            
            yield return new WaitForSeconds(queue[i].timeToMake);
            Unit newUnit = queue[i];
            newUnit.team = team;
            GameObject unit = Instantiate(newUnit.prefab, spawnPoint, Quaternion.identity);
            
            
            NetworkServer.Spawn(unit);
            
            
        }
        queue.Clear();
        makingUnits = false;
    }

    [Server]
    private void FindSpawnPoint()
    {
        RaycastHit hit;
        for(int i = 0; i < 360; i++)
        {
            if (Physics.Raycast(spawnPointFinder.position, Vector3.down, out hit))
            {
                if (hit.transform.CompareTag(spawnable))
                {
                    hasSpawnPoint = true;
                    spawnPoint = hit.point;
                    break;
                }
            }

            spawnPointBase.Rotate(0, i, 0);
        }
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
