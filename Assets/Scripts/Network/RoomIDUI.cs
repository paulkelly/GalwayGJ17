using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomIDUI : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject _roomCodeObj;

    [SerializeField]
    private TMP_Text Text;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            SetRoomCode(PhotonNetwork.CurrentRoom.Name);
        }
        else
        {
            SetRoomCode(string.Empty);
        }
    }

    private void Update()
    {
        if(_roomCodeObj.activeSelf != NetworkManager.InPublicRoom)
        {
            _roomCodeObj.SetActive(NetworkManager.InPublicRoom);
        }
    }

    public override void OnJoinedRoom()
    {
        SetRoomCode(PhotonNetwork.CurrentRoom.Name);
    }

    private void SetRoomCode(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Text.text = string.Empty;
        }
        else
        {
            Text.text = GameNameHash.GetRoomCode(roomName);
        }
    }
}