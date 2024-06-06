using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEditor;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    public bool Toggle;
    [SerializeField] private ConnectionType _connectionType;
    private NetworkManager _networkManager;
    
    private Tugboat _tugboat;
    
    private void Awake()
    {
        if(!Toggle) return;
        
        _networkManager = InstanceFinder.NetworkManager;
        if (TryGetComponent(out Tugboat tug))
            _tugboat = tug;
        else
        {
            Debug.LogError("Tugboat not found", gameObject);
            return;
        }
#if UNITY_EDITOR
        if (_connectionType == ConnectionType.Host)
        {
            if (ParrelSync.ClonesManager.IsClone())
            {
                _tugboat.StartConnection(false);
            }
            else
            {
                _tugboat.StartConnection(true);
                _tugboat.StartConnection(false);
            }

            return;
        }
        
        _tugboat.StartConnection(false);
        
#endif
#if !UNITY_EDITOR
        _tugboat.StartConnection(true);
#endif
    }

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {

#if UNITY_EDITOR
        if (args.ConnectionState == LocalConnectionState.Stopping)
            EditorApplication.isPlaying = false;
#endif
        
        //BootstrapManager.Bootstrap_LoadScene("TestConnect"); //test only
    }
    
    public enum ConnectionType
    {
        Host,
        Client
    }
}
