using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class WeaponaryFix : NetworkBehaviour
{
    [SerializeField] private SkillSlot[] _skillSlot;

    public void AssignCheck()
    {
        //SkillsHolder skillsHolder = gameObject.GetComponentInChildren<SkillsHolder>();
        //if(skillsHolder != null) skillsHolder.AssignAnimators();
        //foreach (var ss in _skillSlot)
        //{
            //ss.AssignAnimators();
        //}
    }
}
