using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ClientLoadoutSave : NetworkBehaviour
{
    public static ClientLoadoutSave Instance;
    public List<ISkill> pickedSkills;
    public bool InstanceDebug;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner) SetInstance();
    }

    public void SetInstance()
    {
        Instance = this;
        InstanceDebug = true;
    }
}
