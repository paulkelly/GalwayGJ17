using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailableGameList : RoomEntryPool
{
    private Dictionary<string, AvailableGameEntry> _roomList = new Dictionary<string, AvailableGameEntry>();
    private bool _joinButtonEnabled = true;

    private void OnEnable()
    {
        NetworkManager.OnGameListUpdated += UpdateRoomList;
        NetworkManager.OnNetworkManagerBusy += NetworkManagerBusy;
        NetworkManager.OnNetworkManagerIdle += NetworkManagerIdle;
    }

    private void OnDisable()
    {
        NetworkManager.OnGameListUpdated -= UpdateRoomList;
        NetworkManager.OnNetworkManagerBusy -= NetworkManagerBusy;
        NetworkManager.OnNetworkManagerIdle -= NetworkManagerIdle;
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        if (!_isInitialised) return;

        List<string> toRemove = new List<string>();
        List<RoomInfo> toAdd = new List<RoomInfo>();
        foreach(RoomInfo room in roomList)
        {
            if(_roomList.ContainsKey(room.Name))
            {
                if (room.RemovedFromList)
                {
                    toRemove.Add(room.Name);
                }
                else
                {
                    _roomList[room.Name].SetPlayerCount(room.PlayerCount, room.MaxPlayers);
                }
            }
            else
            {
                toAdd.Add(room);
            }
        }
        foreach(string roomName in toRemove)
        {
            RemoveGameEntry(roomName);
        }
        foreach(RoomInfo room in toAdd)
        {
            CreateNewGameEntry(room);
        }
    }

    public void NetworkManagerBusy()
    {
        _joinButtonEnabled = false;
        foreach(var room in _roomList.Values)
        {
            room.Active = _joinButtonEnabled;
        }
    }

    public void NetworkManagerIdle()
    {
        _joinButtonEnabled = true;
        foreach (var room in _roomList.Values)
        {
            room.Active = _joinButtonEnabled;
        }
    }

    private void CreateNewGameEntry(RoomInfo roomInfo)
    {
        AvailableGameEntry entry = GetObject();
        entry.Init(roomInfo.Name, roomInfo.PlayerCount, roomInfo.MaxPlayers);
        entry.Active = _joinButtonEnabled;
        _roomList.Add(roomInfo.Name, entry);
    }

    private void RemoveGameEntry(string name)
    {
        _roomList[name].Release();
        _roomList.Remove(name);
    }
}