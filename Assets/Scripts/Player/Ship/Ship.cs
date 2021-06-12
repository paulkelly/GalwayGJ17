using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private const float InputDeadZone = 0.04f;
    private const float QuaternionRotationTime = 0.3f;
    
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _accelerateTime;
    [SerializeField] private float _deccelerateTime;

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
    private float _thrust;
    private Vector2 _velocity;
    private Vector2 _acceleration;

    private void Start()
    {
        _thrusters = GetComponentsInChildren<Thruster>();
        _cannons = GetComponentsInChildren<Cannon>();
    }

    private void Update()
    {
        Vector2 forward = transform.rotation * Vector2.up;
        _thrustMulti = _thrustMultiplierCurve.Evaluate(Mathf.Clamp01(Vector2.Dot(forward, ThurstVector)));

        _thrust = Mathf.Clamp01(ThrustAllocation * _thrustMulti);
     
        foreach (var thruster  in _thrusters)
        {
            thruster.Thrust = _thrust;
        }
        
        HandleShipVelocity();
        
        foreach (var cannon  in _cannons)
        {
            cannon.ParentSpeed = _velocity;
            cannon.Vector = CannonVector;
            cannon.Strength = CannonAllocation;
        }
    }

    private void HandleShipVelocity()
    {
        // Vector2 thrustVector = (_acceleration * _thrust * Time.deltaTime) * ThurstVector;
        // _velocity = Vector2.ClampMagnitude(_velocity + thrustVector, _maxSpeed);
        Vector2 targetVelocity = (_maxSpeed * _thrust) * ThurstVector;
        bool accelerating = Vector2.Dot(targetVelocity, _velocity) > 0 &&
                            targetVelocity.sqrMagnitude > _velocity.sqrMagnitude;

        _velocity = Vector2.SmoothDamp(_velocity, targetVelocity, ref _acceleration, accelerating ? _accelerateTime : _deccelerateTime);

        Vector2 moveVector = Time.deltaTime * _velocity;
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x + moveVector.x, position.y + moveVector.y,
            position.z);
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
