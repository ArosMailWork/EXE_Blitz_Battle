using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JetBrains.Annotations;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEngine;


public class SkillSlot : NetworkBehaviour
{
    [ReadOnly, SerializeField] PlayerController _playerController; 
    public ISkill Skill;
    [CanBeNull, ReadOnly] public Animator SkillPrefabAnimator;
    [CanBeNull, ReadOnly] public NetworkAnimator SkillPrefabNetworkAnimator;
    [CanBeNull] public NetworkObject SkillPrefab;
    
    public float currentCD;
        
    public bool InputStatus; //Debug only
    public readonly SyncVar<bool> InputStatusSync = new SyncVar<bool>();
    public bool Locked;
    
    [HideInInspector] public GameObject spawnedObject;

    //[ServerRpc] //This do and tell server only (can use for spawn)
    public void Initialize(GameObject obj)
    {
        PlayerController setPCController = obj.GetComponent<PlayerController>();
        ObserverSetup(setPCController);
    }

    [ObserversRpc] //Only Server can Execute this to all everyone
    public void ObserverSetup(PlayerController setPCController)
    {
        _playerController = setPCController;
    }

    public void InitServer(GameObject obj, PlayerController setPCController)
    {
        _playerController = setPCController;
        if (Skill != null && Skill.skillType == SkillType.Weaponary)
        {
            //Spawn Prefab as a Child 
            if (SkillPrefab == null)
            {
                SkillPrefab = Instantiate(Skill.SkillPrefab, this.transform); 
                SkillPrefab.SetParent(this);
                base.Spawn(SkillPrefab, Owner); //must have ownership for the client authen in animator
                Debug.Log("Spawned SkillSlot Prefab: " + _playerController.LocalConnection + _playerController.IsServer);
            }
            AssignAnimators(SkillPrefab);
        } 
        
    }
    
    [ObserversRpc]
    public void AssignAnimators(NetworkObject sp)
    {
        if (sp == null && Skill.skillType == SkillType.Weaponary)
        {
            Debug.Log("Found no prefab");
            return;
        }

        if (Skill != null && Skill.skillType == SkillType.Weaponary)
        {
            //Debug.Log("Assign Animators");
            if (SkillPrefabAnimator == null)
            {
                //Debug.Log("took 1");
                SkillPrefabAnimator = sp.GetComponentInChildren<Animator>();
            }

            if (SkillPrefabNetworkAnimator == null)
            {
                //Debug.Log("took 2");
                SkillPrefabNetworkAnimator = SkillPrefabAnimator.GetComponent<NetworkAnimator>();
                SkillPrefabNetworkAnimator.SetAnimator(SkillPrefabAnimator);
            }
        }
    }

    //tell server to update the animator, somehow it worked
    public void PlayAnimTrigger()
    {
        Debug.Log("sync Trigger ? ");
        SkillPrefabNetworkAnimator.SetTrigger("Trigger");
    }

    //fix this part, it not change values from client to owner
    public void PlayAnimVar(FrameInput _frameInput, float Hor, float Ver)
    {
        SkillPrefabAnimator.SetBool("Toggle", InputStatus);
        SkillPrefabAnimator.SetFloat("Horizontal", Hor);
        SkillPrefabAnimator.SetFloat("Vertical", Ver);
        
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

    public void Activate(bool InputStatus)
    {
        if (Locked || currentCD > 0) return;
        if (Skill == null)
        {
            Debug.Log("none skill, yay");
            return;
        }

        this.InputStatus = InputStatus;
        Skill.Activate(_playerController, this);
        if(Skill.skillType != SkillType.Weaponary) currentCD = Skill.Cooldown;
    }
    
}
