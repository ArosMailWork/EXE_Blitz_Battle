using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class Loadout : NetworkBehaviour
{
    public SkillsHolder skillHolder;
    public List<ISkill> skillList;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        skillHolder = GetComponentInChildren<SkillsHolder>();
        skillHolder.AssignSkills(skillList);
    }
}
