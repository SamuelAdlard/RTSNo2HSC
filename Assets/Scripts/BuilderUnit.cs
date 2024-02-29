using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class BuilderUnit : Unit
{
    public Building refillPoint;
    public Building targetBuilding;
    public GameObject builderUI;

    public override void Selected()
    {
        base.Selected();
        player.builders++;
        builderUI.SetActive(true);
    }

    public override void Deselected()
    {
        base.Deselected();
        player.builders--;
        if (player.builders <= 0)
        {
            builderUI.SetActive(false);
        }
        
    }


}
