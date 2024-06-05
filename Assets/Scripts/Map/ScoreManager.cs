using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using UnityEngine;

[System.Serializable]
public struct PlayerLifeAmount
{
    public int ClientID;
    public PlayerController pController;
    public int currentLife;
    public int maxLife;
}

[System.Serializable]
public struct ListPlayerLifeAmount
{
    public List<PlayerLifeAmount> _playerLifeAmountsList;
}

[RequireComponent(typeof(GameManager))]
public class ScoreManager : NetworkBehaviour
{
    public readonly SyncVar<ListPlayerLifeAmount> playerLifeAmountSync = new SyncVar<ListPlayerLifeAmount>(new SyncTypeSettings(1f));
    public ListPlayerLifeAmount playerLifeAmount;

    private void Awake()
    {
        playerLifeAmountSync.OnChange += on_LifeAmount;
    }
    void on_LifeAmount(ListPlayerLifeAmount prev, ListPlayerLifeAmount next, bool asServer)
    {
        playerLifeAmount = next;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }

    [ServerRpc]
    public void SetStartLife(PlayerController pController, int setMaxLife)
    {
        Debug.Log("Score Set: " + pController.PlayerID);
        PlayerLifeAmount setLifeData = new PlayerLifeAmount();
        setLifeData.pController = pController;
        setLifeData.ClientID = pController.PlayerID;
        setLifeData.maxLife = setMaxLife;
        setLifeData.currentLife = setMaxLife;
        
        bool found = false;
        for (int i = 0; i < playerLifeAmount._playerLifeAmountsList.Count; i++)
        {
            if (playerLifeAmount._playerLifeAmountsList[i].ClientID == pController.PlayerID)
            {
                playerLifeAmount._playerLifeAmountsList[i] = setLifeData; // Overwrite existing data
                found = true;
                break;
            }
        }
        if (!found)
            playerLifeAmount._playerLifeAmountsList.Add(setLifeData); // Add new data
        
        playerLifeAmountSync.Value = playerLifeAmount; // Update the SyncVar
    }

    List<PlayerLifeAmount> bufferList; // Take a clone of the list
    PlayerLifeAmount BufferPLA;
    public void PlayerLoseLife(int PlayerID, int amount = 1)
    {
        bufferList = new List<PlayerLifeAmount>(playerLifeAmount._playerLifeAmountsList);
        BufferPLA = new PlayerLifeAmount();
        
        for (int i = 0; i < bufferList.Count; i++)
        {
            //cant adjust directly so just use buffer then overwrite
            if (bufferList[i].ClientID == PlayerID)
            {
                BufferPLA = bufferList[i];
                BufferPLA.currentLife -= amount;
                bufferList[i] = BufferPLA;
                break;
            }
        }

        if (BufferPLA.pController != null)
        {
            if(BufferPLA.currentLife <= 0) PlayerDead(BufferPLA.ClientID, BufferPLA.pController);   
            playerLifeAmount._playerLifeAmountsList = bufferList; // Update the list with modified data
            playerLifeAmountSync.Value = playerLifeAmount; // Update the SyncVar
        }
    }

    [ServerRpc]
    void PlayerDead(int PlayerID, PlayerController pController)
    {
        doPlayerDead(PlayerID, pController);
    }
    
    [ObserversRpc]
    void doPlayerDead(int PlayerID, PlayerController pController)
    {
        Debug.Log("PlayerID " + PlayerID + " Dead!!");
        
        if (pController)
        {
            //call anything from pController here
        }
    }

}


