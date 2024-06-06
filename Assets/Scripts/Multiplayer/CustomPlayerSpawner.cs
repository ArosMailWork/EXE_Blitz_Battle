using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using UnityEngine;
using WebSocketSharp;

public class CustomPlayerSpawner : NetworkBehaviour
{
    public bool _Toggle;
    private int _nextSpawn;
    private NetworkManager _networkManager;
    public event Action<NetworkObject> OnSpawned;
    public List<NetworkConnection> connList;
    
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField] private NetworkObject _playerPrefab;
    //[SerializeField] private NetworkObject _dummyPrefab;
    //public int DummyAmount = 1; 
    public int setMaxLife = 3; 

    public string SpawnerTag;
    
    public Transform[] spawnPoints;
    public string sceneName;
    public bool _addToDefaultScene;
    public static CustomPlayerSpawner Instance;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if(_networkManager == null) return;

        // Add this event so we can spawn when at scene have update in connection 
        _networkManager.SceneManager.OnClientPresenceChangeEnd += SceneManager_OnClientPresenceChangeEnd;
    }

    private void OnDisable()
    {
        //InstanceFinder.ClientManager.UnregisterBroadcast<Data>(OnSpawnBroadcast);
        //InstanceFinder.ServerManager.UnregisterBroadcast<Data>(OnClientSpawnBroadcast);
    }
    
    private void Start()
    {
        //connList = _networkManager.ClientManager.Clients.Values.ToList();
        Instance = this;
        
        InstanceFinder.ClientManager.RegisterBroadcast<Data>(OnSpawnBroadcast);
        InstanceFinder.ServerManager.RegisterBroadcast<Data>(OnClientSpawnBroadcast);
    }

    private NetworkConnection spawnConn;
    void SceneManager_OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs changeEvent)
    {
        spawnConn = changeEvent.Connection;
        //Debug.Log("this go second: " + spawnConn);
        ClientEndBroadcastSpawn(spawnConn);
    }

    #region Spawn
    
    public void FindSpawns()
    {
        if (!SpawnerTag.IsNullOrEmpty())
        {
            GameObject[] spawners = GameObject.FindGameObjectsWithTag(SpawnerTag);
            spawnPoints = new Transform[spawners.Length];

            for(int i=0;i<spawners.Length;i++)
            {
                spawnPoints[i] = spawners[i].transform;
            }
        }
    }
    private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        //No spawns specified.
        if (spawnPoints.Length == 0)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
            return;
        }

        Transform result = spawnPoints[_nextSpawn];
        if (result == null)
        {
            SetSpawnUsingPrefab(prefab, out pos, out rot);
        }
        else
        {
            pos = result.position;
            rot = result.rotation;
        }

        //Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= spawnPoints.Length)
            _nextSpawn = 0;
    }
    private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        pos = prefab.position;
        rot = prefab.rotation;
    }

    public void SpawnPlayer(NetworkConnection conn, bool added)
    {
        doSpawnPlayer(conn, added);
    }
    
    void doSpawnPlayer(NetworkConnection conn, bool added)
    {
        if (!_Toggle || added) return; //not added and spawner turn On only
        //Debug.Log("Try Spawn: " + conn);
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        
        FindSpawns();

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);
        
        //Debug.Log("Scene CPCS joined: " + conn);

        //NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        NetworkObject nob = Instantiate(_playerPrefab, position, rotation);
        base.Spawn(nob, conn);
        
        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);
        
        CameraTracking.Instance.AddObj(nob.gameObject);
        //GameManager.Instance.ScoreManager.SetStartLife(nob.GetComponent<PlayerController>(), setMaxLife);
    }
    
    #endregion

    #region Broadcast
    
    public struct Data : IBroadcast
    {
        public NetworkConnection conn;
        public int tIndex;
    }
    
    //Send shiet to server again
    private void OnClientSpawnBroadcast(NetworkConnection networkConnection, Data data, Channel channel)
    {
        //Debug.Log("Through Client");
        InstanceFinder.ServerManager.Broadcast(data);
    }
    
    //Receive
    private void OnSpawnBroadcast(Data data, Channel channel)
    {
        if(!Instance.IsServer) return;
        //Debug.Log("Server Broadcast: " + data.conn);
        SpawnPlayer(data.conn, false);
    }

    public void ClientEndBroadcastSpawn(NetworkConnection connInput)
    {
        var data = new Data() { conn = connInput };
        //Debug.Log("Seem like it worked, nice " + data.conn + InstanceFinder.IsServer);
        
        if (InstanceFinder.IsServerStarted)
            InstanceFinder.ServerManager.Broadcast(data);
        else if (InstanceFinder.IsClientStarted)
            InstanceFinder.ClientManager.Broadcast(data);
    }
    
    #endregion
}
