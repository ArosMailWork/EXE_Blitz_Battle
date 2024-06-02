using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClientLoad : MonoBehaviour
{
    public static TestClientLoad Instance;
    public List<ISkill> pickedSkills;
    public bool InstanceDebug;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetInstance()
    {
        Instance = this;
        InstanceDebug = true;
    }
}
