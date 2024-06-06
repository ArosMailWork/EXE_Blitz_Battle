using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Ults/Air Hop")]
public class AirHop : ISkill
{
    public Vector3 PosOffset = new Vector3(0,-0.6f, 0);
    public int health;
    public float lifeTime;

    private void Awake()
    {
        Platform platform = SkillPrefab.GetComponent<Platform>();
        platform.SetHealth(health);
        platform.SetLifeTime(lifeTime);
    }

    public override void SkillActive(PlayerController _playerController)
    {
        //_playerController.Dash().Forget();
        Debug.Log("Air hop called");
        _playerController.PlayerSpawnObj(SkillPrefab, PosOffset, Quaternion.identity);
    }
}
