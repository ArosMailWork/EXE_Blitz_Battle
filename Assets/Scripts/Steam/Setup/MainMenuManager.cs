using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Spawning;
using JetBrains.Annotations;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    //[SerializeField, CanBeNull] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField, CanBeNull] private TextMeshProUGUI lobbyTitle, lobbyTitle2;
    
    [SerializeField, CanBeNull] private TextMeshProUGUI lobbySteamID, CustomLobbyID;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveButton;
    
    //temp
    [SerializeField] private List<Transform> SpawnList;

    private void Awake() => Instance = this;
    private void Start()
    {
        Instance = this;
        //OpenMainMenu();
    }
    
    
    public void CreateLobby()
    {
        BootstrapManager.Instance.CreateLobby();
    }
    public void OpenMainMenu()
    {
        //CloseAllScreens();
        //menuScreen.SetActive(true);
    }
    public void OpenLobby()
    {
        //CloseAllScreens();
        //lobbyScreen.SetActive(true);
    }

    
    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        Debug.Log(Instance.startGameButton.gameObject);
        
        //if (Instance.lobbyTitle != null) Instance.lobbyTitle.text = lobbyName;
        //if (Instance.lobbyTitle2 != null) Instance.lobbyTitle2.text = lobbyName;
        Instance.startGameButton.gameObject.SetActive(isHost);
        Instance.leaveButton.gameObject.SetActive(!isHost);
        if (Instance.lobbySteamID != null) Instance.lobbySteamID.text = BootstrapManager.CurrentLobbyID.ToString();
        if (Instance.CustomLobbyID != null) Instance.CustomLobbyID.text = BootstrapManager.CurrentCustomLobbyID.ToString();
        Instance.OpenLobby();
    }
    
    void CloseAllScreens()
    {
        //menuScreen.SetActive(false);
        //lobbyScreen.SetActive(false);
    }

    
    public void JoinLobby()
    {
        BootstrapManager.LeaveLobby();
        BootstrapManager.Instance.Join(lobbyInput.text);
    }
    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainMenu();
    }
    
    public void StartGame()
    {
        string[] scenesToClose = new string[] { "Menu" };
        BootstrapNetworkManager.ChangeNetworkScene("Practice", scenesToClose);
    }
    
    
    public void CopyToClipboard(TextMeshProUGUI CopyUI)
    {
        GUIUtility.systemCopyBuffer = CopyUI.text;
        Debug.Log("Copied '" + CopyUI.text + "' to clipboard.");
    }
    public void PasteFromClipboard(TMP_InputField CopyUI)
    {
        CopyUI.text = GUIUtility.systemCopyBuffer;
        Debug.Log("Pasted '" + CopyUI.text + "' from clipboard.");
    }
}
