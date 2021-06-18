using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class AvailableGameEntry : PooledObject
{
    public TMP_Text RoomName;
    public TMP_Text PlayerCount;
    public Button Button;

    private string _roomName;

    public delegate void TryJoinRoom(string roomName);
    public static event TryJoinRoom OnTryJoinRoom;

    public void Init(string roomName, int currentPlayers, int maxPlayers)
    {
        _roomName = roomName;
        RoomName.text = GameNameHash.GetRoomCode(_roomName);
        SetPlayerCount(currentPlayers, maxPlayers);
    }

    public void SetPlayerCount(int currentPlayers, int maxPlayers)
    {
        PlayerCount.text = string.Format("{0}/{1}", currentPlayers, maxPlayers);
    }

    public void OnJoin()
    {
        OnTryJoinRoom?.Invoke(_roomName);
    }

    public bool Active
    {
        get { return Button.IsInteractable(); }
        set { Button.interactable = value; }
    }
}
