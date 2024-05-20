using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    public FocusLevel focusLevel;
    public List<GameObject> players;
    
    public float DepthAdjustSpeed = 5f;
    public float AngleAdjustSpeed = 7f;
    public float PositionAdjustSpeed = 5f;

    public float DepthMax = -10f;
    public float DepthMin = -22f;
    
    public float AngleMax = 11f;
    public float AngleMin = 3f;

    private float CameraEulerX;
    private Vector3 cameraPosition;

    public static CameraTracking Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddObj(GameObject playerAdd)
    {
        players.Add(playerAdd);
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

    private float smoothVelocityX, smoothVelocityY, smoothVelocityZ;
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
            if(players[i] == null) continue;
            Vector3 playerPos = players[i].transform.position;
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
}
