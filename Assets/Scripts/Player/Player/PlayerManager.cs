using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;

    private readonly List<PlayerIndicator> _players = new List<PlayerIndicator>();
    
    

    // Player instances are created by Photon when a player connects
    // When a player spawns, it calls this method
    public static void RegisterPlayer(PlayerIndicator player)
    {
        if (_instance != null)
        {
            _instance._players.Add(player);
            player.transform.parent = _instance.transform;
            _instance.PositionPlayers();
        }
    }

    public static void RemovePlayer(PlayerIndicator player)
    {
        if (_instance != null)
        {
            if (_instance._players.Contains(player))
            {
                _instance._players.Remove(player);
                _instance.PositionPlayers();
            }
        }
    }

    private void PositionPlayers()
    {
        int totalPlayers = _players.Count;
        if (totalPlayers < 1) return;
        for(int i=0; i<totalPlayers; i++)
        {
            _players[i].SetPosition(i*(360f/totalPlayers));
        }
    }

    private void Awake()
    {
        _instance = this;
    }
    
}
