using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Spawning;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UIElements.Toggle;

public class LobbyMenu : MonoBehaviour
{
    public static LobbyMenu instance;

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

    private void Awake() => instance = this;
    private void Start()
    {
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
        if(instance.lobbyTitle != null) instance.lobbyTitle.text = lobbyName;
        if(instance.lobbyTitle2 != null) instance.lobbyTitle2.text = lobbyName;
        instance.startGameButton.gameObject.SetActive(isHost);
        instance.leaveButton.gameObject.SetActive(!isHost);
        if(instance.lobbySteamID != null) instance.lobbySteamID.text = BootstrapManager.CurrentLobbyID.ToString();
        instance.CustomlobbyID.text = BootstrapManager.CurrentCustomLobbyID.ToString();
        //instance.OpenLobby();
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
    public void StartGame(string sceneName)
    {
        string[] scenesToClose = new string[] { "Menu" };
        BootstrapNetworkManager.ChangeNetworkScene(sceneName, scenesToClose);
    }
}
