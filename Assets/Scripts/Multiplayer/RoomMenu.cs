using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class RoomMenu : NetworkBehaviour
{
    //Global
    [ObserversRpc]
    public void Global_LoadScene(string sceneName)
    {
        if(!InstanceFinder.IsServerStarted) return;

        var slud = new SceneLookupData(sceneName);
        var sld = new SceneLoadData(slud)
        {
            MovedNetworkObjects = new NetworkObject[] { NetworkObject },
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
}
