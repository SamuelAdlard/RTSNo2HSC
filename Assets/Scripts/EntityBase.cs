using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class EntityBase : NetworkBehaviour
{
    [Header("Team")]
    //Leads to owner player object
    public Player player;
    //the id of the player who owns the entity
    [SyncVar]public int team; //TODO: Make a system that properly handles teams
    public List<Material> teamColours = new List<Material>();

    [Header("Gameplay")]
    //health
    public int health = 10;
    //supplies
    public int supplyStores = 10;
    //The maximum number of supplies the entity can have
    public int maximumCapacity = 10;
    //Whether the entity is a unit or a building
    public int type = 0; //0 is unit, 1 is building
}
