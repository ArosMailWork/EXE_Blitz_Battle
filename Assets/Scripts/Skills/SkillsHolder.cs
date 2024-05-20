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
    [ReadOnly] public PlayerController _playerController;
    public int SkillAmount;
    public List<SkillBind> SkillBinds;

    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerController = GetComponentInParent<PlayerController>();

        if (!base.IsOwner)
        {
            this.enabled = false;
            return;
        }
        
        foreach (var skillBind in SkillBinds)
        {
            if (_playerController != null) skillBind.SkillSlots.Initialize(_playerController);
            else Debug.Log("failed: " + _playerController.gameObject.name);
        }

    }

    #region Unity Method

    private void Start()
    {
        InstantResetCD(-1);
    }

    private void Update()
    {
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
