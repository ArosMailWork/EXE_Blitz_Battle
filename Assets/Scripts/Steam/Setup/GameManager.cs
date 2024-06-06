using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Spawning;
using FishNet.Object;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public ScoreManager ScoreManager;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }

    private void Awake()
    {
        ScoreManager = GetComponent<ScoreManager>();
    }
    private void Start()
    {
        Instance = this;
    }

    public void PlayerDead(int PlayerID)
    {
        if(ScoreManager != null) ScoreManager.PlayerLoseLife(PlayerID);
    }
    
}
