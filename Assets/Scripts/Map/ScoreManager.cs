using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct PlayerLifeAmount
{
    public int ClientID;
    public PlayerController pController;
    public int currentLife;
    public int maxLife;
}

[RequireComponent(typeof(GameManager))]
public class ScoreManager : NetworkBehaviour
{
    public int maxLifeDefault = 3;
    
    public readonly SyncDictionary<NetworkConnection, PlayerLifeAmount> _playerLifesSync = new();
    [ShowInInspector] public Dictionary<NetworkConnection, PlayerLifeAmount> _playerLifes 
                    = new Dictionary<NetworkConnection, PlayerLifeAmount>();
    
    public static ScoreManager Instance;

    private void Awake()
    {
        _playerLifesSync.OnChange += _playerLifes_OnChange;
    }
    private void Start()
    {
        Instance = this;
    }

    void _playerLifes_OnChange(SyncDictionaryOperation op, NetworkConnection key, PlayerLifeAmount value, bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Add:
                if (!_playerLifes.ContainsKey(key)) _playerLifes.Add(key, value);
                break;
            case SyncDictionaryOperation.Set:
                _playerLifes[key] = value;
                break;
            case SyncDictionaryOperation.Remove:
                if (_playerLifes.ContainsKey(key)) _playerLifes.Remove(key);
                break;
            case SyncDictionaryOperation.Clear:
                _playerLifes.Clear();
                break;
            case SyncDictionaryOperation.Complete:
                // Optional: Perform any additional logic after all operations are complete
                break;
        }
    }

    [ServerRpc]
    public void AddPlayer(NetworkConnection conn, PlayerController pController)
    {
        PlayerLifeAmount newPlayer = new PlayerLifeAmount();
        //newPlayer.ClientID = ClientManager.Connection.ClientId;
        newPlayer.ClientID = conn.ClientId;
        newPlayer.pController = pController;
        newPlayer.maxLife = maxLifeDefault;
        newPlayer.currentLife = newPlayer.maxLife;
        
        _playerLifesSync.Add(ClientManager.Connection, newPlayer);
    }
    
    public void PlayerLoseLife(int PlayerID, int amount = 1)
    {
        
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


