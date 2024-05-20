using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosFix : MonoBehaviour
{
    public bool TogglePos = true, ToggleRot, EnableMode, StartMode = true;
    public bool WorldAxisMode;
    public Vector3 SetPos, SetRot;
    void Start()
    {
        if(!StartMode) return;
        if (TogglePos)
        {
            if(!WorldAxisMode) this.transform.localPosition = SetPos;
        }

        if (ToggleRot)
        {
            if(!WorldAxisMode) this.transform.localRotation = Quaternion.Euler(SetRot);
        }
    }

    private void OnEnable()
    {
        if(!EnableMode) return;
        if (TogglePos)
        {
            if(!WorldAxisMode) this.transform.localPosition = SetPos;
        }

        if (ToggleRot)
        {
            if(!WorldAxisMode) this.transform.localRotation = Quaternion.Euler(SetRot);
        }
    }
}
