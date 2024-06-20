using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Border : MonoBehaviour
{
    public List<Transform> SpawnPoints;
    public float ReviveDelay = 3f;

    private void Start()
    {
        SpawnPoints = CustomPlayerSpawner.Instance.spawnPoints.ToList(); 
    }

    private async UniTaskVoid TPRandomPlace(PlayerController playerController, float delayTime = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        int index = Random.Range(0, SpawnPoints.Count);
        playerController.Teleport(SpawnPoints[index].transform.position);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.PlayerLifeAmountUpdate();

            if (ScoreManager.Instance.PlayerLifeRemain(player.PlayerID))
            {
                CameraTracking.Instance.StopTrack(player.PlayerID, ReviveDelay);
                TPRandomPlace(player, ReviveDelay);
            }
            else
            {
                CameraTracking.Instance.ChangeTrackState(player.PlayerID, false);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Player")
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if(!player.DummyMode) player.PlayerLifeAmountUpdate();

            if (ScoreManager.Instance.PlayerLifeRemain(player.PlayerID))
            {
                CameraTracking.Instance.StopTrack(player.PlayerID, ReviveDelay);
                TPRandomPlace(player, ReviveDelay);
                //fill hp
            }
            else
            {
                CameraTracking.Instance.ChangeTrackState(player.PlayerID, false);
            }
        }
    }
}
