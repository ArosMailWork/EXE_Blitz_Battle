using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public struct SkillBind
{
    public KeyCode KeyBind;
    public SkillSlot SkillSlots;
}

//Take Input then summon dae skills XD
public class SkillsHolder : MonoBehaviour
{
    [ReadOnly] PlayerController _playerController;
    public int SkillAmount;
    public List<SkillBind> SkillBinds;

    #region Unity Method

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();

        foreach (var skillBind in SkillBinds)
        {
            skillBind.SkillSlots.Initialize(_playerController);
        }
    }

    private void Start()
    {
        SkillAmount = SkillBinds.Count;
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
        SkillBinds[slot].SkillSlots.Activate(true, true);
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

    #endregion
}
