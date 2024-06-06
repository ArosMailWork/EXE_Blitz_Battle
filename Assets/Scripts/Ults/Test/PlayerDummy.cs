using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class PlayerDummy : NetworkBehaviour
{
    private NetworkManager _networkManager;
    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null) return;

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Debug.Log("HI " + base.LocalConnection);
        
        //Debug.Log("Add To CamView");
        //CameraTracking.Instance.AddObj(this.gameObject);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        Debug.Log("BYE " + base.LocalConnection);
    }
}
