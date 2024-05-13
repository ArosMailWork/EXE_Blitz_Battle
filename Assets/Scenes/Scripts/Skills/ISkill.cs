using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SkillType
{
    Utilities, Weaponary, Buff 
}

[Serializable]
public class ISkill : ScriptableObject
{
    public SkillType skillType;
    public string Name;
    public float Cooldown;
    public bool InputStatus;

    [CanBeNull] public GameObject SkillPrefab;

    public void Activate(PlayerController _playerController, SkillSlot skillSlot, bool TapMode = false)
    {
        //Debug.Log(Name + ": Activated! CD: " + Cooldown);
        if (SkillPrefab != null) SkillActiveAnimator(_playerController, skillSlot, TapMode);
        else SkillActive(_playerController);
    }

    public virtual void SkillActive(PlayerController _playerController) {}
    public virtual void SkillActiveAnimator(PlayerController _playerController, SkillSlot skillSlot, bool TapMode = false) {}
}
