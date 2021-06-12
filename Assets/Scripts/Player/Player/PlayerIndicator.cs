using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    [SerializeField]
    private float _indicatorDistance;

    [SerializeField]
    private Transform _inputDirectionIndicator;

    private const float InputDeadZone = 0.04f;
    
    private bool _hasBeenPositioned;
    private bool _moving;
    private PlayerPosition _playerPosition;

    private float _inputAngle;

    /// <summary>
    /// Sets target position to an angle between 0 and 360
    /// </summary>
    /// <param name="angle"></param>
    public void SetPosition(float angle)
    {
        _moving = true;
        if (!_hasBeenPositioned)
        {
            _playerPosition = new PlayerPosition(angle);
        }
        else
        {
            _playerPosition.SetTarget(angle);
        }
        _hasBeenPositioned = true;
    }
    
    #region Input Parameters
    // Input parameters set from PlayerController
    public Vector2 InputDirection { get; set; }
    public float Thrust { get; set; }
    public float Cannons { get; set; }
    #endregion

    #region Unity Methods
    private void Start()
    {
        PlayerManager.RegisterPlayer(this);
        ShipController.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        PlayerManager.RemovePlayer(this);
        ShipController.RemovePlayer(this);
    }

    private void Update()
    {
        if (_moving)
        {
            _playerPosition.Update();

            float angle = _playerPosition.CurrentPosition * Mathf.Deg2Rad;
            transform.localPosition = _indicatorDistance * new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
            
            _moving = _playerPosition.Moving;
        }

        float inputMag = InputDirection.sqrMagnitude;
        if (inputMag > InputDeadZone)
        {
            _inputAngle = Vector2.SignedAngle(InputDirection, Vector2.up);
        }
        
        _inputDirectionIndicator.rotation = Quaternion.AngleAxis(-_inputAngle, Vector3.forward);
    }
    #endregion
}
