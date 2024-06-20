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
    
    public readonly SyncDictionary<int, PlayerLifeAmount> _playerLifesSync 
                    = new SyncDictionary<int, PlayerLifeAmount>(new SyncTypeSettings(1f));
    [ShowInInspector] public Dictionary<int, PlayerLifeAmount> _playerLifes 
                    = new Dictionary<int, PlayerLifeAmount>();
    
    public static ScoreManager Instance;

    private void Awake()
    {
        _playerLifesSync.OnChange += _playerLifes_OnChange;
    }
    private void Start()
    {
        Instance = this;
        GameManager.Instance.ScoreM = Instance;
    }
    void _playerLifes_OnChange(SyncDictionaryOperation op, int key, PlayerLifeAmount value, bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Add:
                if (!_playerLifes.ContainsKey(key)) _playerLifes.Add(key, value);
                break;
            case SyncDictionaryOperation.Set:
                _playerLifes[key] = value;
                if(asServer) CheckWinner();
                break;
            case SyncDictionaryOperation.Remove:
                if (_playerLifes.ContainsKey(key)) _playerLifes.Remove(key);
                break;
            case SyncDictionaryOperation.Clear:
                _playerLifes.Clear();
                break;
            case SyncDictionaryOperation.Complete:
                break;
        }

    }
    
    void CheckWinner()
    {
        int playersWithLife = 0;
        PlayerLifeAmount winner = new PlayerLifeAmount();
        foreach (var playerLife in _playerLifes.Values)
        {
            if (playerLife.currentLife > 0)
            {
                playersWithLife++;
                winner = playerLife;
            }
        }

        Debug.Log("Check Winner: " + playersWithLife);
        if (playersWithLife == 1)
        {
            GameManager.Instance.PlayerWonEnd(winner.ClientID);
        }
    }

    public bool PlayerLifeRemain(int PlayerID, int PredictModify = 0)
    {
        foreach (var LifeAmountData in _playerLifesSync.Values)
        {
            if(LifeAmountData.currentLife - PredictModify <= 0) return false;
        }
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayer(int ClientID, PlayerController pController)
    {
        PlayerLifeAmount newPlayer = new PlayerLifeAmount();
        //newPlayer.ClientID = ClientManager.Connection.ClientId;
        newPlayer.ClientID = ClientID;
        newPlayer.pController = pController;
        newPlayer.maxLife = maxLifeDefault;
        newPlayer.currentLife = newPlayer.maxLife;
        
        //_playerLifesSync.Add(ClientManager.Connection.ClientId, newPlayer);
        _playerLifesSync.Add(ClientID, newPlayer);
    }
    
    //[ServerRpc]
    public void PlayerLoseLife(int PlayerID, int amount = 1) //server only
    {
        var keys = new List<int>(_playerLifesSync.Keys);
        foreach (var key in keys)
        {
            var player = _playerLifesSync[key];
            if (player.ClientID == PlayerID)
            {
                var updatedPlayer = player;
                updatedPlayer.currentLife -= amount;
                _playerLifesSync[key] = updatedPlayer;
                
                PlayerDead(player.ClientID);
            }
        }
    }

    public void PlayerDead(int PlayerID)
    {
        doPlayerDead(PlayerID);
    }
    //[ObserversRpc]
    void doPlayerDead(int PlayerID)
    {
        Debug.Log("PlayerID " + PlayerID + " Dead!!");

        foreach (var playerLifeData in _playerLifesSync)
        {
            if (playerLifeData.Value.ClientID == PlayerID && playerLifeData.Value.currentLife <= 0)
            {
                //call anything from pController here
                playerLifeData.Value.pController.PlayerDead();
            }
        }
        
    }

}


