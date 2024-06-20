using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Spawning;
using FishNet.Managing.Scened;
using FishNet.Object;
using JetBrains.Annotations;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [CanBeNull] public ScoreManager ScoreM;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }
    
    private void Start()
    {
        Instance = this;
    }

    public void PlayerDead(int PlayerID) //take as a hub
    {
        if(ScoreM != null) ScoreM.PlayerLoseLife(PlayerID);
    }

    public void PlayerWonEnd(int PlayerID)
    {
        Debug.Log("Winner: " + PlayerID);
        
        //Global_LoadScene("Award");
    }
    
    [ObserversRpc]
    public void Global_LoadScene(string sceneName)
    {
        if(!InstanceFinder.IsServerStarted) return;

        var slud = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(slud)
        {
            PreferredActiveScene = new PreferredScene(slud),
        };
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        
    }
}
