using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using JetBrains.Annotations;
using QFSW.QC.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

public class HurtBox : NetworkBehaviour
{
    [FoldoutGroup("Stats")]
    public int Team;

    [FoldoutGroup("Stats")] 
    public bool Toggle = true;
    [FoldoutGroup("Stats")]
    public float Damage, StackDamage = 1, InitKBMultiplier = 5;
    [FoldoutGroup("Stats/Mode")] 
    public bool YAxisOnly, DownKB, KBOnly, PogoHit, ResetJumpCount;
    [FoldoutGroup("Stats/Advanced")] 
    public float PogoMultiplier = 1.25f;
    [FoldoutGroup("Stats/Advanced")] 
    public int BonusJumpCount = 1;
    [FoldoutGroup("Stats/Advanced")] 
    public float ExtraHorizontalVel = 0.3f;

    [FoldoutGroup("Debug and Setup")] 
    public PlayerController isOwner;
    [FoldoutGroup("Debug and Setup")]
    [CanBeNull] public Animator WeaponAnimator;

    private void Start()
    {
        isOwner = GetComponentInParent<PlayerController>();
        //Team = Owner.Team;
        if(WeaponAnimator == null) WeaponAnimator = GetComponentInParent<Animator>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void dealDamage(PlayerController playerController)
    {
        //if(Owner.Team == playerController.Team) return;
        
        Vector3 KBDirect = isOwner.transform.position - playerController.transform.position;
        KBDirect.x *= -1;
        KBDirect.x = KBDirect.x >= 0 ? 5 : -5;
        if (YAxisOnly)
        {
            KBDirect.x = 0;
            //Debug.Log("Y Only");
        }
        
        //force Y base on hit
        KBDirect.y = 2f;
        if (DownKB) KBDirect.y = -Mathf.Abs(KBDirect.y);
        else KBDirect.y = Mathf.Abs(KBDirect.y);
        
        Debug.DrawLine(playerController.transform.position, playerController.transform.position + KBDirect, Color.red, 2f);
        
        if(!KBOnly) playerController.ReceiveDamage(KBDirect, Damage);
        else playerController.ReceiveKB(KBDirect, InitKBMultiplier, Damage);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(!Toggle) return;
        
        //Debug.Log(other.gameObject.name);
        if (ResetJumpCount)
        {
            //Debug.Log("Reset");
            isOwner.ResetJumpCount(BonusJumpCount);
        }

        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Platform" || other.gameObject.tag == "Bullet")
        {
            if (PogoHit && other.gameObject != isOwner.gameObject)
            {
                isOwner.ApplyJump(PogoMultiplier, ExtraHorizontalVel);
                //Debug.Log("Extra Vel");
            }
            
            //Debug.Log("Play Particle here");

            var pcontroller = other.gameObject.GetComponent<PlayerController>();
            if(pcontroller != null) dealDamage(pcontroller);
        }
    }
    
    
}
