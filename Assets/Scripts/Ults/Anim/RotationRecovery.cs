using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RotationRecovery : MonoBehaviour
{
    #region Variables

        [Header("Status")]
        public bool active = true;
        public bool Debug_enable = false;
        
        [Space(10)]
        [Header("Variables")]
        
        [Range(0,90)] public float impactRot = 15f;
        [Range(0,90)] public float maxRot = 60f;
        public float recoverSpeed = 0.5f;

        [Space(5)]
        [Header("Assign object")]
        
        public Transform DebugDirect_obj; //gen direct Vector
    
        private float accumulatedRotation = 0f;

    #endregion

    void Update()
    {
        //using input and generated vector to apply direction force
        if (Debug_enable) DebugInput();
        
        RecoverRot(Quaternion.Euler(Vector3.zero));
    }

    #region Calculations
    
    private Vector3 direction, rotationAxis;
    private Quaternion rotation_add, newRotation, rotDifference;
    private Quaternion Recover_rot;
    private float angle;
    
    public void RotateGameObjectByVector(Vector3 incomingVector)
    {
        #region Rotate Calculate
        
        // Get the rotation axis base on object
        // (the spine bone using x axis as the vector down -> left side is the upper direction in world pos)
        direction = new Vector3(incomingVector.x, 0, incomingVector.z).normalized;
        rotationAxis = Vector3.Cross(direction, -transform.right);

        // Calculate the rotation quaternion based on the axis and rotation amount
        rotation_add = Quaternion.AngleAxis(impactRot, rotationAxis);
        
        // Apply the rotation to the game object
        transform.rotation = rotation_add * transform.rotation;
        
        #endregion

        #region Limit Rotation
        // Calculate new rotation
        newRotation = rotation_add * transform.rotation;
        // Get the difference from current rotation to new rotation
        rotDifference = newRotation * Quaternion.Inverse(transform.rotation);
        
        // Convert the rotation difference to an angle
        angle = rotDifference.eulerAngles.magnitude;

        // If adding the rotation would exceed 90 degrees, limit it to 90 degrees
        if (accumulatedRotation + angle > maxRot)
            angle = maxRot - accumulatedRotation;
        
        #endregion
    }
    
    void RecoverRot(Quaternion recoverRotation)
    {
        // Slerp to Recover
        Recover_rot = Quaternion.Slerp(transform.localRotation, recoverRotation, recoverSpeed * Time.deltaTime);

        // Apply the new rotation to the GameObject
        transform.localRotation = Recover_rot;
    }
    #endregion
    
    #region Debug and Test

        private Vector3 debug_direction;
        private Vector3 GenVec;
        
        void DebugDirect()
        {
            DebugExtension.Draw2DArrowGizmo(DebugDirect_obj.position, transform.position, false);
        }
        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                GenVec = DebugVectorGen(DebugDirect_obj);
                RotateGameObjectByVector(GenVec);
            }
        }
            
        //Gen Vector (from DirectPos go direct to the transform)
        Vector3 DebugVectorGen(Transform DirectPos)
        {
            debug_direction = (DirectPos.position - transform.position).normalized;
            return new Vector3(debug_direction.x, 0, debug_direction.z);
        }

        void OnDrawGizmos()
        {
            if(Debug_enable) DebugDirect();
        }

    #endregion

}
