using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class RoomMenu : NetworkBehaviour
{
    public static RoomMenu Instance;
    public List<LoadoutSelector> LoadoutSelectors;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void JoinMap(string sceneName) // change to mapID for future
    {
        bool ToggleFlag = true;
        List<LoadoutSelector> selectorsToRemove = new List<LoadoutSelector>();
        
        //remove null stuffs
        foreach (var selector in LoadoutSelectors)
        {
            if (selector == null) LoadoutSelectors.Remove(selector);
        }
        
        //Check if one player not ready, not gonna play
        foreach (var LS in LoadoutSelectors)
        {
            if (!LS.LoadoutToggleUI._ToggleisOnSync.Value) ToggleFlag = false;
        }
        
        if (ToggleFlag) Global_LoadScene(sceneName);
    }
    
    //Global
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
    
    [ObserversRpc]
    public void Global_UnloadScene(string sceneName)
    {
        if(!InstanceFinder.IsServerStarted) return;

        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
    }

    public void Host_LoadScene(string sceneName)
    {
        if(!InstanceFinder.IsServerStarted) return;
        SceneLookupData slud = new SceneLookupData(sceneName);
        SceneLoadData sld = new SceneLoadData(slud)
        {
            MovedNetworkObjects = new NetworkObject[] { NetworkObject },
            PreferredActiveScene = new PreferredScene(slud),
        };

        sld.ReplaceScenes = ReplaceOption.All;
        //Load scenes for several connections at once.
        NetworkConnection[] conns = base.NetworkManager.ServerManager.Clients.Values.ToArray();
        InstanceFinder.SceneManager.LoadConnectionScenes(conns, sld);
    }
    public void Host_UnloadScene(string sceneName)
    {
        if(!InstanceFinder.IsServerStarted) return;

        SceneLookupData slud = new SceneLookupData(sceneName);
        SceneUnloadData sld = new SceneUnloadData(slud)
        {
            PreferredActiveScene = new PreferredScene(slud),
        };

        //Load scenes for several connections at once.
        NetworkConnection[] conns = InstanceFinder.ServerManager.Clients.Values.ToArray();
        InstanceFinder.SceneManager.UnloadConnectionScenes(conns, sld);
    }

    private void Awake()
    {
        Instance = this;
    }
}
