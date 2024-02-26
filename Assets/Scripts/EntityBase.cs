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
    [SyncVar]public int team;
    public List<Material> teamColours = new List<Material>();

    [Header("Gameplay")]
    //Whether the entity is a unit or a building
    public string type = "entity";
    //health
    public int health = 10;
    //supplies
    public int supplyStores = 10;
    //The maximum number of supplies the entity can have
    public int maximumCapacity = 10;
    
}
