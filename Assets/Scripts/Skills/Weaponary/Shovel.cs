using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Weapon/Shovel", order = 1)]
public class Shovel : ISkill
{
    public override void SkillActiveAnimator(PlayerController _playerController, SkillSlot skillSlot)
    {
        //take input then translate it to tell the animator
        FrameInput _frameInput = _playerController._frameInput;

        if (skillSlot.SkillPrefabAnimator == null)
        {
            Debug.Log("Animator plz ?");
            skillSlot.AssignAnimators();
            return;
        }

        skillSlot.PlayAnimVar(_frameInput, (float)_frameInput.MoveInputVec.x, (float)_frameInput.MoveInputVec.y);
        //skillSlot.PlayAnimTrigger();
        skillSlot.SkillPrefabNetworkAnimator.SetTrigger("Trigger");
        
    }
}
