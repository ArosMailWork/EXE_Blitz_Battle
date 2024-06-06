using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

public class Platform : NetworkBehaviour
{
    private NetworkAnimator networkAnimator;
    public readonly SyncVar<int> HealthSync = new SyncVar<int>(new SyncTypeSettings(1f));
    public int currentHealth = 10;
    public int Health = 10;
    public readonly SyncVar<float> currentLifeTimeSync = new SyncVar<float>(new SyncTypeSettings(1f));
    public float currentLifeTime;
    public float LifeTime = 2;
    
    private void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
        currentLifeTimeSync.OnChange += on_lifeTimer;
        HealthSync.OnChange += on_heath;
    }

    public void SetDefault()
    {
        SetLifeTime(LifeTime);
        SetHealth(Health);
    }

    private void on_lifeTimer(float prev, float next, bool asServer)
    {
        currentLifeTime = next;
        if(next <= 0) DespawnObj();
    }
    private void on_heath(int prev, int next, bool asServer)
    {
        currentHealth = next;
        if(next <= 0) DespawnObj();
    }
    
    private void FixedUpdate()
    {
        if (currentLifeTime > 0)
        {
            //Debug.Log(currentLifeTimeSync.Value);
            currentLifeTime -= Time.fixedDeltaTime;
            SetLifeTime(currentLifeTime);
        }
    }

    [Button]
    [ServerRpc(RequireOwnership = false)]
    public void SetLifeTime(float time)
    {
        currentLifeTimeSync.Value = time;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealth(int health)
    {
        HealthSync.Value = health;
    }

    void DespawnObj()
    {
        networkAnimator.SetTrigger("Trigger");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void doDespawnObj()
    {
        InstanceFinder.ServerManager.Despawn(NetworkObject);
    }
}
