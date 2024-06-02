using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutSelector : NetworkBehaviour
{
    public NetworkObject CLSavePrefab;
    [ReadOnly, CanBeNull] public NetworkObject CLSaveC;
    public bool isOwner;
    public List<SnapScrollView> selectScrollViews;
    public List<ISkill> skillList;
    public Toggle LoadoutToggle;

    [ReadOnly] public List<ISkill> SkillLoadout = new List<ISkill>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (base.IsOwner)
        {
            isOwner = true;
            SpawnCLSave();
        }
        else
        {
            LoadoutToggle.interactable = false;
            this.enabled = false;
        }
        
        foreach (var ssv in selectScrollViews)
        {
            ssv.LSelector = this;
            if (!isOwner) ssv._scrollRect.horizontal = false;
        }
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    [Button]
    public void SpawnCLSave()
    {
        SpawnCLSaveRPC();
    }
    
    [ServerRpc]
    public void SpawnCLSaveRPC()
    {
        CLSaveC = Instantiate(CLSavePrefab);
        base.Spawn(CLSaveC, Owner);
        
        SpawnCLSaveOnServer(CLSaveC, this);
    }

    [ObserversRpc(BufferLast = true)]
    public void SpawnCLSaveOnServer(NetworkObject netObj, LoadoutSelector script)
    {
        script.CLSaveC = netObj;
    }

    public ISkill ChooseSkill(int skillIndex)
    {
        if (skillIndex >= skillList.Count) return skillList[skillList.Count - 1];
        return skillList[skillIndex];
    }
    
    [ServerRpc]
    public void LockLoadout()
    {
        //Debug.Log("sending to server");
        LockLoadoutServer(this, LoadoutToggle.isOn);
    }

    [ObserversRpc(BufferLast = true)]
    void LockLoadoutServer(LoadoutSelector LSelector, bool toggle)
    {
        //Reset Loadout and Load new one
        LSelector.SkillLoadout.Clear(); 
        foreach (SnapScrollView scrollView in LSelector.selectScrollViews)
        {
            scrollView.isLocked = LSelector.LoadoutToggle.isOn;
            int selectedItemIndex = scrollView.currentItemSync.Value;
            ISkill selectedSkill = ChooseSkill(selectedItemIndex);
            LSelector.SkillLoadout.Add(selectedSkill);
            //Debug.Log("Menu Load: " + selectedSkill.Name);
        }
        
        LSelector.CLSaveC.GetComponent<ClientLoadoutSave>().pickedSkills = LSelector.SkillLoadout;
    }
}
