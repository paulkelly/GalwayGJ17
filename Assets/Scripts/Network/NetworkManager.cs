using System;
using System.Collections;
using System.Collections.Generic;
using Billygoat;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string LobbySceneName = "Lobby";
    private const string MainSceneName = "MainScene";
    #region Inspector Variables
    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 8;

    [SerializeField] 
    private GameObject _playerPrefab;

    [SerializeField] private bool _debug;
    #endregion

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    public static bool IsConnecting { get; private set; }
    public static bool IsOffline { get; private set; }
    public static bool InPublicRoom { get; private set; }
    
    private static NetworkManager _instance;
    private static bool _isBusy;
    private static bool _spawnPlayerWhenReady;

    public static bool IsBusy
    {
        get { return _isBusy; }
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                if (_isBusy)
                {
                    OnNetworkManagerBusy?.Invoke();
                }
                else
                {
                    OnNetworkManagerIdle?.Invoke();
                }
            }
        }
    }
    
    public delegate void NetworkManagerBusy();
    public static event NetworkManagerBusy OnNetworkManagerBusy;
    public delegate void NetworkManagerIdle();
    public static event NetworkManagerIdle OnNetworkManagerIdle;

    public delegate void GameListUpdated(List<RoomInfo> roomList);

    public static event GameListUpdated OnGameListUpdated;

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";

    private List<RoomInfo> _roomInfo;

    private Action _joinRoomCallback;

    #region Unity Methods

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void CreateInstance()
    {
        if (_instance == null)
        {
            NetworkManager obj = FindObjectOfType<NetworkManager>();
            if (obj != null)
            {
                return;
            }
            
            GameObject prefab = Resources.Load<GameObject>("NetworkManager");
            
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = "Network Manager";
            }
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        if (!PhotonNetwork.IsConnected)
        {
            Connect();
        }
        else
        {
            if(PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            if (!PhotonNetwork.OfflineMode && !PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        
        BGSceneLoader.OnSceneLoadComplete += BGSceneLoaderOnOnSceneLoadComplete;
        AvailableGameEntry.OnTryJoinRoom += AvailableGameEntry_OnTryJoinRoom;
    }

    private void OnDestroy()
    {
        BGSceneLoader.OnSceneLoadComplete -= BGSceneLoaderOnOnSceneLoadComplete;
        AvailableGameEntry.OnTryJoinRoom -= AvailableGameEntry_OnTryJoinRoom;
    }

    #endregion

    #region PUN CALLBACKS
    public override void OnConnectedToMaster()
    {
#if DEBUG
        if(_debug) Debug.Log("On connected");
#endif
        if (IsConnecting)
        {
            IsConnecting = false;
        }

        if (_spawnPlayerWhenReady)
        {
            JoinAnyGame();
        }
        else if (!PhotonNetwork.OfflineMode && !PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        
        if (IsConnecting)
        {
            IsConnecting = false;
        }

        if (cause == DisconnectCause.DnsExceptionOnConnect)
        {
#if DEBUG
            if(_debug) Debug.Log("Unable to connect to internet, using offline mode");
#endif
            InPublicRoom = false;
            PhotonNetwork.OfflineMode = true;
            IsOffline = true;
        }
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        base.OnErrorInfo(errorInfo);

#if DEBUG
        if(_debug) Debug.Log("Error: " + errorInfo);
#endif
    }

    public override void OnJoinedRoom()
    {
#if DEBUG
        if(_debug) Debug.Log("Room Joined");
#endif
        IsBusy = false;
        
        SpawnPlayerLocal();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
#if DEBUG
        if(_debug) Debug.Log("Failed to join random room, create a new one");
#endif
        IsBusy = false;
        CreateRoom();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
#if DEBUG
        Debug.Log("Join room failed: " + message);
#endif
        IsBusy = false;
        ReturnToLobbyLocal();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        ReturnToLobbyLocal();   
    }

    public override void OnJoinedLobby()
    {
        UpdateRoomList(new List<RoomInfo>());
    }

    public override void OnLeftLobby()
    {
        UpdateRoomList(new List<RoomInfo>());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _roomInfo = roomList;
        UpdateRoomList();
    }
    #endregion

    public static void SpawnPlayer()
    {
        if (_instance != null)
        {
            _instance.SpawnPlayerLocal();
        }
        else
        {
            _spawnPlayerWhenReady = true;
        }
    }
    public static void ReturnToLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if(_instance != null) _instance.ReturnToLobbyLocal();   
        }
    }
    
    public static void JoinAnyGame()
    {
        if(_instance != null) _instance.TryToJoinRoom();
    }

    public static void HostGame()
    {
        if(_instance != null) _instance.CreateRoom();
    }
    
    public static void StartSoloGame()
    {
        if(_instance != null) _instance.CreatePrivateRoom();
    }

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    private void Connect()
    {
#if DEBUG
        if(_debug) Debug.Log("Connecting to photon");
#endif
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if(!PhotonNetwork.IsConnected)
        {
            IsConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    private bool InMainScene => SceneManager.GetActiveScene().name.Equals(MainSceneName);

    private void TryToJoinRoom()
    {
#if DEBUG
        if(_debug) Debug.Log("Trying to join random room");
#endif
        IsBusy = true;
        InPublicRoom = true;
        if (InMainScene)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            _joinRoomCallback = () =>
            {
                PhotonNetwork.JoinRandomRoom();
            };
            BGSceneLoader.LoadLevel(MainSceneName);   
        }
    }

    private void CreateRoom()
    {
        InPublicRoom = true;
        IsBusy = true;
        if (InMainScene)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }
        else
        {
            _joinRoomCallback = () =>
            {
                PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayersPerRoom});
            };
            BGSceneLoader.LoadLevel(MainSceneName);
        }
    }
    
    private void CreatePrivateRoom()
    {
        InPublicRoom = false;
        IsBusy = true;
        if (InMainScene)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1, IsOpen = false, IsVisible = false});
        }
        else
        {
            _joinRoomCallback = () =>
            {
                PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1, IsOpen = false, IsVisible = false});
            };
            BGSceneLoader.LoadLevel(MainSceneName);
        }
    }

    public void UpdateRoomList()
    {
        if (_roomInfo != null)
        {
            UpdateRoomList(_roomInfo);
        }
        else
        {
            UpdateRoomList(new List<RoomInfo>());
        }
    }
    
    private void UpdateRoomList(List<RoomInfo> info)
    {
        OnGameListUpdated?.Invoke(info);
    }
    
    private void SpawnPlayerLocal()
    {
        if (IsConnecting || !PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            _spawnPlayerWhenReady = true;
            return;
        }
#if DEBUG
        if(_debug) Debug.Log("Spawning Player");
#endif
        PhotonNetwork.Instantiate(this._playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
    }
    
    private void ReturnToLobbyLocal()
    {
        InPublicRoom = false;
        _spawnPlayerWhenReady = false;
        BGSceneLoader.LoadLevel(LobbySceneName);
    }
    
    
    private void AvailableGameEntry_OnTryJoinRoom(string roomName)
    {
        InPublicRoom = true;
        IsBusy = true;
        if (InMainScene)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            _joinRoomCallback = () =>
            {
                PhotonNetwork.JoinRoom(roomName);
            };
            BGSceneLoader.LoadLevel(MainSceneName);
        }
    }
    
    private void BGSceneLoaderOnOnSceneLoadComplete(string level)
    {
        if(level.Equals(LobbySceneName))
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            _joinRoomCallback?.Invoke();
        }
    }
}
