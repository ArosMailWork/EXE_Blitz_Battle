using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Shovel", order = 1)]
public class Shovel : ISkill
{
    public override void SkillActiveAnimator(PlayerController _playerController, SkillSlot skillSlot , bool TapMode)
    {
        //take input then translate it to tell the animator
        FrameInput _frameInput = _playerController._frameInput;

        if (skillSlot.SkillPrefabAnimator == null)
        {
            Debug.Log("Animator plz ?");
            return;
        }
        skillSlot.SkillPrefabAnimator.SetFloat("Horizontal", (float)_frameInput.MoveInputVec.x);
        skillSlot.SkillPrefabAnimator.SetFloat("Vertical", (float)_frameInput.MoveInputVec.y);
        skillSlot.SkillPrefabAnimator.SetBool("Toggle", skillSlot.InputStatus);
        
        if(TapMode) skillSlot.SkillPrefabAnimator.SetTrigger("Trigger");
        
    }
    
    
}
