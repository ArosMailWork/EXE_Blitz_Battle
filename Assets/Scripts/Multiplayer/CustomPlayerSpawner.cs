using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class CustomPlayerSpawner : MonoBehaviour
{
    public bool _Toggle;
    private int _nextSpawn;
    private NetworkManager _networkManager;
    public event Action<NetworkObject> OnSpawned;
    
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;

    public string SpawnerTag;
    
    public Transform[] spawnPoints;
    public string sceneName;
    public bool _addToDefaultScene;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        
        // Add this event in order to load the scene
        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;

        // Add this event so we can spawn the player
        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    
    public void FindSpawns()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag(SpawnerTag);
        spawnPoints = new Transform[spawners.Length];

        for(int i=0;i<spawners.Length;i++)
        {
            spawnPoints[i] = spawners[i].transform;
        }
    }

    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        Debug.Log(conn);
        if (!asServer || !_Toggle)
            return;
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }
        
        FindSpawns();

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        _networkManager.ServerManager.Spawn(nob, conn);

        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        OnSpawned?.Invoke(nob);
    }
    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        // When server starts load online scene as global. Since this is a global scene clients will automatically join it when connecting.
        if (obj.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Server Connection state = Started");
            
            // Now load the global scene where the game is player. Instead of a fixed name you can also retrieve the
            // name of the game scene from the lobby meta data, making it more dynamic
            
            
            Debug.Log("Loading scene: "+sceneName);
            SceneLoadData sld = new SceneLoadData(sceneName);
            sld.ReplaceScenes = ReplaceOption.All;
            
            Debug.Log("Now load the global map");
            _networkManager.SceneManager.LoadGlobalScenes(sld);
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
}
