using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class LoadoutSetter : NetworkBehaviour
{
    public NetworkConnection conn;
    public SkillsHolder skillHolder;
    public List<ISkill> skillList;
    public Color selectedColor;
    
    [Button]
    [ServerRpc(RunLocally = true)]
    public void LoadSkill(List<ISkill> pickedSkills)
    {
        conn = base.LocalConnection;
        
        skillList = pickedSkills;
        skillHolder.AssignSkills(pickedSkills);
    }

    [Button]
    [ServerRpc(RunLocally = true)]
    public void LoadColor(Color colorSet)
    {
        selectedColor = colorSet;
        skillHolder._playerController.PlayerMat.color = selectedColor;
    }
}
