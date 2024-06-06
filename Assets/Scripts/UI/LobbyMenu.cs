using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Spawning;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UIElements.Toggle;

public class LobbyMenu : MonoBehaviour
{
    public static LobbyMenu Instance;

    //[SerializeField] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField, CanBeNull] private TextMeshProUGUI lobbyTitle, lobbyTitle2;
    
    [SerializeField, CanBeNull] private TextMeshProUGUI lobbySteamID, CustomlobbyID;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private List<Toggle> readyToggles;
    
    //temp
    [SerializeField] private List<Transform> SpawnList;
    private PlayerSpawner _playerSpawner;

    private void Awake() => Instance = this;
    private void Start()
    {
        //OpenMainMenu();
    }
    
    
    public void CreateLobby()
    {
        Invoke(nameof(doCreateLobby), 0.2f);
    }
    
    public void doCreateLobby()
    {
        BootstrapManager.Instance.CreateLobby();
        startGameButton.enabled = true;
        leaveButton.enabled = false;
    }

    
    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        if(Instance.lobbyTitle != null) Instance.lobbyTitle.text = lobbyName;
        if(Instance.lobbyTitle2 != null) Instance.lobbyTitle2.text = lobbyName;
        Instance.startGameButton.gameObject.SetActive(isHost);
        Instance.leaveButton.gameObject.SetActive(!isHost);
        if(Instance.lobbySteamID != null) Instance.lobbySteamID.text = BootstrapManager.CurrentLobbyID.ToString();
        Instance.CustomlobbyID.text = BootstrapManager.CurrentCustomLobbyID.ToString();
    }

    
    public void JoinLobby()
    {
        BootstrapManager.Instance.Join(lobbyInput.text);
        startGameButton.enabled = false;
        leaveButton.enabled = true;
    }
    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
    }
    public void StartGame(string sceneName)
    {
        string[] scenesToClose = new string[] { "Menu" };
        BootstrapNetworkManager.ChangeNetworkScene(sceneName, scenesToClose);
    }
}
