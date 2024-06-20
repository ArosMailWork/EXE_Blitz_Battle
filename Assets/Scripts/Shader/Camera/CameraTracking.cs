using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct TrackingObject
{
    public int playerID;
    public bool Tracking;
    public GameObject obj;
}

public class CameraTracking : MonoBehaviour
{
    public FocusLevel focusLevel;
    public List<TrackingObject> players;
    
    public float DepthAdjustSpeed = 5f;
    public float AngleAdjustSpeed = 7f;
    public float PositionAdjustSpeed = 5f;

    public float DepthMax = -10f;
    public float DepthMin = -22f;
    
    public float AngleMax = 11f;
    public float AngleMin = 3f;

    private float CameraEulerX;
    private Vector3 cameraPosition;
    private float smoothVelocityX, smoothVelocityY, smoothVelocityZ;

    public static CameraTracking Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddObj(PlayerController playerAdd)
    {
        if (players.Any(player => player.obj == playerAdd))
        {
            Debug.Log("Player already exists in the tracking list.");
            return;
        }
        
        TrackingObject addObj = new TrackingObject();
        addObj.Tracking = true;
        addObj.obj = playerAdd.gameObject;
        
        if(!playerAdd.DummyMode) addObj.playerID = playerAdd.PlayerID;
        else addObj.playerID = -2;
        players.Add(addObj);
    }

    void LateUpdate()
    {
        if (players.Count <= 0)
        {
            //this.enabled = false;
            return;
        }
        CalculateCameraLocation();
        MoveCamera();
    }
    void MoveCamera()
    {
        Vector3 pos = gameObject.transform.position;
        if (pos != cameraPosition)
        {
            Vector3 targetPos = Vector3.zero;
            targetPos.x = Mathf.SmoothDamp(pos.x, cameraPosition.x, ref smoothVelocityX, PositionAdjustSpeed);
            targetPos.y = Mathf.SmoothDamp(pos.y, cameraPosition.y, ref smoothVelocityY, AngleAdjustSpeed);
            targetPos.z = Mathf.SmoothDamp(pos.z, cameraPosition.z, ref smoothVelocityZ, DepthAdjustSpeed);
            gameObject.transform.position = targetPos;
        }

        Vector3 localEulerAngles = gameObject.transform.localEulerAngles;
        if (localEulerAngles.x != CameraEulerX)
        {
            Vector3 targetEulerAngles = new Vector3(CameraEulerX, localEulerAngles.y, localEulerAngles.z);
            gameObject.transform.localEulerAngles = 
                Vector3.MoveTowards(localEulerAngles, targetEulerAngles, AngleAdjustSpeed * Time.deltaTime);
        }
    }
    void CalculateCameraLocation()
    {
        Vector3 avgCenter = Vector3.zero;
        Vector3 totalPos = Vector3.zero;
        Bounds playerBounds = new Bounds();

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].obj == null || !players[i].Tracking) continue;
            Vector3 playerPos = players[i].obj.transform.position;
            if (!focusLevel.FocusBounds.Contains(playerPos))
            {
                float playerX = Mathf.Clamp(playerPos.x, focusLevel.FocusBounds.min.x, focusLevel.FocusBounds.max.x);
                float playerY = Mathf.Clamp(playerPos.y, focusLevel.FocusBounds.min.y, focusLevel.FocusBounds.max.y);
                float playerZ = Mathf.Clamp(playerPos.z, focusLevel.FocusBounds.min.z, focusLevel.FocusBounds.max.z);
                playerPos = new Vector3(playerX, playerY, playerZ);
            }

            totalPos += playerPos;
            playerBounds.Encapsulate(playerPos);
        }

        avgCenter = totalPos / players.Count;

        float extents = playerBounds.extents.x + playerBounds.extents.y;
        float lerpPercent = Mathf.InverseLerp(0, (focusLevel.HalfXBounds + focusLevel.HalfYBounds) / 2, extents);

        float depth = Mathf.Lerp(DepthMax, DepthMin, lerpPercent);
        float angle = Mathf.Lerp(AngleMax, AngleMin, lerpPercent);

        CameraEulerX = angle;
        cameraPosition = new Vector3(avgCenter.x, avgCenter.y, depth);
    }

    #region Ults

    public async UniTaskVoid StopTrack(int playerID, float timeDelay = -1)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerID == playerID)
            {
                var updatedPlayer = players[i];
                updatedPlayer.Tracking = false;
                players[i] = updatedPlayer;
                if (timeDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(timeDelay));
                    updatedPlayer.Tracking = true;
                    players[i] = updatedPlayer;
                }
            }
        }
    }
    
    public void ChangeTrackState(int playerID, bool toggle)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerID == playerID)
            {
                var updatedPlayer = players[i];
                updatedPlayer.Tracking = toggle;
                players[i] = updatedPlayer;
                
                Debug.Log("Player " + playerID + " Tracking State now is " + toggle);
                return; // Exit the loop after updating the Tracking state
            }
        }
    }

    #endregion
}
