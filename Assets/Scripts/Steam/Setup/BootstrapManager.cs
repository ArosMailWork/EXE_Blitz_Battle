using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using NaughtyAttributes;
using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using SysRandom = System.Random;

public enum LobbyType
{
    Public ,FriendOnly, Private, PrivateUnique
}

public class BootstrapManager : NetworkBehaviour
{
    public static BootstrapManager Instance;
    private void Awake() => Instance = this;

    [FoldoutGroup("Lobby Settings")]
    public LobbyType LobbyTypeCreate;
    [FoldoutGroup("Lobby Settings")]
    public int LobbyCreateSlot = 4;
    [FoldoutGroup("Lobby Settings")]
    [SerializeField] private int CustomLobbyIDLength = 6;
    
    [HorizontalLine(color: EColor.Green)]
    [SerializeField] private bool SceneToggle = true;
    [SerializeField] private string menuName = "MenuSceneSteam";
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    public Callback<LobbyCreated_t> LobbyCreated;
    public Callback<GameLobbyJoinRequested_t> JoinRequest;
    public Callback<LobbyEnter_t> LobbyEntered;
    public Callback<LobbyMatchList_t> Callback_lobbyList;

    public static event Action OnGetLobbiesListCompleted;

    public static ulong CurrentLobbyID;
    public static string CurrentCustomLobbyID;
    
    public List<CSteamID> lobbyIDS = new List<CSteamID>();
    
    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        
    }
    
    
    public void GoToMenu()
    {
        if (!SceneToggle)
        {
            Debug.Log("First");
            Bootstrap_LoadScene(menuName);
        }
    }

    public static void Bootstrap_LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive); //this one work
    }

    public void CreateLobby()
    {
        
        Debug.Log("its worked!!");

        switch (LobbyTypeCreate)
        {
            case LobbyType.Public:
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, LobbyCreateSlot); 
                break;
            case LobbyType.FriendOnly:
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, LobbyCreateSlot);
                break;
            case LobbyType.Private:
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, LobbyCreateSlot);
                break;
            case LobbyType.PrivateUnique:
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivateUnique, LobbyCreateSlot);
                break;
            default:
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, LobbyCreateSlot); 
                break;
        }
    }

    
    public static string RandomCustomLobbyID(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        SysRandom random = new SysRandom();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public void GetLobbiesList(string customID)
    {
        if (lobbyIDS.Count > 0)
            lobbyIDS.Clear();

        SteamMatchmaking.AddRequestLobbyListStringFilter("SoraCustomID", customID, ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
        
        SteamMatchmaking.RequestLobbyList();
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        Debug.Log("Found " + result.m_nLobbiesMatching + " lobbies!");
        
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
            
            Debug.Log(lobbyID);
        }
        
        // Raise the event to notify any subscribers.
        OnGetLobbiesListCompleted?.Invoke();
    }
    private void CheckDuplicateCustomID(CSteamID SteamLobbyID)
    {
        //activate callback
        GetLobbiesList(CurrentCustomLobbyID);

        bool duplicateFound = false; // Flag to track if duplicate is found

        //start after callback
        OnGetLobbiesListCompleted += () =>
        {
            if (lobbyIDS.Count > 0)
            {
                // Duplicate custom ID found, generate a new one and check again
                CurrentCustomLobbyID = RandomCustomLobbyID(this.CustomLobbyIDLength);
                Debug.Log("Generated new CustomLobbyID: " + CurrentCustomLobbyID);
                CheckDuplicateCustomID(SteamLobbyID);
            }
            else if (!duplicateFound)
            {
                // No duplicate custom ID found, continue with lobby creation
                duplicateFound = true; // Set flag to true to prevent multiple executions
                SteamMatchmaking.SetLobbyData(SteamLobbyID, "SoraCustomID", CurrentCustomLobbyID);
                Debug.Log("Lobby creation was successful: " + CurrentCustomLobbyID + " " + SteamLobbyID);
                //_fishySteamworks.StartConnection(true);
            }
        };
    }
    
    
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        //Debug.Log("Starting lobby creation: " + callback.m_eResult.ToString());
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CSteamID SteamLobbyID = new CSteamID(CurrentLobbyID);
        string SteamID = SteamUser.GetSteamID().ToString();
        
        CurrentCustomLobbyID = RandomCustomLobbyID(this.CustomLobbyIDLength);

        Debug.Log("CustomLobbyID: " + CurrentCustomLobbyID);
        
        SteamMatchmaking.SetLobbyData(SteamLobbyID, "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(SteamLobbyID, "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
        SteamMatchmaking.SetLobbyData(SteamLobbyID, "SoraCustomID", CurrentCustomLobbyID);
        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        
        CheckDuplicateCustomID(SteamLobbyID);
    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        LobbyMenu.Instance.LeaveLobby();
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        CSteamID SteamLobbyID = new CSteamID(CurrentLobbyID);
        CurrentCustomLobbyID = SteamMatchmaking.GetLobbyData(SteamLobbyID, "SoraCustomID");
        string steamID = SteamMatchmaking.GetLobbyData(SteamLobbyID, "HostAddress");
        
        Debug.Log("Entered LobbyID: " + SteamLobbyID);
        Debug.Log("Connect to steamID: " + steamID);
        
        LobbyMenu.LobbyEntered(SteamMatchmaking.GetLobbyData(SteamLobbyID, "name"), _networkManager.IsServerStarted);
        _fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(SteamLobbyID, "HostAddress"));
        _fishySteamworks.StartConnection(false);
    }

    
    public void Join(string CustomID)
    {
        if (CustomID.Length > 15 && CustomID.All(char.IsDigit))
        {
            CSteamID steamID = new CSteamID(Convert.ToUInt64(CustomID));
            JoinByID(steamID);
        }
        else if(CustomID.Length == CustomLobbyIDLength)
            JoinByCustomID(CustomID.ToString());
    }
    void JoinByID(CSteamID steamID)
    {
        Debug.Log("Attempting to join lobby with ID: " + steamID.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(steamID))
        {
            LeaveLobby();
            SteamMatchmaking.JoinLobby(steamID);
            Debug.Log("SteamID Join Side Successed");
        }
        else
            Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);
    }
    void JoinByCustomID(string customID)
    {
        // First, we need to get the list of lobbies using the custom ID.
        GetLobbiesList(customID);

        // Once the callback for `GetLobbiesList` is triggered, we can proceed with joining the lobby.
        OnGetLobbiesListCompleted += () =>
        {
            // Check if any lobbies were found.
            if (lobbyIDS.Count > 0)
            {
                // Select the first lobby from the list.
                CSteamID selectedLobby = lobbyIDS[0];

                // Request data for the selected lobby.
                if (SteamMatchmaking.RequestLobbyData(selectedLobby))
                {
                    // Join the selected lobby.
                    JoinByID(selectedLobby);
                    Debug.Log("CustomID Join Succeeded");
                }
                else
                {
                    Debug.Log("Failed to join lobby with CustomID: " + selectedLobby.m_SteamID);
                }
            }
            else
            {
                Debug.Log("No lobbies found with the specified CustomID: " + customID);
            }
        };
    }
    
    
    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;
        CurrentCustomLobbyID = "";

        LobbyMenu.LobbyEntered("Lobby Name", false);

        Instance._fishySteamworks.StopConnection(false);
        if(Instance._networkManager.IsServerStarted)
            Instance._fishySteamworks.StopConnection(true);
    }
}