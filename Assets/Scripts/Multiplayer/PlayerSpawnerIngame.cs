using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerSpawnerIngame : MonoBehaviour
{
    #region Public.
    
        public event Action<NetworkObject> OnSpawned;
        #endregion

        #region Serialized.

        [Tooltip("Prefab to spawn for the player.")]
        public bool _toggle;
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private bool _addToDefaultScene = true;
        public List<Transform> Spawns;
        public List<NetworkConnection> connList;
        
        public string SpawnerTag;

        #endregion

        #region Private.
        private NetworkManager _networkManager;
        private int _nextSpawn;

        public static PlayerSpawnerIngame Instance;
        #endregion

        private void Start()
        {
            InitializeOnce();
            connList = _networkManager.ClientManager.Clients.Values.ToList();
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        private void InitializeOnce()
        {
            _networkManager = InstanceFinder.NetworkManager;
            Instance = this;
            if (_networkManager == null)
            {
                Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }
            
            _networkManager.SceneManager.OnClientPresenceChangeEnd += SceneManager_OnClientPresenceChangeEnd;
            
        }


        void SceneManager_OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs clientPresenceChangeEvent)
        {
            SpawnPlayer(clientPresenceChangeEvent.Connection, clientPresenceChangeEvent.Added);
        }

    
        public void SpawnPlayer(NetworkConnection conn, bool added)
        {
            if(!_toggle || added) return;
            
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

        private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            //No spawns specified.
            if (Spawns.Count == 0)
            {
                SetSpawnUsingPrefab(prefab, out pos, out rot);
                return;
            }

            Transform result = Spawns[_nextSpawn];
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
            if (_nextSpawn >= Spawns.Count)
                _nextSpawn = 0;
        }
        private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            pos = prefab.position;
            rot = prefab.rotation;
        }

}


