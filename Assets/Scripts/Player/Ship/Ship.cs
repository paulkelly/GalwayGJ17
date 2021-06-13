using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;

public class Ship : MonoBehaviourPun, IPunObservable
{
    private const float InputDeadZone = 0.04f;
    private const float QuaternionRotationTime = 0.3f;

    [SerializeField] private Transform _body;
    
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
    // Thrust vector defaults to ForwardVector if button pressed with stick input
    public Vector2 ForwardVector { get; private set; }

    // Rotation Vars
    private float _inputAngle;
    private Quaternion _targetRotation;
    private Quaternion _rotationVel;
    
    // Thrust Vars
    private float _thrustMulti;
    private float _thrust;
    private Vector2 _velocity;
    private Vector2 _acceleration;
    // Only used for non host players
    private Vector3 _targetPosition;

    private void Start()
    {
        _thrusters = GetComponentsInChildren<Thruster>();
        _cannons = GetComponentsInChildren<Cannon>();
        
        ForwardVector = _body.rotation * Vector2.up;
    }

    private void Update()
    {
        ForwardVector = _body.rotation * Vector2.up;
        _thrustMulti = _thrustMultiplierCurve.Evaluate(Mathf.Clamp01(Vector2.Dot(ForwardVector, ThurstVector)));

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
        if (photonView.IsMine)
        {
            _targetPosition = transform.position;
        }
        
        Vector2 targetVelocity = (_maxSpeed * _thrust) * ThurstVector;
        bool accelerating = Vector2.Dot(targetVelocity, _velocity) > 0 &&
                            targetVelocity.sqrMagnitude > _velocity.sqrMagnitude;

        _velocity = Vector2.SmoothDamp(_velocity, targetVelocity, ref _acceleration, accelerating ? _accelerateTime : _deccelerateTime);

        Vector2 moveVector = Time.deltaTime * _velocity;
        Vector3 position = Vector3.Lerp(transform.position, _targetPosition, 0.1f);
        transform.position = new Vector3(position.x + moveVector.x, position.y + moveVector.y,
            position.z);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            _body.rotation = QuaternionUtil.SmoothDamp(_body.rotation, _targetRotation, ref _rotationVel,
                QuaternionRotationTime);
        }
        
        // Rotate towards input direction
        if (ThurstVector.sqrMagnitude > InputDeadZone)
        {
            _inputAngle = Vector2.SignedAngle(ThurstVector, Vector2.up);
        }
        
        _targetRotation = Quaternion.AngleAxis(-_inputAngle, Vector3.forward);

        _body.rotation = QuaternionUtil.SmoothDamp(_body.rotation, _targetRotation, ref _rotationVel,
            QuaternionRotationTime);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_velocity);
            stream.SendNext(_targetRotation);
        }
        else
        {
            _targetPosition = (Vector3)stream.ReceiveNext();
            _velocity = (Vector2)stream.ReceiveNext();
            _targetRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
