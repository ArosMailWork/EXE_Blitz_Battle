using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : NetworkBehaviour
{
    public Toggle Toggle;
    public TextMeshProUGUI Text;

    public string isOff;
    public string isOn;

    public bool isOwner;
    public bool ToggleValueDebug;
    private readonly SyncVar<bool> _ToggleisOnSync = new SyncVar<bool>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner) 
            isOwner = true;
        else 
            this.enabled = false;
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }
    private void Awake()
    {
        _ToggleisOnSync.OnChange += UpdateDebugValue;
    }
    void UpdateDebugValue(bool prev, bool next, bool asServer)
    {
        //Debug.Log("Sync Value worked, yayyyy");
        ToggleValueDebug = next;
        if(isOwner) UpdateTextRequest(this);
    } // activate when _ToggleisOnSync change


    public void ToggleUpdateValue() //Set from the Toggle (cant call directly cuz need to change to bool :3)
    {
        UpdateSyncValue(this, this.Toggle.isOn);
    }

    [ServerRpc] 
    void UpdateSyncValue(ToggleUI obj, bool ToggleState) // take value then Update it
    {
        _ToggleisOnSync.Value = ToggleState;
        //Debug.Log("Update Sync Value: " + _ToggleisOnSync.Value);
    }

    [ServerRpc]
    void UpdateTextRequest(ToggleUI obj)
    {
        Updatetext(obj);
    }
    [ObserversRpc(BufferLast = true)]
    void Updatetext(ToggleUI obj)
    {
        obj.Text.text = _ToggleisOnSync.Value ? isOn : isOff;
        //Debug.Log("received" + _ToggleisOnSync.Value);
    }
    
}
