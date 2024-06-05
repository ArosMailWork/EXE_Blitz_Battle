using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public struct SkillBind
{
    public KeyCode KeyBind;
    public SkillSlot SkillSlots;
}

//Take Input then summon dae skills XD
public class SkillsHolder : NetworkBehaviour
{
    public PlayerController _playerController;
    public LoadoutSetter loadoutSetter;
    public int SkillAmount;
    public bool toggle = true;
    public List<SkillBind> SkillBinds;

    public static SkillsHolder Instance;

    #region Network Setup

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!_playerController.IsOwner)
        {
            toggle = false;
            return;
        }

        //Debug.Log("Skill Holder" + base.LocalConnection + " " + _playerController.IsOwner);
        //execute load on client side Loadout then tell spawn XD
        if(loadoutSetter != null && _playerController.IsOwner) loadoutSetter.LoadSkill(ClientLoadoutSave.Instance.pickedSkills);
        SpawnSkillObj();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    [Button]
    [ServerRpc]
    public void SpawnSkillObj()
    {
        Debug.Log("Server Spawn !!! " + _playerController.LocalConnection);

        foreach (var skillBind in SkillBinds)
        {
            Instance = this;
            if (_playerController != null)
            {
                skillBind.SkillSlots.InitServer(_playerController.gameObject, _playerController); //Spawn Test
                skillBind.SkillSlots.ObserverSetup(_playerController); //Assign Test
                
                //skillBind.SkillSlots.Initialize(_playerController.gameObject);
            }
            else Debug.Log("cant find playerController");
        }
    }

    #endregion
    

    #region Unity Method

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void Start()
    {
        InstantResetCD(-1);
    }

    private void Update()
    {
        if(!toggle) return;
        foreach (var skillBind in SkillBinds)
        {
            if (Input.GetKeyDown(skillBind.KeyBind)) skillBind.SkillSlots.Activate(true);
            if (Input.GetKeyUp(skillBind.KeyBind)) skillBind.SkillSlots.Activate(false);
        }
    }

    #endregion
    #region Ults
    
    [Button]
    public void ActivateSkillSlot(int slot)
    {
        SkillBinds[slot].SkillSlots.Activate(true);
    }
    
    [Button]
    public void InstantResetCD(int slot)
    {
        if (slot > SkillBinds.Count)
        {
            Debug.Log("Slot not exist, check plz");
            return;
        }
        
        if (slot <= 0)
        {
            foreach (var ss in SkillBinds)
            {
                if (ss.SkillSlots.currentCD >= 0) ss.SkillSlots.currentCD = 0;
            }
            return;
        }
        
        // Update the original struct in the list
        SkillBinds[slot].SkillSlots.currentCD = 0;
    }
    
    public void SetLock(int slot, bool toggle)
    {
        if (slot > SkillBinds.Count)
        {
            Debug.Log("Slot not exist, check plz");
            return;
        }
        
        if (slot <= 0)
        {
            foreach (var ss in SkillBinds)
            {
                if (ss.SkillSlots.currentCD >= 0) ss.SkillSlots.Locked = toggle;
            }
            return;
        }
        
        // Update the original struct in the list
        SkillBinds[slot].SkillSlots.currentCD = 0; 
        
    }
    
    public void AssignSkills(List<ISkill> skillAssignList)
    {
        int index = 0;
        foreach (var skillBind in SkillBinds)
        {
            skillBind.SkillSlots.Skill = skillAssignList[index];
            index++;
        }
    }

    #endregion
}
