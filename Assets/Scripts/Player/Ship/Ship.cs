using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private const float InputDeadZone = 0.04f;
    private const float QuaternionRotationTime = 0.3f;

    [SerializeField] private AnimationCurve _thrustMultiplierCurve;

    private Thruster[] _thrusters;
    private Cannon[] _cannons;
    
    // Input Paramaters
    public Vector2 ThurstVector { get; set; }
    public Vector2 CannonVector { get; set; }
    public float ThrustAllocation { get; set; }
    public float CannonAllocation { get; set; }

    // Rotation Vars
    private float _inputAngle;
    private Quaternion _targetRotation;
    private Quaternion _rotationVel;
    
    // Thrust Vars
    private float _thrustMulti;

    private void Start()
    {
        _thrusters = GetComponentsInChildren<Thruster>();
        _cannons = GetComponentsInChildren<Cannon>();
    }

    private void Update()
    {
        Vector2 forward = transform.rotation * Vector2.up;
        _thrustMulti = _thrustMultiplierCurve.Evaluate(Mathf.Clamp01(Vector2.Dot(forward, ThurstVector)));

        float thrust = Mathf.Clamp01(ThrustAllocation * _thrustMulti);
     
        foreach (var thruster  in _thrusters)
        {
            thruster.Thrust = thrust;
        }
        
        foreach (var cannon  in _cannons)
        {
            cannon.Vector = CannonVector;
            cannon.Strength = CannonAllocation;
        }
    }

    private void FixedUpdate()
    {
        // Rotate towards input direction
        if (ThurstVector.sqrMagnitude > InputDeadZone)
        {
            _inputAngle = Vector2.SignedAngle(ThurstVector, Vector2.up);
        }
        
        _targetRotation = Quaternion.AngleAxis(-_inputAngle, Vector3.forward);

        transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, _targetRotation, ref _rotationVel,
            QuaternionRotationTime);
    }
}
