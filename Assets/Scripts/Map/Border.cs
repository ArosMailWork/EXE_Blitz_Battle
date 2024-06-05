using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Border : MonoBehaviour
{
    public Transform[] SpawnPoints;

    private void Awake()
    {
        SpawnPoints = CustomPlayerSpawner.Instance.spawnPoints; 
    }

    private void TPRandomPlace(PlayerController playerController)
    {
        int index = Random.Range(0, SpawnPoints.Length);
        playerController.Teleport(SpawnPoints[index].transform.position);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            TPRandomPlace(player);
            player.PlayerLifeAmountUpdate();
        }
    }
}
