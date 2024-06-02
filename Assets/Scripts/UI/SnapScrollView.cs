using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SnapScrollView : NetworkBehaviour
{
    public bool isLocked;
    public ScrollRect _scrollRect;
    public RectTransform contentPanel;
    public RectTransform sampleListItem;

    public HorizontalLayoutGroup HLG;
    public readonly SyncVar<int> currentItemSync = new SyncVar<int>();
    [ReadOnly] public int currentItem; //for calculate local and Debug
    public bool isSnapped;

    public float snapForce;
    public float moveDetect = 50;
    [CanBeNull] public TextMeshProUGUI LabelText;
    public string[] ItemNames;
    public RectTransform[] ItemList;
    RectTransform ViewPortRectTransform;

    private bool isUpdated;
    private float snapSpeed;
    private Vector2 OldVelocity;
    public bool isOwner;
    [ReadOnly] public LoadoutSelector LSelector;

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        isOwner = base.IsOwner;
    }
    

    private void Start()
    {
        isSnapped = false;
        isUpdated = false;
        LabelText.text = "not loading, sad";
    }

    private void LateUpdate()
    {
        if (!isOwner || _scrollRect.velocity.magnitude <= 3 && isLocked)
        {
            _scrollRect.horizontal = false;
        }
        
        if (isOwner)
        {
            _scrollRect.horizontal = true;
            SyncItemValueServer();
        }
        SnapUI();
    }

    void SyncItemValueServer() //check first XD
    {
        if (isLocked) return;
        
        //Calculate for Snap
        currentItem = Mathf.RoundToInt(-contentPanel.localPosition.x / (sampleListItem.rect.width + HLG.spacing));
        if (currentItem <= -1) currentItem = 0;
        if (currentItem > ItemList.Length) currentItem = ItemList.Length;
        
        UpdateSyncValue(currentItem);
    }
    
    [ServerRpc] //This logic only work in Server Device, so it will take Scene of Server as data so make sure to input stuffs
    void UpdateSyncValue(int updateValue)
    {
        currentItemSync.Value = updateValue;
        UpdateLabelServer(this);
        
    }

    [ObserversRpc(BufferLast = true)]
    void UpdateLabelServer(SnapScrollView ssv) //Update by Sync Value only
    {
        //Label Update
        if(currentItemSync.Value <= ItemNames.Length - 1 && currentItemSync.Value >= 0)
            ssv.LabelText.text = ssv.ItemNames[ssv.currentItemSync.Value];
        else
            ssv.LabelText.text = ":D Indev";
    }
    
    void SnapUI() //Just local Snap, lol
    {
        //do the snap if swipe slow
        if (_scrollRect.velocity.magnitude < moveDetect && !isSnapped)
        {
            snapSpeed += snapForce * Time.deltaTime;
            contentPanel.localPosition = new Vector3(Mathf.MoveTowards(contentPanel.localPosition.x, 
                    0 - (currentItemSync.Value) * 
                    (sampleListItem.rect.width + HLG.spacing),snapSpeed), 
                    contentPanel.localPosition.y,
                    contentPanel.localPosition.z);
            isSnapped = (contentPanel.localPosition.x == 0 - (currentItemSync.Value * sampleListItem.rect.width + HLG.spacing));
        }
        else
        {
            isSnapped = false;
            snapSpeed = 0;
        }
    }
}
