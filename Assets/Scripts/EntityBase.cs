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
    [SyncVar]public int health = 10;
    //supplies
    [SyncVar]public int supplyStores = 10;
    //The maximum number of supplies the entity can have
    public int maximumCapacity = 10;
    //The effect that appears when the entity is attacked
    public ParticleSystem damageEffect;

    /// <summary>
    /// Tells the unit which player owns it
    /// </summary>
    [ClientCallback]
    private void Start()
    {
        StartCoroutine(SetPlayerReference()); //TODO: Make a better system where the server tells the player which player is correct
    }

    /// <summary>
    /// Runs on the server when the unit takes damage.
    /// Subtracts the damage variable from the health of the unit and destroys the unit if the health goes below zero
    /// </summary>
    /// <param name="damage">The amount of health to be deducted</param>
    /// <returns>Returns true if the unit dies and false if the unit is still alive</returns>
    [Server]
    public bool TakeDamage(int damage)
    {
        health -= damage;
        
        if(health <= 0)
        {
            NetworkServer.Destroy(gameObject);
            return true;
        }
        else
        {
            ClientRpcShowEffect();
        }
        return false;
    }

    /// <summary>
    /// Runs when the entity takes damage and shows the damage particles on all clients
    /// </summary>
    [ClientRpc]
    private void ClientRpcShowEffect()
    {
        if (damageEffect != null)
        {
            damageEffect.Play();
        }
    }

    /// <summary>
    /// Runs when the entity spawns in after a 0.5 second delay to allow time for the server to spawn in the local player.
    /// The colour of the unit will change to the correct team colour after this function runs
    /// </summary>
    /// <returns></returns>
    [Client]
    private IEnumerator SetPlayerReference() //TODO: Use an rpc or something like that to improve this because this is bad :(
    {
        yield return new WaitForSeconds(0.5f);
        Player localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
        if (team == localPlayer.team)
        {
            player = localPlayer;
        }
    }
}
