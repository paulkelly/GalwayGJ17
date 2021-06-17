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
    private readonly List<Vector2> _cannonInputsToProcess = new List<Vector2>();
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

        _cannonInputsToProcess.Clear();
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
                Vector2 normalisedCannonVector = (player.InputDirection.normalized) * player.Cannons;
                _cannonInputsToProcess.Add(normalisedCannonVector);
                cannonVector += normalisedCannonVector;
                float magnitude = player.Cannons;
                if (player.Thrust > ThrustCannonThresholds) magnitude *= 0.5f;
                cannoningPlayers += magnitude;
            }
            
            totalInputs++;
        }

        if (totalInputs > 0)
        {
            ProcessedCannonInputs processedInputs = ProcessCannonInputs(cannonVector);
            
            _ship.ThurstVector = thrustVector.normalized;
            _ship.ThrustAllocation = thrustingPlayers/totalInputs;
            
            _ship.CannonVector = cannonVector.normalized;
            _ship.CannonAllocation = cannoningPlayers/totalInputs;

            _ship.FirstCannonVector = processedInputs.First;
            _ship.SecondCannonVector = processedInputs.Second;
        }
        else
        {
            _ship.ThrustAllocation = 0;
            _ship.CannonAllocation = 0;
            _ship.ThurstVector = Vector2.zero;
            _ship.CannonVector = Vector2.zero;
            _ship.FirstCannonVector = Vector2.zero;
            _ship.SecondCannonVector = Vector2.zero;
        }
    }

    private ProcessedCannonInputs ProcessCannonInputs(Vector2 averageInput)
    {
        if (_cannonInputsToProcess.Count == 0)
        {
            return new ProcessedCannonInputs() {First = Vector2.zero, Second = Vector2.zero};
        }
        if (_cannonInputsToProcess.Count == 1)
        {
            return new ProcessedCannonInputs() {First = _cannonInputsToProcess[0], Second = _cannonInputsToProcess[0]};
        }
        if (_cannonInputsToProcess.Count == 2)
        {
            return new ProcessedCannonInputs() {First = _cannonInputsToProcess[0], Second = _cannonInputsToProcess[1]};
        }
        
        Vector2 first = Vector2.zero;
        Vector2 second = Vector2.zero;

        foreach (var input in _cannonInputsToProcess)
        {
            if (Mathf.Abs(Vector2.Angle(averageInput, input)) < 90)
            {
                first += input;
            }
            else
            {
                second += input;
            }
        }

        first /= _cannonInputsToProcess.Count;
        second /= _cannonInputsToProcess.Count;
        
        return new ProcessedCannonInputs() {First = first, Second = second};
    }
}

public struct ProcessedCannonInputs
{
    public Vector2 First;
    public Vector2 Second;
}
