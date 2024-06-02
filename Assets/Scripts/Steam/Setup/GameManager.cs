using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Spawning;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
