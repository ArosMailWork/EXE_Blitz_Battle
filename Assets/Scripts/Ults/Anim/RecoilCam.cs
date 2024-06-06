using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

public class RecoilCam : MonoBehaviour
{
    //Rotations
    Vector3 defaultRot;
    Vector3 currentRot;
    Vector3 targetRot;

    //Recoil (Hipfire)
    [SerializeField] float recoilX = -4f;
    [SerializeField] float recoilY = 4f;
    [SerializeField] float recoilZ = 0.7f;
    [SerializeField] float recoilScale = 1;
    
    //Settings
    float snappiness = 6;
    [SerializeField] float snappiness_Hipfire = 6;
    [SerializeField] float returnSpeed = 2.5f;

    private float elapsedTime = 0f;
    public static RecoilCam Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        defaultRot = this.transform.localRotation.eulerAngles;
    }

    void Update()
    {
        /*
            //simplier, but more painful to calculate
            if(targetRot != defaultRot) 
            targetRot = Vector3.Lerp(targetRot, defaultRot, returnSpeed * Time.deltaTime); 
            if(currentRot != targetRot) 
            currentRot = Vector3.Slerp(currentRot, targetRot, snappiness * Time.fixedDeltaTime); 
            this.transform.localRotation = Quaternion.Euler(currentRot); 
         */
        
        //return base on second
        if (targetRot != defaultRot)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / (returnSpeed));
            targetRot = Vector3.Lerp(targetRot, defaultRot, t);
        }

        if (currentRot != targetRot)
        {
            currentRot = Vector3.Slerp(currentRot, targetRot, snappiness * Time.fixedDeltaTime);
            this.transform.localRotation = Quaternion.Euler(currentRot);
        }

        this.transform.localRotation = Quaternion.Euler(currentRot);
    }

    public void SetRecoilStats(RecoilData recoilStats)
    {
        if(recoilStats == null) return;
        
        recoilX = recoilStats.RecoilX;
        recoilY = recoilStats.RecoilY;
        recoilZ = recoilStats.RecoilZ;
        recoilScale = recoilStats.RecoilScale;
        snappiness_Hipfire = recoilStats.Snappiness_Hipfire;
        returnSpeed = recoilStats.FireRate;
    }

    public void RecoilFire()
    {
        snappiness = snappiness_Hipfire;
        targetRot += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ)) * recoilScale;
        elapsedTime = 0f;
    }
    
    [Button("Recoil Fire")]
    public void ActivateRecoilFire()
    {
        RecoilFire();
    }
}
