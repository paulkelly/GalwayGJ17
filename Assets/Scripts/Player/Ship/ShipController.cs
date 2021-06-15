using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private Ship _ship;

    private const float InputDeadZone = 0.2f;
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

        float totalInputs = 0;
        float thrustingPlayers = 0;
        float cannoningPlayers = 0;
        Vector2 thrustVector = Vector2.zero;
        Vector2 cannonVector = Vector2.zero;

        foreach (var player in _players)
        {
            if (player.Thrust > ThrustCannonThresholds)
            {
                float magnitude = player.InputDirection.magnitude;
                if (magnitude > InputDeadZone)
                {
                    thrustVector += player.InputDirection;
                    magnitude *= player.Thrust;
                }
                else
                {
                    thrustVector += _ship.ForwardVector;
                    magnitude = player.Thrust;
                }

                if (player.Cannons > ThrustCannonThresholds) magnitude *= 0.5f;
                thrustingPlayers +=magnitude;
            }
            if (player.Cannons > ThrustCannonThresholds)
            {
                cannonVector += player.InputDirection * player.Cannons;
                float magnitude = player.Cannons;
                if (player.Thrust > ThrustCannonThresholds) magnitude *= 0.5f;
                cannoningPlayers += magnitude;
            }
            
            totalInputs++;
        }

        if (totalInputs > 0)
        {
            _ship.ThurstVector = thrustVector.normalized;
            _ship.ThrustAllocation = thrustingPlayers/totalInputs;
            
            _ship.CannonVector = cannonVector.normalized;
            _ship.CannonAllocation = cannoningPlayers/totalInputs;
        }
        else
        {
            _ship.ThrustAllocation = 0;
            _ship.CannonAllocation = 0;
            _ship.ThurstVector = Vector2.zero;
            _ship.CannonVector = Vector2.zero;
        }
    }
}
