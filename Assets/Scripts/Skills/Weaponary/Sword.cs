using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Weapon/Sword", order = 1)]
public class Sword : ISkill
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

        float inputX = (float)_frameInput.MoveInputVec.x;
        float inputY = (float)_frameInput.MoveInputVec.y;

        if (inputX > 0) inputX = 1;
        else if (inputX < 0) inputX = -1;
        else inputX = _playerController.LastMove.x;

        skillSlot.PlayAnimVar(_frameInput, inputX, inputY);
        
        //make sure this shit only activate once
        if(skillSlot.InputStatus) 
            skillSlot.SkillPrefabNetworkAnimator.SetTrigger("Trigger");
        
    }
}
