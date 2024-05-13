using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;


public class SkillSlot : MonoBehaviour
{
    [ReadOnly] PlayerController _playerController; 
    public ISkill Skill;
    [CanBeNull, ReadOnly] public Animator SkillPrefabAnimator;
    [CanBeNull, ReadOnly] public GameObject SkillPrefab;
    
    public float currentCD;
    public bool InputStatus; //Debug only
    public bool Locked;

    public void Initialize(PlayerController setPCController)
    {
        _playerController = setPCController;

        if (Skill != null && Skill.skillType == SkillType.Weaponary)
        {
            //Spawn Prefab as a Child 
            SkillPrefab = Instantiate(Skill.SkillPrefab, this.transform);
            SkillPrefabAnimator = SkillPrefab.GetComponent<Animator>();
        }
    }
    private void Update()
    {
        //Timer
        if (currentCD > 0)
        {
            currentCD -= Time.deltaTime;
            return;
        }
    }

    public void Activate(bool InputStatus, bool TapMode = false)
    {
        if (Locked || currentCD > 0) return;
        if (Skill == null)
        {
            Debug.Log("none skill, yay");
            return;
        }

        this.InputStatus = InputStatus;
        Skill.Activate(_playerController, this, TapMode);
        if(Skill.skillType != SkillType.Weaponary) currentCD = Skill.Cooldown;
    }
    
}
