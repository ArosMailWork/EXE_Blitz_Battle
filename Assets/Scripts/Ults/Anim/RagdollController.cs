using System;
using System.Collections;
using System.Collections.Generic;
using DitzelGames.FastIK;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class RagdollController : MonoBehaviour
{
    private Animator targetAnimator;

    public GameObject parent;
    public Collider[] ragdoll_cols;
    public Rigidbody[] ragdoll_rbs;
    public CharacterJoint[] ragdoll_joint;
    public Transform[] reset_transform;
    [CanBeNull] public Rigidbody rb_freeze;

    [SerializeField] private bool active;
    [SerializeField] private bool Debug_toggle;
    [SerializeField] private bool reset_Pos = true;
    
    private bool previousToggleValue;
    private FastIKFabric IK_object;

    void GetRagdollParts()
    {
        ragdoll_cols = GetComponentsInChildren<Collider>();
        ragdoll_rbs = GetComponentsInChildren<Rigidbody>();
        ragdoll_joint = GetComponentsInChildren<CharacterJoint>();
        IK_object = GetComponentInChildren<FastIKFabric>();
    }

    void Awake()
    {
        parent = this.transform.parent.gameObject;
        this.transform.SetParent(parent.transform);
        targetAnimator = GetComponent<Animator>();
        GetRagdollParts();
    }

    private void Start()
    {
        RagdollToggle(false);
    }

    private void Update()
    {
        if(!active) return;
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug_toggle = !Debug_toggle;
        }
        
        if (Debug_toggle != previousToggleValue)
        {
            RagdollToggle(Debug_toggle);
            previousToggleValue = Debug_toggle;
        }
    }

    void RagdollToggle(bool toggle)
    {
        if (toggle)
        {
            IK_object.enabled = false;
            targetAnimator.enabled = false;
            
            foreach (var joint in ragdoll_joint)
            {
                joint.enableCollision = true;
            }
            foreach (var rb in ragdoll_rbs)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            foreach (var col in ragdoll_cols)
            {
                col.enabled = true;
            }
            
            this.transform.SetParent(null);
            
            if(rb_freeze != null) Invoke(nameof(AutoFreeze_rb), 0.5f);
        }
        else
        {
            foreach (var joint in ragdoll_joint)
            {
                joint.enableCollision = false;
            }
            foreach (var rb in ragdoll_rbs)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
            foreach (var col in ragdoll_cols)
            {
                col.enabled = false;
            }

            foreach (var trans in reset_transform)
            {
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
            }

            this.transform.SetParent(parent.transform);
            
            //idk, this seem fun thats why I keep it, lol
            if (reset_Pos)
            {
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
            }
            
            IK_object.enabled = true;
            targetAnimator.enabled = true;
            //main_cols.enabled = false;
        }
    }

    void AutoFreeze_rb()
    {
        rb_freeze.isKinematic = true;
    }
}
