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
    public TextMeshProUGUI productionIndicator;
    public Button createUnit;
    public string spawnable = "ground";
    public Transform spawnPointBase;
    public Transform spawnPointFinder;
    bool hasSpawnPoint = false;
    public Vector3 spawnOffset;
    Vector3 spawnPoint;
    float nextSpawn;
    bool makingUnits = false;
    bool makingUnitsHere = false;

    [Client]
    public override void Selected()
    {
        base.Selected();
        if (unitProductionUI == null)
        {
            //Finds the gameobjects for the UI
            unitProductionUI = FindInActiveObjectByName("UnitProductionMenu");
            unitDropdown = FindInActiveObjectByName("UnitProductionDropdown").GetComponent<TMP_Dropdown>();
            createUnit = FindInActiveObjectByName("MakeUnitButton").GetComponent<Button>();
            productionIndicator = FindInActiveObjectByName("IndicatorText").GetComponent<TextMeshProUGUI>();
            createUnit.onClick.AddListener(() => { AddToQueue(unitDropdown.value, team); }); //Runs the function CmdAddUnitToQueue when the button is pressed
        }
        makingUnitsHere = true;
        PopulateDropdown();
        unitProductionUI.SetActive(true);

    }

    public override void Deselected()
    {
        base.Deselected();
        makingUnitsHere = false;
        unitProductionUI.SetActive(false);
        productionIndicator.text = "";
    }

    private void PopulateDropdown()
    {
        unitDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (Unit unit in units) 
        {
            options.Add($"{unit.name} - {unit.price}");
        }
        unitDropdown.AddOptions(options);
    }

    private void AddToQueue(int index, int team)
    {
        if (makingUnitsHere)
        {
            CmdAddUnitToQueue(index, team);
        }
    }
    
    
    [Command(requiresAuthority = false)]
    private void CmdAddUnitToQueue(int index, int team)
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
            ClientRPCFeedBack(true, index);
        }
        else
        {
            ClientRPCFeedBack(false, index);
        }
        
    }

    [ClientCallback]
    private void ClientRPCFeedBack(bool success, int index)
    {
        try
        {
            print(success);
            print(index);
            print(units[index].name);
            print(productionIndicator.text);
            if (success)
            {
                productionIndicator.text = $"Added {units[index].name} to the queue.";
            }
            else
            {
                productionIndicator.text = $"Failed to add {units[index].name} to the queue.";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
        
    }

    [Server]
    private IEnumerator MakeUnits()
    {
        makingUnits = true;
        
        for (int i = 0; i < queue.Count && queue[i] != null; i++ )
        {
            yield return new WaitForSeconds(queue[i].timeToMake);
            
            try
            {
                Unit newUnit = queue[i];
                //newUnit.player = player;
                newUnit.team = team;
                GameObject unit = Instantiate(newUnit.prefab, spawnPoint, Quaternion.identity);
                NetworkServer.Spawn(unit);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
            
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
            
            if (Physics.Raycast(spawnPointFinder.position, -spawnPointFinder.up, out hit))
            {
                Debug.DrawRay(spawnPointFinder.position, -spawnPointFinder.transform.up * hit.distance, Color.green, 1000);
                print(hit.transform.tag);
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
