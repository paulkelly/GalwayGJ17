using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Ship : MonoBehaviourPun, IPunObservable
{
    public const float MaxShield = 100;
    private const float ShieldRechargeRate = 6;
    private const float ShieldDisableTime = 6f;
    private const float InputDeadZone = 0.09f;
    private const float RotationTime = 0.3f;
    private const float AngularVelStopTime = 0.3f;
    private const float SyncPositionTime = 0.3f;
    
    [SerializeField] private float _upperYBounds;
    [SerializeField] private float _lowerYBounds;

    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minAccelerateTime;
    [SerializeField] private float _maxAccelerateTime;
    [SerializeField] private float _deccelerateTime;

    [SerializeField] private Shield _shield;
    [SerializeField] private GameObject _bodyGameObject;
    [SerializeField] private GameObject _followGameObject;
    [SerializeField] private GameObject _destroyedGameObject;
    [SerializeField] private AnimationCurve _thrustAngleMultiplierCurve;
    
    public static Ship Instance { get; private set; }

    private Rigidbody2D _rigidbody;
    private Thruster[] _thrusters;
    private Cannon[] _cannons;
    private RCSThruster[] _rcs;

    public float Shield { get; set; } = MaxShield;
    public bool ShieldDisabled { get; private set; }
    public bool IsDestroyed { get; private set; }
    public Vector2 Position => _rigidbody.position;
    
    // Input Paramaters
    public Vector2 ThurstVector { get; set; }
    public Vector2 CannonVector { get; set; }
    public Vector2 FirstCannonVector { get; set; }
    public Vector2 SecondCannonVector { get; set; }
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
    private Vector2 _syncPositionDampVel;

    private float _shieldDisabledTime;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!ShieldDisabled) return;
        
        Rigidbody2D otherRigidbody = other.rigidbody;
        if (otherRigidbody != null)
        {
            if (_shield.HasNotCollidedRecently(otherRigidbody))
            {
                DestroyShip();
            }
        }
    }

    public void HandleCollision(float mass, float impactVelocity)
    {
        if (ShieldDisabled)
        {
            DestroyShip();
            return;
        }
        
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

    private void DestroyShip()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("DestroyShipRPC", RpcTarget.AllBuffered);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _thrusters = GetComponentsInChildren<Thruster>();
        _cannons = GetComponentsInChildren<Cannon>();
        _rcs = GetComponentsInChildren<RCSThruster>();
        
        ForwardVector = transform.rotation * Vector2.up;
    }

    private void Update()
    {
        if (IsDestroyed) return;
        
        if (ShieldDisabled)
        {
            _shieldDisabledTime += Time.deltaTime;
            if (_shieldDisabledTime > ShieldDisableTime) ShieldDisabled = false;
        }
        ForwardVector = transform.rotation * Vector2.up;
        if (transform.position.y > _upperYBounds) ThurstVector = new Vector2(ThurstVector.x, Mathf.Min(ThurstVector.y, 0));
        if (transform.position.y < _lowerYBounds) ThurstVector = new Vector2(ThurstVector.x, Mathf.Max(ThurstVector.y, 0));
        _thrustMulti = _thrustAngleMultiplierCurve.Evaluate(Mathf.Clamp01(Vector2.Dot(ForwardVector, ThurstVector)));

        _thrust = Mathf.Clamp01(ThrustAllocation * _thrustMulti);
        
        float remainingEnergy = 1 - Mathf.Clamp01(_thrust + CannonAllocation);
        Shield = Mathf.Clamp(Shield + ((remainingEnergy*remainingEnergy) * ShieldRechargeRate * Time.deltaTime), 0 ,MaxShield);

        foreach (var thruster  in _thrusters)
        {
            thruster.Thrust = _thrust;
        }

        foreach (var cannon  in _cannons)
        {
            cannon.ParentSpeed = _velocity;
            //cannon.Vector = CannonVector;
            cannon.Strength = Mathf.Clamp01(CannonAllocation);
        }

        _cannons[0].Vector = FirstCannonVector;
        _cannons[1].Vector = SecondCannonVector;

        EnergyBarUI.UpdateUI(remainingEnergy, _thrust, Mathf.Clamp01(CannonAllocation), Shield/MaxShield, ShieldDisabled);
    }

    private void HandleShipVelocity()
    {
        if (IsDestroyed) return;
        
        if (photonView.IsMine)
        {
            _targetPosition = transform.position;
        }
        else
        {
            _rigidbody.position = Vector2.SmoothDamp(_rigidbody.position, _targetPosition, ref _syncPositionDampVel,
                SyncPositionTime);
        }
        
        Vector2 targetVelocity = (_maxSpeed * _thrust) * ThurstVector;
        bool accelerating = targetVelocity.sqrMagnitude > _velocity.sqrMagnitude;

        float accelerateTime = Mathf.Lerp(_minAccelerateTime, _maxAccelerateTime, _thrust);
        _velocity = Vector2.SmoothDamp(_velocity, targetVelocity, ref _acceleration, accelerating ? accelerateTime : _deccelerateTime);

        _rigidbody.velocity = _velocity;
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
            else if (CannonVector.sqrMagnitude > InputDeadZone && Vector2.Angle(FirstCannonVector, SecondCannonVector) < 45)
            {
                _inputAngle = -Vector2.SignedAngle(CannonVector, Vector2.up);
            }
            else
            {
                _inputAngle = _rigidbody.rotation;
            }
        }
        _rigidbody.angularVelocity = Mathf.SmoothDamp(_rigidbody.angularVelocity, 0, ref _angularDragVel,
            AngularVelStopTime);
        _rigidbody.SetRotation(Mathf.SmoothDampAngle(_rigidbody.rotation, _inputAngle, ref _rotationVel, RotationTime));

        foreach (var rcs in _rcs)
        {
            rcs.Rotation = Mathf.DeltaAngle(_rigidbody.rotation, _inputAngle);
        }
    }

    [PunRPC]
    public void DestroyShipRPC()
    {
        IsDestroyed = true;
        
        _rigidbody.velocity = Vector2.zero;
        _followGameObject.SetActive(false);
        _bodyGameObject.SetActive(false);
        _destroyedGameObject.SetActive(true);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_velocity);
            stream.SendNext(_inputAngle);
            stream.SendNext(Shield);
            stream.SendNext(ShieldDisabled);
        }
        else
        {
            _targetPosition = (Vector3)stream.ReceiveNext();
            _velocity = (Vector2)stream.ReceiveNext();
            _inputAngle = (float)stream.ReceiveNext();
            Shield = (float)stream.ReceiveNext();
            ShieldDisabled = (bool)stream.ReceiveNext();
            
            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            _targetPosition += lag * (Vector3)_velocity;

            if (Vector2.Distance(_targetPosition, _rigidbody.position) > 3)
            {
                _rigidbody.position = _targetPosition;
            }
        }
    }
}
