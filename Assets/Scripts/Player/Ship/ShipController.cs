using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private Ship _ship;

    private const float ThrustCannonThresholds = 0.3f;
    private static ShipController _instance;

    private readonly List<PlayerIndicator> _players = new List<PlayerIndicator>();
    public static void RegisterPlayer(PlayerIndicator player)
    {
        if (_instance != null)
        {
            _instance._players.Add(player);
        }
    }

    public static void RemovePlayer(PlayerIndicator player)
    {
        if (_instance != null)
        {
            if (_instance._players.Contains(player))
            {
                _instance._players.Remove(player);
            }
        }
    }
    
    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {

        int totalPlayer = _players.Count;
        int thrustingPlayers = 0;
        int cannoningPlayers = 0;
        Vector2 thrustVector = Vector2.zero;
        Vector2 cannonVector = Vector2.zero;

        foreach (var player in _players)
        {
            if (player.Thrust > ThrustCannonThresholds)
            {
                thrustVector += player.InputDirection * player.Thrust;
                thrustingPlayers++;
            }
            if (player.Cannons > ThrustCannonThresholds)
            {
                cannonVector += player.InputDirection * player.Cannons;
                cannoningPlayers++;
            }
        }

        if (totalPlayer > 0)
        {
            _ship.ThurstVector = thrustVector / totalPlayer;
        }
        else
        {
            _ship.ThurstVector = Vector2.zero;
        }

        if (totalPlayer > 0)
        {
            _ship.CannonVector = cannonVector / totalPlayer;
        }
        else
        {
            _ship.CannonVector = Vector2.zero;
        }

        _ship.ThrustAllocation = thrustVector.magnitude;
        _ship.CannonAllocation = cannonVector.magnitude;
    }
}
