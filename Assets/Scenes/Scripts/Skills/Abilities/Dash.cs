using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Dash", order = 1)]
public class Dash : ISkill
{
    public override void SkillActive(PlayerController _playerController)
    {
        _playerController.Dash();
    }
    
    
}
