using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;

public class Ship : MonoBehaviourPun, IPunObservable
{
    public const float MaxShield = 100;
    private const float ShieldRechargeRate = 10;
    private const float ShieldDisableTime = 1f;
    private const float InputDeadZone = 0.04f;
    private const float RotationTime = 0.3f;
    private const float AngularVelStopTime = 0.3f;

    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _accelerateTime;
    [SerializeField] private float _deccelerateTime;

    [SerializeField] private AnimationCurve _thrustMultiplierCurve;

    private Rigidbody2D _rigidbody;
    private Thruster[] _thrusters;
    private Cannon[] _cannons;

    public float Shield { get; set; } = MaxShield;
    public bool ShieldDisabled { get; private set; }
    
    // Input Paramaters
    public Vector2 ThurstVector { get; set; }
    public Vector2 CannonVector { get; set; }
    public float ThrustAllocation { get; set; }
    public float CannonAllocation { get; set; }
    // Thrust vector defaults to ForwardVector if button pressed with stick input
    public Vector2 ForwardVector { get; private set; }

    // Rotation Vars
    private float _inputAngle;
    private float _rotationVel;
    private float _angularDragVel;
    // private Quaternion _targetRotation;
    // private Quaternion _rotationVel;
    
    // Thrust Vars
    private float _thrustMulti;
    private float _thrust;
    private Vector2 _velocity;
    private Vector2 _acceleration;
    // Only used for non host players
    private Vector3 _targetPosition;

    private float _shieldDisabledTime;
    
    public void HandleCollision(float mass, float impactVelocity)
    {
        float damage = DamageUtil.GetDamage(mass, impactVelocity);
        if (Shield < damage)
        {
            Shield = 0;
            ShieldDisabled = true;
            _shieldDisabledTime = 0;
        }
        else
        {
            Shield -= damage;
        }

    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _thrusters = GetComponentsInChildren<Thruster>();
        _cannons = GetComponentsInChildren<Cannon>();
        
        ForwardVector = transform.rotation * Vector2.up;
    }

    private void Update()
    {
        if (ShieldDisabled)
        {
            _shieldDisabledTime += Time.deltaTime;
            if (_shieldDisabledTime > ShieldDisableTime) ShieldDisabled = false;
        }
        ForwardVector = transform.rotation * Vector2.up;
        _thrustMulti = _thrustMultiplierCurve.Evaluate(Mathf.Clamp01(Vector2.Dot(ForwardVector, ThurstVector)));

        _thrust = Mathf.Clamp01(ThrustAllocation * _thrustMulti);
     
        foreach (var thruster  in _thrusters)
        {
            thruster.Thrust = _thrust;
        }

        foreach (var cannon  in _cannons)
        {
            cannon.ParentSpeed = _velocity;
            cannon.Vector = CannonVector;
            cannon.Strength = Mathf.Clamp01(CannonAllocation);
        }

        float remainingEnergy = 1 - Mathf.Clamp01(_thrust + CannonAllocation);
        Shield = Mathf.Clamp(Shield + ((remainingEnergy*remainingEnergy) * ShieldRechargeRate * Time.deltaTime), 0 ,MaxShield);
        
        EnergyBarUI.UpdateUI(remainingEnergy, _thrust, Mathf.Clamp01(CannonAllocation), Shield/MaxShield, ShieldDisabled);
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

        _rigidbody.velocity = _velocity;
        // Vector2 moveVector = Time.deltaTime * _velocity;
        // Vector3 position = Vector3.Lerp(transform.position, _targetPosition, 0.1f);
        // transform.position = new Vector3(position.x + moveVector.x, position.y + moveVector.y,
        //     position.z);
    }

    private void FixedUpdate()
    {
        HandleShipVelocity();

        if (photonView.IsMine)
        {
            // Rotate towards input direction
            if (ThurstVector.sqrMagnitude > InputDeadZone)
            {
                _inputAngle = -Vector2.SignedAngle(ThurstVector, Vector2.up);
            }
            else
            {
                _inputAngle = _rigidbody.rotation;
            }
        }
        _rigidbody.angularVelocity = Mathf.SmoothDamp(_rigidbody.angularVelocity, 0, ref _angularDragVel,
            AngularVelStopTime);
        _rigidbody.SetRotation(Mathf.SmoothDampAngle(_rigidbody.rotation, _inputAngle, ref _rotationVel, RotationTime));
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_velocity);
            stream.SendNext(_inputAngle);
        }
        else
        {
            _targetPosition = (Vector3)stream.ReceiveNext();
            _velocity = (Vector2)stream.ReceiveNext();
            _inputAngle = (float)stream.ReceiveNext();
        }
    }
}
