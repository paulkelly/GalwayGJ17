using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private Ship _ship;

    private const float InputDeadZone = 0.04f;
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

        int totalPlayers = _players.Count;
        int thrustingPlayers = 0;
        int cannoningPlayers = 0;
        Vector2 thrustVector = Vector2.zero;
        Vector2 cannonVector = Vector2.zero;

        foreach (var player in _players)
        {
            if (player.Thrust > ThrustCannonThresholds)
            {
                if (player.InputDirection.sqrMagnitude > InputDeadZone)
                {
                    thrustVector += player.InputDirection * player.Thrust;
                }
                else
                {
                    thrustVector += _ship.ForwardVector;
                }

                thrustingPlayers++;
            }
            if (player.Cannons > ThrustCannonThresholds)
            {
                cannonVector += player.InputDirection * player.Cannons;
                cannoningPlayers++;
            }
        }

        if (totalPlayers > 0)
        {
            _ship.ThurstVector = thrustVector / totalPlayers;
            _ship.ThrustAllocation = (float)thrustingPlayers/totalPlayers;
            
            _ship.CannonVector = cannonVector / totalPlayers;
            _ship.CannonAllocation = (float)cannoningPlayers/totalPlayers;
        }
        else
        {
            _ship.ThurstVector = Vector2.zero;
            _ship.CannonVector = Vector2.zero;
        }
    }
}
