using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
    {
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
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;

        #region Unity Methods
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (!PhotonNetwork.IsConnected)
            {
                Connect();
            }
            else
            {
                SpawnPlayer();
            }
        }

        #endregion

        #region PUN CALLBACKS
        public override void OnConnectedToMaster()
        {
#if DEBUG
            if(_debug) Debug.Log("On connected");
#endif
            if (isConnecting)
            {
                isConnecting = false;
            }

            if (!PhotonNetwork.OfflineMode && !PhotonNetwork.InLobby)
            {
                TryToJoinRoom();
            }
            else
            {
                SpawnPlayer();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            if (cause == DisconnectCause.DnsExceptionOnConnect)
            {
#if DEBUG
                if(_debug) Debug.Log("Unable to connect to internet, using offline mode");
#endif
                PhotonNetwork.OfflineMode = true;
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
            SpawnPlayer();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
#if DEBUG
            if(_debug) Debug.Log("Failed to join random room, create a new one");
#endif
            CreateRoom();
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
            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonNetwork.InRoom)
                {
#if DEBUG
                    if(_debug) Debug.Log("Already connected but not in a room, try to join room");
#endif
                    TryToJoinRoom();
                }
            }
            else
            {
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        private void TryToJoinRoom()
        {
#if DEBUG
            if(_debug) Debug.Log("Trying to join random room");
#endif
            PhotonNetwork.JoinRandomRoom();
        }

        private void CreateRoom()
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        private void SpawnPlayer()
        {
#if DEBUG
            if(_debug) Debug.Log("Spawning Player");
#endif
            PhotonNetwork.Instantiate(this._playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }
        #endregion
    }
